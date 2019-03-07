namespace MultiPhaseProcessor
{
    public interface IProcessee<TInput, TOutput> : IProcessee<TInput>
    {
    }

    public interface IProcessee<TInput> : IProcessee
    {
        void BeginProcessing();
        void AddWorkItem(TInput workItem);
        void NoMoreWorkToAdd();
    }

    public interface IProcessee
    {
        void AddNext(IProcessee processee);
    }
}