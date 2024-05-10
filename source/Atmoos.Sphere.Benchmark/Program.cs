using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Atmoos.Sphere.BenchmarkDotNet;

using static BenchmarkDotNet.Columns.StatisticColumn;

// dotnet run -c Release --project Atmoos.Sphere.Benchmark
var assembly = typeof(Program).Assembly;
var config = DefaultConfig.Instance.HideColumns(StdDev, Median, Kurtosis, BaselineRatioColumn.RatioStdDev);

var summary = BenchmarkSwitcher.FromAssembly(assembly).Run(args, config);

await assembly.Export(summary);
