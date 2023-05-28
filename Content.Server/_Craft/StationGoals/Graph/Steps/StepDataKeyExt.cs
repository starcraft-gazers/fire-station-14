using System.Collections.Generic;

namespace Content.Server._Craft.StationGoals.Graph.Steps;

internal static class StepDataKeyExt
{
    internal static TValue? GetOrFallback<TValue>(this Dictionary<StepDataKey, object> source, StepDataKey key, TValue fallback)
    {
        if (source.ContainsKey(key))
        {
            var value = source[key];
            if (value is TValue)
                return (TValue) value;
            else
                return fallback;
        }

        return fallback;
    }
}
