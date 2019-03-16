using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Natery.MultiPhaseProcessor.Test
{
    [TestClass]
    public class Test_Processor
    {
        [TestMethod]
        public async Task Processor_FullData_Processees_SingleThreaded_Works()
        {
            var processor = new Processor<string>();

            var list = new List<string>();

            Func<string, Task<string>> first = async (i) =>
            {
                list.Add(i);
                return await Task.FromResult((int.Parse(i) + 1).ToString());
            };

            Func<string, Task<string>> second = async (i) =>
            {
                list.Add(i);
                return await Task.FromResult((int.Parse(i) + 1).ToString());
            };

            Func<string, Task> third = (i) =>
            {
                list.Add(i);
                return Task.CompletedTask;
            };

            processor
                .WithHeadProcessee<string>(first, 1)
                .WithProcessee<string, string>(second, 1)
                .WithTailProcessee<string>(third, 1);

            processor.AddWorkItems(new[] { "10", "20", "30" });

            await processor.BeginAsync();

            CollectionAssert.AreEqual(new string[] { "10", "20", "30", "11", "21", "31", "12", "22", "32" }, list);
        }

        [TestMethod]
        public async Task Processor_FullData_Processees_MultiThreaded_Works()
        {
            var processor = new Processor<string>();

            var list = new List<string>();

            Func<string, Task<string>> first = async (i) =>
            {
                list.Add(i);
                return await Task.FromResult((int.Parse(i) + 1).ToString());
            };

            Func<string, Task<string>> second = async (i) =>
            {
                list.Add(i);
                return await Task.FromResult((int.Parse(i) + 1).ToString());
            };

            Func<string, Task> third = (i) =>
            {
                list.Add(i);
                return Task.CompletedTask;
            };

            processor
                .WithHeadProcessee<string>(first)
                .WithProcessee<string, string>(second)
                .WithTailProcessee<string>(third);

            processor.AddWorkItems(new[] { "10", "20", "30" });

            await processor.BeginAsync();

            CollectionAssert.AreEquivalent(new string[] { "10", "11", "12", "20", "21", "22", "30", "31", "32" }, list);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Proccessor_AddNullHead_Exception()
        {
            var p = new Processor<int>();
            p.WithHeadProcessee<int>((int i) => Task.FromResult(1));
            p.WithHeadProcessee<int>((int i) => Task.FromResult(1));
        }

        //#14: Value types fail because of 'default' keyword for queue usage
        [Ignore]
        [TestMethod]
        public void Processor_ValueTypesAsync()
        {
            var condition = false;

            (new Thread(async () => await TestFunction())).Start();

            SpinWait.SpinUntil(() => condition, 1000);

            Assert.IsTrue(condition);

            async Task TestFunction()
            {
                var p = new Processor<int>();
                p.WithHeadProcessee<int>((i) => Task.FromResult(1));
                p.WithTailProcessee<int>((i) => Task.FromResult(1));
                p.AddWorkItem(1);

                await p.BeginAsync();
                condition = true;
            };
        }
    }
}
