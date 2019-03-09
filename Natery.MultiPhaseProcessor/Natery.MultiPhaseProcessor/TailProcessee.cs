using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class TailProcessee<TInput> : ITailProcessee, IProcessee<TInput>
    {
        internal ConcurrentQueue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task> _action;

        public TailProcessee(Func<TInput, Task> action)
        {
            _queue = new ConcurrentQueue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
        }

        public async Task BeginProcessingAsync()
        {
            await Executor();
        }

        private async Task Executor()
        {
            TInput input = default;
            while (_moreWorkToAdd || _queue.TryDequeue(out input))
            {
                if (input != default)
                    await _action(input);
                else
                    await Task.Delay(100);

                input = default;
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