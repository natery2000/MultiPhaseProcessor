using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor
{
    public class TailProcessee<TInput> : ITailProcessee, IProcessee<TInput>
    {
        internal Queue<TInput> _queue;
        internal bool _moreWorkToAdd;
        internal Func<TInput, Task> _action;

        public TailProcessee(Func<TInput, Task> action)
        {
            _queue = new Queue<TInput>();
            _moreWorkToAdd = true;
            _action = action;
        }

        async Task IProcessee<TInput>.BeginProcessingAsync()
        {
            while (_queue.Count > 0 || _moreWorkToAdd)
            {
                if (_queue.Count > 0)
                    await _action(_queue.Dequeue());
                else
                    await Task.Delay(100);
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