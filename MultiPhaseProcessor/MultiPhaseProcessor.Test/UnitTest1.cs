using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MultiPhaseProcessor.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var p = new Processor<int>();
            p
                .WithHeadProcessee(new HeadProcessee<int, int>(i => i++))
                .WithProcessee(new Processee<int, int>(i => i++))
                .WithTailProcessee(new TailProcessee<int>(i => Console.WriteLine(i)));

            p.AddWorkItems(new[] { 1, 2, 3 });

            p.Begin();
        }
    }
}
