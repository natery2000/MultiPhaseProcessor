using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor.Test
{
    [TestClass]
    public class Test_Processor
    {
        [TestMethod]
        public async Task Processor_OutOfOrderDueToDelay_Observed()
        {
            var processor = new Processor<int>();

            var list = new List<int>();

            Func<int, Task<int>> first = async (i) =>
            {
                list.Add(i);
                return await Task.FromResult(i + 1);
            };

            Func<int, Task<int>> second = async (i) =>
            {
                if (i == 21) await Task.Delay(1000);
                list.Add(i);
                return await Task.FromResult(i + 1);
            };

            Func<int, Task> third = (i) =>
            {
                list.Add(i);
                return Task.CompletedTask;
            };

            processor
                .WithHeadProcessee(new HeadProcessee<int, int>(first))
                .WithProcessee(new Processee<int, int>(second))
                .WithTailProcessee(new TailProcessee<int>(third));

            processor.AddWorkItems(new[] { 10, 20, 30 });

            await processor.BeginAsync();

            CollectionAssert.AreEqual(new int[] { 10, 20, 30, 11, 12, 21, 31, 22, 32 }, list);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Proccessor_AddNullHead_Exception()
        {
            var p = new Processor<int>();
            p.WithHeadProcessee(new HeadProcessee<int, int>((int i) => Task.FromResult(1)));
            p.WithHeadProcessee(new HeadProcessee<int, int>((int i) => Task.FromResult(1)));
        }
    }
}
