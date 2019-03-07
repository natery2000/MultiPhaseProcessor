using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiPhaseProcessor
{
    public class Processee<TInput, TOutput> : IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal Queue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, TOutput> _action;

        public Processee(Func<TInput, TOutput> action)
        {
            _queue = new Queue<TInput>();
            _moreWorkToAdd = false;
            _action = action;
        }

        void IProcessee<TInput>.BeginProcessing()
        {
            Task.Run(() => _next.BeginProcessing());

            while (_queue.Count > 0 || _moreWorkToAdd)
            {
                var input = _queue.Dequeue();
                var output = _action(input);
                _next.AddWorkItem(output);
            }

            _next.NoMoreWorkToAdd();
        }

        void IProcessee<TInput>.AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }

        void IProcessee.AddNext(IProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }
    }
}