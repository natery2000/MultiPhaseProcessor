using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public interface IProcessee<TInput, TOutput> : IProcessee<TInput>
    {
    }

    public interface IProcessee<TInput>
    {
        Task BeginProcessingAsync();
        void AddWorkItem(TInput workItem);
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