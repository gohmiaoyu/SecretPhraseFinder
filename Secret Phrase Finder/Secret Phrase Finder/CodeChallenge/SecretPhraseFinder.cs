using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BackendDeveloperCodeChallenge
{
    using ExtensionMethods;
    using System.Threading.Tasks;

    public static class SecretPhraseFinder
    {
        public static string FindSecretPhrase(string anagram, string secretPhraseMd5Hash, ReadOnlyCollection<string> allowedWords)
        {
            ReadOnlyDictionary<char, int> anagramCharCounts = anagram.Where(character => !Char.IsWhiteSpace(character))
                                                                     .GroupBy(character => character)
                                                                     .ToDictionary(keySelector: group => group.Key,
                                                                                   elementSelector: group => group.Count())
                                                                     .ToReadOnly();
            int numCharsInAnagram = anagramCharCounts.Values.Sum();

            ReadOnlyCollection<string> wordsPossiblyInAnagram = FindSubsetWordsOfAnagram(anagram, allowedWords);
            ReadOnlyDictionary<int, string[]> wordCandidatesGroupedByLength = wordsPossiblyInAnagram.GroupBy(word => word.Length)
                                                                                                    .ToDictionary(keySelector: group => group.Key,
                                                                                                                  elementSelector: group => group.ToArray())
                                                                                                    .ToReadOnly();

            // We start by searching through all one-word sentences containing all the characters appearing in the secret phrase. 
            // If the secret phrase is not found, we proceed to search through all two-word sentences, followed by all three-word sentences, etc.,
            // until we find the secret phrase, or exhaust the entire search space.
            int currentNumWordsInSentence = 1;
            bool foundSecretPhrase = false;
            string secretPhrase = null;

            while (!foundSecretPhrase)
            {
                foundSecretPhrase = TryFindSecretPhrase(currentNumWordsInSentence,
                                                        numCharsInAnagram,
                                                        anagramCharCounts,
                                                        wordCandidatesGroupedByLength,
                                                        secretPhraseMd5Hash,
                                                        out secretPhrase);
                currentNumWordsInSentence++;
            }

            return secretPhrase;
        }

        private static ReadOnlyCollection<string> FindSubsetWordsOfAnagram(string anagram, ReadOnlyCollection<string> allowedWords)
        {
            return allowedWords.Select(word => word.Trim())
                               .Where(word => word.Length > 0 && word.IsSubsetOf(anagram))
                               .Distinct()
                               .ToReadOnly();
        }

        private static bool TryFindSecretPhrase(int numWordsInSentence,
                                                int numCharactersInAnagram,
                                                ReadOnlyDictionary<char, int> anagramCharCounts,
                                                ReadOnlyDictionary<int, string[]> wordCandidatesGroupedByLength,
                                                string secretPhraseMd5Hash,
                                                out string secretPhrase)
        {
            ReadOnlyCollection<ReadOnlyCollection<int>> wordLengthCombinations = GenerateWordLengthCombinations(numCharactersInAnagram, numWordsInSentence);

            string foundPhrase = null; // Alas, closures don't allow out parameters...

            Action<ReadOnlyCollection<int>, ParallelLoopState> TryFindPhrase = (wordLengthCombination, state) =>
            {
                if (wordLengthCombination.Any(wordLength => !wordCandidatesGroupedByLength.ContainsKey(wordLength)))
                {
                    return; // Failure
                }

                var initialCharacters = new CharacterPool(anagramCharCounts);

                LinkedList<string> sentences = FindSentencesWithWordLengths(wordLengthCombination, initialCharacters, wordCandidatesGroupedByLength);

                if (sentences.Any())
                {
                    foreach (string sentence in sentences)
                    {
                        IEnumerable<IEnumerable<string>> permutationsOfSentence = Math.FindPermutations(sentence.Split(), numWordsInSentence);

                        // Lazily traverse through all permutations of the sentence and exit immediately if we find the secret phrase.
                        foreach (IEnumerable<string> permutation in permutationsOfSentence)
                        {
                            if (state.IsStopped)
                            {
                                return; // Another task found the secret phrase
                            }

                            string permutationString = String.Join(" ", permutation.ToArray());
                            string candidateMd5Hash = permutationString.ComputeMD5Hash().ConvertToHexString(useLowerCase: true);

                            if (candidateMd5Hash == secretPhraseMd5Hash)
                            {
                                foundPhrase = permutationString;
                                state.Stop();

                                return; // Success
                            }
                        }
                    }
                }
            };

            Parallel.ForEach(wordLengthCombinations, TryFindPhrase);
            secretPhrase = foundPhrase;

            return secretPhrase != null;
        }

        private static ReadOnlyCollection<ReadOnlyCollection<int>> GenerateWordLengthCombinations(int numCharsInSentence, int numWordsInSentence)
        {
            return Math.FindSetsOfNumbersAddingUpTo(targetSum: numCharsInSentence, setSize: numWordsInSentence)
                       .Select(wordLengths => wordLengths.OrderByDescending(wordLength => wordLength).ToReadOnly())
                       .ToReadOnly();
        }

        private static LinkedList<string> FindSentencesWithWordLengths(ReadOnlyCollection<int> requiredWordLengths, 
                                                                       CharacterPool initialCharacters, 
                                                                       ReadOnlyDictionary<int, string[]> allowedWordsGroupedByLength)
        {
            var foundSentences = new LinkedList<string>();
            string seedSentence = String.Empty;

            const int IndexOfFirstWord = 0;

            int lengthOfFirstWord = requiredWordLengths[IndexOfFirstWord];

            string[] candidatesForFirstWord;
            bool candidatesExistForFirstWord = allowedWordsGroupedByLength.TryGetValue(lengthOfFirstWord, out candidatesForFirstWord);

            if (candidatesExistForFirstWord)
            {
                foreach (string wordCandidate in candidatesForFirstWord)
                {
                    FindSentencesBeginningWith(wordCandidate, 
                                               seedSentence, 
                                               requiredWordLengths, 
                                               IndexOfFirstWord, 
                                               initialCharacters, 
                                               allowedWordsGroupedByLength, 
                                               foundSentences);
                }
            }

            return foundSentences;
        }

        private static void FindSentencesBeginningWith(string word, 
                                                       string sentenceSoFar, 
                                                       ReadOnlyCollection<int> lengthsOfWordsInSentence, 
                                                       int currentWordIndexInSentence, 
                                                       CharacterPool remainingCharacters, 
                                                       ReadOnlyDictionary<int, string[]> allowedWordsGroupedByLength, 
                                                       LinkedList<string> allFoundSentences)
        {
            string updatedSentence = null;

            if (remainingCharacters.TryRemoveAllCharacters(word))
            {
                updatedSentence = sentenceSoFar == String.Empty ? word : sentenceSoFar + " " + word;

                if (remainingCharacters.NumCharsLeft == 0)
                {
                    allFoundSentences.AddLast(updatedSentence);
                }
                else
                {
                    int nextWordIndexInSentence = currentWordIndexInSentence + 1;
                    int lengthOfNextWord = lengthsOfWordsInSentence[nextWordIndexInSentence];

                    string[] candidatesForNextWord;
                    bool candidatesExistForNextWord = allowedWordsGroupedByLength.TryGetValue(lengthOfNextWord, out candidatesForNextWord);

                    if (candidatesExistForNextWord)
                    {
                        foreach (string wordCandidate in candidatesForNextWord)
                        {
                            FindSentencesBeginningWith(wordCandidate, 
                                                       updatedSentence, 
                                                       lengthsOfWordsInSentence, 
                                                       nextWordIndexInSentence, 
                                                       remainingCharacters, 
                                                       allowedWordsGroupedByLength, 
                                                       allFoundSentences);
                        }
                    }
                }

                remainingCharacters.AddCharacters(word);
            }
        }

        private static string ConvertToHexString(this byte[] hashedInputByteArray, bool useLowerCase)
        {
            var hexResult = new StringBuilder(hashedInputByteArray.Length * 2);

            for (int i = 0; i < hashedInputByteArray.Length; i++)
            {
                hexResult.Append(hashedInputByteArray[i].ToString(useLowerCase ? "x2" : "X2"));
            }

            return hexResult.ToString();
        }

        /// <summary>
        /// Stores characters and their counts, and allows for removal and insertion of characters significantly faster than would be the case for a regular dictionary
        /// </summary>
        private class CharacterPool
        {
            private readonly int lowestAsciiValue;
            private readonly int[] characterCounts;
            private readonly int length;
            private int currentTotalCount;

            public CharacterPool(IDictionary<char, int> characterCounts)
            {
                this.lowestAsciiValue = characterCounts.Keys.Min();
                this.length = characterCounts.Keys.Max() - lowestAsciiValue + 1;
                this.characterCounts = new int[this.length];
                this.currentTotalCount = characterCounts.Values.Sum();

                foreach (KeyValuePair<char, int> charCount in characterCounts)
                {
                    this.characterCounts[charCount.Key - this.lowestAsciiValue] = charCount.Value;
                }
            }

            public int NumCharsLeft { get { return this.currentTotalCount; } }

            public bool TryRemoveAllCharacters(string characters)
            {
                int numCharsToRemove = characters.Length;

                if (numCharsToRemove > this.currentTotalCount)
                {
                    return false;
                }

                int numCharsRemoved = 0;

                while (numCharsRemoved < numCharsToRemove)
                {
                    int indexInCounts = characters[numCharsRemoved] - this.lowestAsciiValue;

                    if (indexInCounts < 0 || indexInCounts >= this.length)
                    {
                        break; // Failure
                    }

                    if (this.characterCounts[indexInCounts] > 0)
                    {
                        this.characterCounts[indexInCounts]--;
                    }
                    else
                    {
                        break; // Failure
                    }

                    numCharsRemoved++;
                }

                this.currentTotalCount -= numCharsRemoved;

                if (numCharsRemoved < numCharsToRemove)
                {
                    this.AddCharacters(characters, numCharsToAdd: numCharsRemoved); // Since we failed to remove all characters, add the removed ones back.
                    return false;
                }

                return true;
            }

            public void AddCharacters(string characters)
            {
                this.AddCharacters(characters, characters.Length);
            }

            private void AddCharacters(string characters, int numCharsToAdd)
            {
                int numCharsAdded = 0;

                while (numCharsAdded < numCharsToAdd)
                {
                    int charCountIndex = characters[numCharsAdded] - this.lowestAsciiValue;
                    this.characterCounts[charCountIndex]++;

                    numCharsAdded++;
                }

                this.currentTotalCount += numCharsAdded;
            }
        }
    }
}