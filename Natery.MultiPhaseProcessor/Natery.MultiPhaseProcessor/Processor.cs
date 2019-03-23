using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class Processor<TInput>
    {
        internal IProcessee<TInput> _head;
        internal IProcessee _lastAdded;

        public Processor() { }

        public Processor<TInput> WithProcessee<TProcesseeInput, TProcesseeOutput>(
            Func<TProcesseeInput, Task<TProcesseeOutput>> action, 
            int maxDegreesOfParallelism = 10)
        {
            var processee = new Processee<TProcesseeInput, TProcesseeOutput>(action, maxDegreesOfParallelism);
            if (_lastAdded == null)
                _head = (IProcessee<TInput>)processee;
            else
                _lastAdded.AddNext(processee);
            
            _lastAdded = processee;

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
            _head.NoMoreWorkToAdd();
            await _head.BeginProcessingAsync();
        }
    }
}