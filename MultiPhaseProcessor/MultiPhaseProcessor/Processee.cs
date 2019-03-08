using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiPhaseProcessor
{
    public class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>
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

        async Task IProcessee<TInput>.BeginProcessingAsync()
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
            _next.NoMoreWorkToAdd();
        }

        void IProcessee<TInput>.AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }

        void IProcessee.AddNext(IProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }
    }
}