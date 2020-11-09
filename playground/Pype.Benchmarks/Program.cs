using BenchmarkDotNet.Running;
using McMaster.Extensions.CommandLineUtils;
using Pype.Benchmarks.BackgroundProcessing;
using Pype.Benchmarks.BusComparison;
using Pype.Benchmarks.SendComparison;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pype.Benchmarks
{
    public enum BenchmarkTests
    { 
        BusComparison,
        SendComparison,
        BackgroundQueue,
        BackgroundChannels,
        BackgroundDataFlow,
        BackgroundReactive
    }

    public class Program
    {
        private readonly Dictionary<BenchmarkTests, Action> _benchmarkTests = new()
        {
            { BenchmarkTests.BusComparison, () => BenchmarkRunner.Run<BusComparisonBenchmarks>() },
            { BenchmarkTests.SendComparison, () => BenchmarkRunner.Run<SendComparisonBenchmarks>() },
            { BenchmarkTests.BackgroundQueue, () => BenchmarkRunner.Run<BackgroundQueueBenchmark>() },
            { BenchmarkTests.BackgroundChannels, () => BenchmarkRunner.Run<BackgroundChannelsBenchmark>() },
            { BenchmarkTests.BackgroundDataFlow, () => BenchmarkRunner.Run<BackgroundDataFlowBenchmark>() },
            { BenchmarkTests.BackgroundReactive, () => BenchmarkRunner.Run<BackgroundReactiveBenchmark>() }
        };

        public static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public void OnExecute()
        {
            foreach (var test in Tests)
            {
                _benchmarkTests[test].Invoke();
            }
        }

        [Argument(0, Description = "Benchmark tests to run. Allowed multiple values: BusComparison, SendComparison, BackgroundQueue, BackgroundChannels, BackgroundDataFlow, BackgroundReactive.")]
        [Required]
        public BenchmarkTests[] Tests { get; }
    }
}
