using System.Collections.Generic;
using System.Linq;

namespace BackendDeveloperCodeChallenge
{
    public static class Math
    {
        /// <summary>
        /// This method finds all natural numbers adding up to a target sum.
        /// </summary>
        /// <remarks>Let's say we want to find all sets of three natural numbers that add up to 10. 
        /// <paramref name="targetSum"/> would then be 10, whereas <paramref name="setSize"/> would be 3.</remarks>
        public static IEnumerable<IEnumerable<int>> FindSetsOfNumbersAddingUpTo(int targetSum, int setSize)
        {
            if (setSize == 1)
            {
                return new int[1][] { new int[1] { targetSum } };
            }

            // In our example, setSize is 3, and targetSum is 10. We begin by creating an IEnumerable collection { 1, 2, 3 }.
            // Observe that if we are considering pairs of numbers that add up to 10, the largest number possible in any such pair is 9 (9 + 1 = 10).
            // If we are considering sets of three numbers that add up to 10, the largest number possible in any such set is 8 (8 + 1 + 1 = 10).
            // We can therefore make the generalization that the largest number possible in a set of numbers adding up to a target sum is targetSum - setSize + 1.
            IEnumerable<IEnumerable<int>> combinations = Enumerable.Range(1, setSize)
                                                                   .Select(size => Enumerable.Range(1, targetSum - setSize + 1))
                                                                   .CartesianProduct();

            // We are only interested in sets containing elements that add up to our targetSum, so let's filter out the rest.
            combinations = combinations.Where(combination => combination.Sum(element => element) == targetSum);

            // Make sure there are no duplicates! { 3, 3, 4 } is the same as { 4, 3, 3 } for our purposes, because addition is commutative.
            return combinations.Distinct(new CombinationsComparer()).ToArray();
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(emptyProduct, (accumulator, sequence) =>
                             from accumulatedSoFar in accumulator
                             from unaddedItem in sequence
                             select accumulatedSoFar.Concat(new[] { unaddedItem }));
        }

        public static IEnumerable<IEnumerable<T>> FindPermutations<T>(this IEnumerable<T> inputs, int desiredSetSize)
        {
            if (desiredSetSize == 1)
            {
                return inputs.Select(element => new T[] { element });
            }

            return FindPermutations(inputs, desiredSetSize - 1).SelectMany(collectionSelector: shorterPermutation => inputs.Except(shorterPermutation),
                                                                           resultSelector: (shorterPermutation, newElement) => shorterPermutation.Concat(new T[] { newElement }));
        }

        public class CombinationsComparer : IEqualityComparer<IEnumerable<int>>
        {
            public bool Equals(IEnumerable<int> x, IEnumerable<int> y)
            {
                return x.OrderBy(a => a)
                        .SequenceEqual(y.OrderBy(b => b));
            }

            public int GetHashCode(IEnumerable<int> numbers)
            {
                return numbers.Sum();
            }
        }
    }
}