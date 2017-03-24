using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BackendDeveloperCodeChallenge.ExtensionMethods
{
    public static class EnumerableExtensions
    {
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> items)
        {
            if (items is T[])
            {
                return Array.AsReadOnly((T[])items);
            }
            else
            {
                return Array.AsReadOnly(items.ToArray());
            }
        }
    }
}
