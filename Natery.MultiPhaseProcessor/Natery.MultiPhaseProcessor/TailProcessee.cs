using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natery.MultiPhaseProcessor
{
    public class TailProcessee<TInput> : ITailProcessee, IProcessee<TInput>
    {
        internal ConcurrentQueue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task> _action;
        internal int _maxDegreesOfParallelism;

        public TailProcessee(Func<TInput, Task> action, int maxDegreesOfParallelism = 10)
        {
            _queue = new ConcurrentQueue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
            _maxDegreesOfParallelism = maxDegreesOfParallelism;
        }

        public async Task BeginProcessingAsync()
        {
            await Executor();
        }

        private async Task Executor()
        {
            var actionBlock = new ActionBlock<TInput>(
            actionInput =>
            {
                _action(actionInput).Wait();
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _maxDegreesOfParallelism });

            TInput input = default(TInput);
            while (_moreWorkToAdd || _queue.TryDequeue(out input))
            {
                if (!(input == null || input.Equals(default(TInput))))
                {
                    actionBlock.Post(input);

                    input = default(TInput);
                }
                else
                    await Task.Delay(100);

                input = default(TInput);
            }
            actionBlock.Complete();
            await actionBlock.Completion;
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