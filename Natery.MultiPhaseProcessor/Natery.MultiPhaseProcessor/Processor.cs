using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class Processor<TInput>
    {
        internal IProcesseeWithInput<TInput> _head;
        internal IProcessee _lastAdded;

        public Processor<TInput> WithProcessee<TProcesseeInput, TProcesseeOutput>(
            Func<TProcesseeInput, Task<TProcesseeOutput>> action, 
            int maxDegreesOfParallelism = 10)
        {
            var processee = new Processee<TProcesseeInput, TProcesseeOutput>(action, maxDegreesOfParallelism);
            if (_lastAdded == null)
            {
                if (!typeof(TInput).IsAssignableFrom(typeof(TProcesseeInput)))
                    throw new ArgumentException($"Processee function contains invalid input type parameter for the Processor");
                _head = (IProcesseeWithInput<TInput>)processee;
            }
            else
                ((IProcesseeWithOutput<TProcesseeInput>)_lastAdded).AddNext(processee);
            
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