using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    internal interface IProcessee<TInput, TOutput> : IProcessee<TInput>
    {
    }

    internal interface IProcessee<TInput> : IProcessee
    {
        void AddWorkItem(TInput workItem);
    }

    internal interface IProcessee
    {
        void AddNext(IProcessee processee);
        Task BeginProcessingAsync();
        void NoMoreWorkToAdd();
    }
}