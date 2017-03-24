using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BackendDeveloperCodeChallenge.ExtensionMethods
{
    public static class DictionaryExtensions
    {
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> items)
        {
            return new ReadOnlyDictionary<TKey, TValue>(items);
        }
    }
}
