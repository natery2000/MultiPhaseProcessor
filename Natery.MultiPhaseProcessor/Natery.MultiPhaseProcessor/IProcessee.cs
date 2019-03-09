using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public interface IProcessee<TInput, TOutput> : IProcessee<TInput>
    {
    }

    public interface IProcessee<TInput>
    {
        void AddWorkItem(TInput workItem);
        Task BeginProcessingAsync();
    }

    public interface INonHeadProcessee
    {
        void NoMoreWorkToAdd();
    }

    public interface IProcesseeWithNext<TInput> : IProcesseeWithNext, IProcessee<TInput>
    {
    }

    public interface IProcesseeWithNext
    {
        void AddNext(INonHeadProcessee processee);
    }

    public interface IHeadProcessee<TInput> : IProcesseeWithNext<TInput>
    {
    }

    public interface ITailProcessee : INonHeadProcessee { }
}