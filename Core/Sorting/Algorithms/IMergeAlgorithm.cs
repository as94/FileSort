namespace Core.Sorting.Algorithms;

public interface IMergeAlgorithm<T> where T : IComparable<T>
{
    IEnumerable<T> Merge(IReadOnlyList<IEnumerable<T>> sortedSources);
}