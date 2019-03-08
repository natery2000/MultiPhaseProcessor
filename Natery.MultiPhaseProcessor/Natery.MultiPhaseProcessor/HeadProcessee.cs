using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class HeadProcessee<TInput, TOutput> : IHeadProcessee<TInput>, IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal Queue<TInput> _queue;
        internal Func<TInput, Task<TOutput>> _action;

        public HeadProcessee(Func<TInput, Task<TOutput>> action)
        {
            _queue = new Queue<TInput>();
            _action = action;
        }

        public void AddData(TInput data)
        {
            AddWorkItem(data);
        }

        public async Task BeginProcessingAsync()
        {
            var nextProcessing = _next.BeginProcessingAsync();

            var currentProcessing = Executor();

            await Task.WhenAll(nextProcessing, currentProcessing);
        }

        private async Task Executor()
        {
            while (_queue.Count > 0)
            {
                var output = await _action(_queue.Dequeue());
                _next.AddWorkItem(output);
            }
            _next.NoMoreWorkToAdd();
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            throw new NotImplementedException();
        }

        public void AddNext(IProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }
    }

    public interface IHeadProcessee<TInput> : IProcessee
    {
        void AddWorkItem(TInput workItem);
        Task BeginProcessingAsync();
    }
}