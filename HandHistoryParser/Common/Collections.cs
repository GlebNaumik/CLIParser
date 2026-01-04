using System.Collections.Immutable;

namespace HandHistoryParser;

public static class Collections {
    public static ImmutableList<T>
    MapToImmutableList<TSource, T>(this IEnumerable<TSource> source, Func<TSource, T> selector) =>
        source.Select(selector).ToImmutableList();

    public static IEnumerable<(string firstString, string secondString)>
    ConvertToTuple(this IEnumerable<string> input) {
        for (var i = 0; i < input.Count(); i += 2) {
            var firstString = input.ElementAt(i);
            string? secondString = null;
            if (i + 1 < input.Count())
                secondString = input.ElementAt(i + 1);
            yield return (firstString, secondString);
        }
    }

}