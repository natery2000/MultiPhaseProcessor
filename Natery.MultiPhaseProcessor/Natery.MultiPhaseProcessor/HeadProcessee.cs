using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natery.MultiPhaseProcessor
{
    internal class HeadProcessee<TInput, TOutput> : IHeadProcessee<TInput>, IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal ConcurrentQueue<TInput> _queue;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _maxDegreesOfParallelism;
        internal int _count;

        internal HeadProcessee(Func<TInput, Task<TOutput>> action, int maxDegreesOfParallelism = 10)
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

        public async Task BeginProcessingAsync()
        {
            _count = _queue.Count;

            var nextProcessing = _next.BeginProcessingAsync();

            var executor = Executor();

            await Task.WhenAll(executor, nextProcessing);
        }

        private async Task Executor()
        {
            int progress = 0;

            var actionBlock = new ActionBlock<TInput>(
            actionInput =>
            {
                var output = _action(actionInput).Result;
                _next.AddWorkItem(output);
                progress++;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _maxDegreesOfParallelism });

            while (_queue.TryDequeue(out var input))
                actionBlock.Post(input);

            actionBlock.Complete();
            await actionBlock.Completion;

            ((INonHeadProcessee)_next).NoMoreWorkToAdd();
        }
    }
}