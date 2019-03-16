using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    internal interface IProcessee<TInput, TOutput> : IProcessee<TInput>
    {
    }

    internal interface IProcessee<TInput>
    {
        void AddWorkItem(TInput workItem);
        Task BeginProcessingAsync();
    }

    internal interface INonHeadProcessee
    {
        void NoMoreWorkToAdd();
    }

    internal interface IProcesseeWithNext<TInput> : IProcesseeWithNext, IProcessee<TInput>
    {
    }

    internal interface IProcesseeWithNext
    {
        void AddNext(INonHeadProcessee processee);
    }

    internal interface IHeadProcessee<TInput> : IProcesseeWithNext<TInput>
    {
    }

    internal interface ITailProcessee : INonHeadProcessee { }
}