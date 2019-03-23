using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natery.MultiPhaseProcessor
{
    internal class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>
    {
        internal IProcesseeWithInput<TOutput> _next;
        internal ConcurrentQueue<WorkItem<TInput>> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _maxDegreesOfParallelism;
        internal int _count = 0;

        internal Processee(Func<TInput, Task<TOutput>> action, int maxDegreesOfParallelism = 10)
        {
            _queue = new ConcurrentQueue<WorkItem<TInput>>();
            _moreWorkToAdd = true;
            _action = action;
            _maxDegreesOfParallelism = maxDegreesOfParallelism;
        }

        public void AddNext(IProcesseeWithInput<TOutput> processee)
        {
            _next = processee;
        }

        public async Task BeginProcessingAsync()
        {
            var nextProcessing =  _next?.BeginProcessingAsync() ?? Task.CompletedTask;

            var currentProcessing = Executor();

            await Task.WhenAll(nextProcessing, currentProcessing);
        }

        public void AddWorkItem(WorkItem<TInput> workItem)
        {
            _count++;
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }

        private async Task Executor()
        {
            int progress = 0;

            var actionBlock = new ActionBlock<TInput>(
            actionInput =>
            {
                var output = _action(actionInput).Result;
                if (_next != null)
                    _next.AddWorkItem(new WorkItem<TOutput>(output));

                progress++;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _maxDegreesOfParallelism });

            WorkItem<TInput> input = default(WorkItem<TInput>);
            while (_moreWorkToAdd || _queue.TryDequeue(out input))
            {
                if (!(input == null || input.Equals(default(TInput))))
                {
                    actionBlock.Post(input.Item);

                    input = default(WorkItem<TInput>);
                }
                else
                    await Task.Delay(100);

            }
            actionBlock.Complete();
            await actionBlock.Completion;

            if (_next != null)
                _next.NoMoreWorkToAdd();
        }
    }
}