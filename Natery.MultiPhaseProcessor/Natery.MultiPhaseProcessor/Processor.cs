using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class Processor<TInput>
    {
        internal IHeadProcessee<TInput> _head;
        internal ITailProcessee _tail;
        internal IProcesseeWithNext _lastAdded;

        public Processor() { }

        public Processor<TInput> WithHeadProcessee<TProcesseeOutput>(
            Func<TInput, Task<TProcesseeOutput>> action, 
            int maxDegreesOfParallelism = 10)
        {
            if (_lastAdded != null) throw new Exception();

            var headProcessee = new HeadProcessee<TInput, TProcesseeOutput>(action, maxDegreesOfParallelism);
            _head = headProcessee;
            _lastAdded = headProcessee;

            return this;
        }

        public Processor<TInput> WithProcessee<TProcesseeInput, TProcesseeOutput>(
            Func<TProcesseeInput, Task<TProcesseeOutput>> action, 
            int maxDegreesOfParallelism = 10)
        {
            var processee = new Processee<TProcesseeInput, TProcesseeOutput>(action, maxDegreesOfParallelism);
            _lastAdded.AddNext(processee);
            _lastAdded = (IProcesseeWithNext)processee;

            return this;
        }

        public Processor<TInput> WithTailProcessee<TProcesseeInput>(
            Func<TProcesseeInput, Task> action, 
            int maxDegreesOfParallelism = 10)
        {
            var tailProcessee = new TailProcessee<TProcesseeInput>(action, maxDegreesOfParallelism);
            _lastAdded.AddNext(tailProcessee);
            _tail = tailProcessee;

            return this;
        }

        public void AddWorkItem(TInput workItem)
        {
            _head.AddWorkItem(workItem);
        }

        public void AddWorkItems(IEnumerable<TInput> workItems)
        {
            foreach (var workItem in workItems) AddWorkItem(workItem);
        }

        public async Task BeginAsync()
        {
            await _head.BeginProcessingAsync();
        }
    }
}