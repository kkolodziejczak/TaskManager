namespace TaskManager.Common;

internal static class QueueExtensions
{
    internal static bool IsEmpty<T>(this Queue<T> queue)
        => queue.Count == 0;

    internal static bool IsNotEmpty<T>(this Queue<T> queue)
        => !queue.IsEmpty();
}