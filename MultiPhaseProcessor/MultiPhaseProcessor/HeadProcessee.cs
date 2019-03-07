using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiPhaseProcessor
{
    public class HeadProcessee<TInput, TOutput> : IHeadProcessee<TInput>, IProcessee<TInput, TOutput>
    {
        internal IProcessee<TOutput> _next;
        internal Queue<TInput> _queue;
        internal Func<TInput, TOutput> _action;

        public HeadProcessee(Func<TInput, TOutput> action)
        {
            _queue = new Queue<TInput>();
            _action = action;
        }

        public void AddData(TInput data)
        {
            AddWorkItem(data);
        }

        public void BeginProcessing()
        {
            Task.Run(() => _next.BeginProcessing());

            while (_queue.Count > 0)
            {
                var input = _queue.Dequeue();
                var output = _action(input);
                _next.AddWorkItem(output);
            }

            _next.NoMoreWorkToAdd();
        }

        public void AddWorkItem(TInput workItem)
        {
            _queue.Enqueue(workItem);
        }

        public void NoMoreWorkToAdd()
        {
            throw new NotImplementedException();
        }

        public void AddNext(IProcessee processee)
        {
            _next = (IProcessee<TOutput>)processee;
        }
    }

    public interface IHeadProcessee<TInput> : IProcessee
    {
        void AddWorkItem(TInput workItem);
        void BeginProcessing();
    }
}