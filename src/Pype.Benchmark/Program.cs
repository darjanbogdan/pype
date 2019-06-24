using BenchmarkDotNet.Running;
using System;

namespace Pype.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SendRequestBenchmarks>();
        }
    }
}
