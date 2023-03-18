using System;
using System.Collections.Generic;
using System.Text;

namespace Transmittal.Library.Extensions;
public static class CollectionsExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue = default)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }
}
