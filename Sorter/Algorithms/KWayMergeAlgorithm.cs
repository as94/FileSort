namespace Sorter.Algorithms;

public sealed class KWayMergeAlgorithm<T> : IMergeAlgorithm<T> where T : IComparable<T>
{
    public IEnumerable<T> Merge(IReadOnlyList<IEnumerable<T>> sortedSources)
    {
        var enumerators = sortedSources
            .Select(s => s.GetEnumerator())
            .ToList();

        var pq = new PriorityQueue<(T value, int index), T>();

        for (var i = 0; i < enumerators.Count; i++)
        {
            if (enumerators[i].MoveNext())
            {
                pq.Enqueue((enumerators[i].Current, i), enumerators[i].Current);
            }
        }

        try
        {
            while (pq.Count > 0)
            {
                var (value, index) = pq.Dequeue();
                yield return value;

                if (enumerators[index].MoveNext())
                {
                    pq.Enqueue((enumerators[index].Current, index),
                        enumerators[index].Current);
                }
            }
        }
        finally
        {
            foreach (var e in enumerators)
            {
                e.Dispose();
            }
        }
    }
}