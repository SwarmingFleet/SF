
namespace SwarmingFleet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Precondition
    {
        public static void CheckElementsCount<T>(this IEnumerable<T> collection, int length, string paramName)
        {
            collection.ThrowIfNull(nameof(collection));
            collection.ThrowIfNull(nameof(paramName));
            if (collection.Count() != length)
                throw new ArgumentOutOfRangeException(paramName);
        }
    }
}
