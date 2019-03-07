using System;
using System.Collections.Generic;

namespace MultiPhaseProcessor
{

    public class Processor<TInput>
    {
        internal IHeadProcessee<TInput> _head;
        internal ITailProcessee _tail;
        internal IProcessee _lastAdded;

        public Processor() { }

        public Processor<TInput> WithHeadProcessee(IHeadProcessee<TInput> headProcessee)
        {
            if (_lastAdded != null) throw new Exception();

            _head = headProcessee;
            _lastAdded = headProcessee;

            return this;
        }

        public Processor<TInput> WithProcessee(IProcessee processee)
        {
            _lastAdded.AddNext(processee);
            _lastAdded = processee;

            return this;
        }

        public Processor<TInput> WithTailProcessee(ITailProcessee tailProcessee)
        {
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

        public void Begin()
        {
            _head.BeginProcessing();
        }
    }
}