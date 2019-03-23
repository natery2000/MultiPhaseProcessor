namespace Natery.MultiPhaseProcessor
{
    internal class WorkItem<T>
    {
        internal T Item { get; }

        public WorkItem(T item)
        {
            Item = item;
        }
    }
}
