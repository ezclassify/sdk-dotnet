using System.Text.Json.Nodes;

namespace EzClassify.Tests;

/// <summary>
/// Structural subset comparison shared by the conformance and replayer-contract
/// suites: every property or array prefix present in expected must match in
/// actual, while extra actual content is allowed. An expected object read
/// against an actual array addresses it as the Node reference matcher does: a
/// canonical decimal key names an element and <c>length</c> is the element
/// count, so a plan can pin an array's exact length. Both suites must apply
/// identical match semantics, so the predicate lives in one place.
/// </summary>
internal static class JsonSubset
{
    /// <summary>Return true when <paramref name="expected"/> is a structural subset of <paramref name="actual"/>.</summary>
    internal static bool Matches(JsonNode? actual, JsonNode? expected)
    {
        if (expected is null) return actual is null;
        if (actual is null) return false;

        if (expected is JsonObject expectedObject && actual is JsonArray array)
        {
            foreach (var kv in expectedObject)
            {
                if (!Matches(ArrayMember(array, kv.Key), kv.Value)) return false;
            }
            return true;
        }

        if (expected is JsonObject expObj)
        {
            if (actual is not JsonObject actObj) return false;
            foreach (var kv in expObj)
            {
                if (!Matches(actObj[kv.Key], kv.Value)) return false;
            }
            return true;
        }

        if (expected is JsonArray expArr)
        {
            if (actual is not JsonArray actArr) return false;
            if (actArr.Count < expArr.Count) return false;
            for (int i = 0; i < expArr.Count; i++)
            {
                if (!Matches(actArr[i], expArr[i])) return false;
            }
            return true;
        }

        return JsonNode.DeepEquals(actual, expected);
    }

    /// <summary>Read <paramref name="key"/> from <paramref name="array"/> as a JavaScript property lookup would.</summary>
    private static JsonNode? ArrayMember(JsonArray array, string key)
    {
        if (key == "length") return JsonValue.Create(array.Count);
        if (key.Length == 0 || key.Length > 9 || !key.All(char.IsAsciiDigit) || (key.Length > 1 && key[0] == '0'))
        {
            return null;
        }
        var index = int.Parse(key, System.Globalization.CultureInfo.InvariantCulture);
        return index < array.Count ? array[index] : null;
    }
}
