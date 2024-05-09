using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using static BenchmarkDotNet.Columns.StatisticColumn;

// dotnet run -c Release --project Atmoos.Sphere.Benchmark
var config = DefaultConfig.Instance.HideColumns(StdDev, Median, Kurtosis, BaselineRatioColumn.RatioStdDev);

var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
