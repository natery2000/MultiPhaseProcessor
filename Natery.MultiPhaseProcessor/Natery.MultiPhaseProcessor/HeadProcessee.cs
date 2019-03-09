using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class HeadProcessee<TInput, TOutput> : IHeadProcessee<TInput>, IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal ConcurrentQueue<TInput> _queue;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _count;

        public HeadProcessee(Func<TInput, Task<TOutput>> action)
        {
            _queue = new ConcurrentQueue<TInput>();
            _action = action;
        }

        public void AddNext(INonHeadProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public async Task BeginProcessingAsync()
        {
            _count = _queue.Count;

            var nextProcessing = _next.BeginProcessingAsync();

            var currentProcessing = Executor();

            await Task.WhenAll(nextProcessing, currentProcessing);
        }

        private async Task Executor()
        {
            int progress = 0;

            TInput input = default(TInput);
            while (_queue.TryDequeue(out input))
            {
                if (input != null || !input.Equals(default(TInput)))
                {
                    var output = await _action(input);
                    _next.AddWorkItem(output);
                    progress++;
                    input = default(TInput);
                }
            }

            //Need to wait for all items to complete
            //Needed when multi-threading is implemented
            //while (progress < _count) { await Task.Delay(100); }

            ((INonHeadProcessee)_next).NoMoreWorkToAdd();
        }
    }
}