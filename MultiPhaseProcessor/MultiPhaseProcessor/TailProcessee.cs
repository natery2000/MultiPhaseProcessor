using System;
using System.Collections.Generic;

namespace MultiPhaseProcessor
{
    public class TailProcessee<TInput> : ITailProcessee, IProcessee<TInput>
    {
        internal Queue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Action<TInput> _action;

        public TailProcessee(Action<TInput> action)
        {
            _queue = new Queue<TInput>();
            _moreWorkToAdd = false;
            _action = action;
        }

        void IProcessee<TInput>.BeginProcessing()
        {
            while (_queue.Count > 0 || _moreWorkToAdd)
            {
                var input = _queue.Dequeue();
                _action(input);
            }
        }

        void IProcessee<TInput>.AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        void IProcessee<TInput>.NoMoreWorkToAdd()
        {
            _moreWorkToAdd = false;
        }

        public void AddNext(IProcessee processee)
        {
            throw new NotImplementedException();
        }
    }

    public interface ITailProcessee : IProcessee { }
}