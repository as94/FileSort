namespace Core.Sorting.Algorithms;

public interface IChunkSortAlgorithm<in T> where T : IComparable<LineRecord>
{
    void Sort(T[] buffer, int count);
}