namespace Core.Sorting.Algorithms;

public sealed class ArrayChunkSortAlgorithm<T> : IChunkSortAlgorithm<T>
    where T : IComparable<LineRecord>
{
    public void Sort(T[] buffer, int count)
    {
        Array.Sort(buffer, 0, count);
    }
}