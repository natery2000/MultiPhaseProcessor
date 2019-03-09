using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>, IProcesseeWithNext<TInput>, INonHeadProcessee
    {
        internal IProcessee<TOutput> _next;
        internal ConcurrentQueue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _count = 0;

        public Processee(Func<TInput, Task<TOutput>> action)
        {
            _queue = new ConcurrentQueue<TInput>();
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
            int progress = 0;

            TInput input = default(TInput);
            while (_moreWorkToAdd || _queue.TryDequeue(out input))
            {
                if (input != null || !input.Equals(default(TInput)))
                {
                    var output = await _action(input);
                    _next.AddWorkItem(output);
                    progress++;
                }
                else
                    await Task.Delay(100);

                input = default(TInput);
            }

            //Need to wait for all items to complete
            //Needed when multi-threadin is implemented
            //while (progress < _count) { await Task.Delay(100); }

            ((INonHeadProcessee)_next).NoMoreWorkToAdd();
        }

        public void AddWorkItem(TInput workItem)
        {
            _count++;
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