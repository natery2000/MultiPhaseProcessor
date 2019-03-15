﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natery.MultiPhaseProcessor
{
    public class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>, IProcesseeWithNext<TInput>, INonHeadProcessee
    {
        internal IProcessee<TOutput> _next;
        internal ConcurrentQueue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task<TOutput>> _action;
        internal int _maxDegreesOfParallelism;
        internal int _count = 0;

        public Processee(Func<TInput, Task<TOutput>> action, int maxDegreesOfParallelism = 10)
        {
            _queue = new ConcurrentQueue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
            _maxDegreesOfParallelism = maxDegreesOfParallelism;
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

            var actionBlock = new ActionBlock<TInput>(
            actionInput =>
            {
                var output = _action(actionInput).Result;
                _next.AddWorkItem(output);
                progress++;
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

            }

            //Need to wait for all items to complete
            while (progress < _count) { Thread.Sleep(100); }

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