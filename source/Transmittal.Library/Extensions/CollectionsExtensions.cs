namespace Transmittal.Library.Extensions;
public static class CollectionsExtensions
{
#if NET8_0_OR_GREATER
    // This method is available in .NET but not in Framework
#else
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
#endif

}
