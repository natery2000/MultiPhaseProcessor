using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>, IProcesseeWithNext<TInput>, INonHeadProcessee
    {
        internal IProcessee<TOutput> _next;
        internal Queue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task<TOutput>> _action;

        public Processee(Func<TInput, Task<TOutput>> action)
        {
            _queue = new Queue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
        }

        public async Task BeginProcessingAsync()
        {
            var nextProcessing = _next.BeginProcessingAsync();
            
            var currentProcessing = Executor();
            
            await Task.WhenAll(nextProcessing, currentProcessing);
        }

        private async Task Executor()
        {
            while (_queue.Count > 0 || _moreWorkToAdd)
            {
                if (_queue.Count > 0)
                {
                    var output = await _action(_queue.Dequeue());
                    _next.AddWorkItem(output);
                }
                else
                    await Task.Delay(100);
            }
            ((INonHeadProcessee)_next).NoMoreWorkToAdd();
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }

        public void AddNext(INonHeadProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }
    }
}