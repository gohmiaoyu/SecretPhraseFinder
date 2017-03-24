using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace BackendDeveloperCodeChallenge
{
    using ExtensionMethods;
    using static SecretPhraseFinder;

    /// <summary>
    /// This program was written to solve a backend developer challenge designed by a Danish software company.
    /// The challenge consists of finding three different secret phrases, given an anagram, the MD5 hashes of the secret phrases, 
    /// and a list of nearly 100,000 words that are allowed to be in the secret phrases.
    /// On a typical desktop computer in 2017, the three phrases take ~0.1 seconds, ~0.05 seconds and ~2 seconds respectively to be found --
    /// see the screenshot "Found Secret Phrases.PNG" in the folder "CodeChallenge".
    /// </summary>
    class Program
    {
        static void Main(string[] args) {
            const string allowedWordsFileName = "AllowedWords.txt";

            var allowedWordsDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent; // Move up two levels from the output folder
            string allowedWordsFilePath = Path.Combine(allowedWordsDirectory.FullName, allowedWordsFileName);

            ReadOnlyCollection<string> allowedWords = File.ReadAllLines(allowedWordsFilePath).ToReadOnly();

            const string anagram = "poultry outwits ants";
            var md5hashesOfSecretPhrases = Array.AsReadOnly(new[] { "e4820b45d2277f3844eac66c903e84be",
                                                                    "23170acc097c24edb98fc5488ab033fe",
                                                                    "665e5bcb0c20062fe8abaaf4628bb154" });

            var stopwatch = new Stopwatch();

            foreach (var md5hash in md5hashesOfSecretPhrases) {
                stopwatch.Start();
                string secretPhrase = FindSecretPhrase(anagram, md5hash, allowedWords);
                stopwatch.Stop();

                Console.WriteLine("The secret phrase '" + secretPhrase + "' with MD5 hash '" + md5hash + "' was found in " + stopwatch.Elapsed.TotalSeconds + " seconds.");
                stopwatch.Reset();
            }
            
            Console.WriteLine("Press any key to exit the program: ");
            Console.ReadKey();
        }
    }
}
