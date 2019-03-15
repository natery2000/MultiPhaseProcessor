using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natery.MultiPhaseProcessor
{
    public class HeadProcessee<TInput, TOutput> : IHeadProcessee<TInput>, IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal ConcurrentQueue<TInput> _queue;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _maxDegreesOfParallelism;
        internal int _count;

        public HeadProcessee(Func<TInput, Task<TOutput>> action, int maxDegreesOfParallelism = 10)
        {
            _queue = new ConcurrentQueue<TInput>();
            _action = action;
            _maxDegreesOfParallelism = maxDegreesOfParallelism;
        }

        public void AddNext(INonHeadProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
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

            Executor();

            await Task.WhenAll(nextProcessing);
        }

        private void Executor()
        {
            int progress = 0;

            var actionBlock = new ActionBlock<TInput>(
            actionInput =>
            {
                var output = _action(actionInput).Result;
                _next.AddWorkItem(output);
                progress++;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _maxDegreesOfParallelism });

            TInput input = default(TInput);
            while (_queue.TryDequeue(out input))
            {
                if (!(input == null || input.Equals(default(TInput))))
                {
                    actionBlock.Post(input);
                    
                    input = default(TInput);
                }
            }

            //Need to wait for all items to complete
            while (progress < _count) { Thread.Sleep(100); }

            ((INonHeadProcessee)_next).NoMoreWorkToAdd();
        }
    }
}