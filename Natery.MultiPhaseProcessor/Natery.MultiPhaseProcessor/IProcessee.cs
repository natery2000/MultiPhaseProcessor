using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    internal interface IProcessee<TInput, TOutput> : IProcesseeWithInput<TInput>, IProcesseeWithOutput<TOutput>
    {
    }

    internal interface IProcesseeWithInput<TInput> : IProcessee
    {
        void AddWorkItem(TInput workItem);
    }

    internal interface IProcesseeWithOutput<TOutput> : IProcessee
    {
        void AddNext(IProcesseeWithInput<TOutput> processee);
    }

    internal interface IProcessee
    {
        Task BeginProcessingAsync();
        void NoMoreWorkToAdd();
    }
}