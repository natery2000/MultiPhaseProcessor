using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class TailProcessee<TInput> : ITailProcessee, IProcessee<TInput>
    {
        internal Queue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task> _action;

        public TailProcessee(Func<TInput, Task> action)
        {
            _queue = new Queue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
        }

        public async Task BeginProcessingAsync()
        {
            await Executor();
        }

        private async Task Executor()
        {
            while (_queue.Count > 0 || _moreWorkToAdd)
            {
                if (_queue.Count > 0)
                    await _action(_queue.Dequeue());
                else
                    await Task.Delay(100);
            }
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }
    }
}