using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using LargeCollections;
using LargeCollections.Collections;
using LargeCollections.Linq;
using LargeCollections.Operations;
using NConsoler;

namespace LargeCollectionProfiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            Consolery.Run(typeof(Program), args);
        }

        private static SizeBasedAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector();

        [Action]
        public static void Run(
            [Optional(null)]string tempRoot,
            [Optional(false)]bool hugeCollections)
        {
            if(!String.IsNullOrEmpty(tempRoot)) accumulatorSelector.TemporaryFileProvider = new TemporaryFileProvider(tempRoot);

            var widths = new int[] {25, 14, 20, 14, 20};

            Console.WriteLine(String.Join("", Tabulate(new object[] { "", "Enumerables", "", "Large Collections", "" }, widths).ToArray()));
            Console.WriteLine(String.Join("", Tabulate(new object[] { "Test", "Duration", "Memory", "Duration", "Memory" }, widths).ToArray()));
            foreach (var comparison in GetComparisons(hugeCollections))
            {
                Console.WriteLine(String.Join("", Tabulate(
                    comparison.Cells.ToArray(),
                    widths).ToArray()));
            }
            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private static IEnumerable<string> Tabulate(object[] cells, int[] widths)
        {
            for(var i = 0; i < cells.Length; i++)
            {
                yield return Format(cells[i], widths[i], i == 0);
            }
        }

        private static string Format(object value, int width, bool leftAlign)
        {
            var str = value.ToString();
            if(leftAlign)
            {
                return str.PadRight(width, ' ');
            }
            return str.PadLeft(width, ' ');
        }

        private static IEnumerable<Comparison> GetComparisons(bool hugeCollections)
        {
            const long thousand = 1000;
            const long million = thousand * thousand;

            const long absurd = 2 * thousand * million;
            const long large = 3 * million;
            const long midsize = 100 * thousand;
            const long small = 500;
            const float similar = 0.9f;
            const float dissimilar = 0.2f;

            if (hugeCollections)
            {
                yield return Compare("Huge similar sets", absurd, absurd, similar);
                yield return Compare("Huge dissimilar sets", absurd, absurd, dissimilar);
            }

            yield return Compare("Large similar sets", large, large, similar);
            yield return Compare("Large dissimilar sets", large, large, dissimilar);
            yield return Compare("Midsize similar sets", midsize, midsize, 0.9f);
            yield return Compare("Midsize dissimilar sets", midsize, midsize, dissimilar);
            yield return Compare("Small similar sets", small, small, 0.9f);
            yield return Compare("Small dissimilar sets", small, small, dissimilar);
            yield return Compare("A bigger than B", large, small, similar);
            yield return Compare("B bigger than A", small, large, similar);
        }

        public struct MemoryUsage
        {
            private readonly long? bytes;

            public MemoryUsage(long bytes)
            {
                this.bytes = bytes;
            }

            public override string ToString()
            {
                if (bytes == null) return "";
                double b = bytes.Value;
                if (b < 1024)
                {
                    return Units(b, "bytes");
                }
                b /= 1024;
                if (b < 1024)
                {
                    return Units(b, "KB");
                }
                b /= 1024;
                return Units(b, "MB");
            }

            private string Units(double count, string unit)
            {
                return String.Format("{0:0.00} {1}", count, unit);
            }
        }
        
        public struct Comparison
        {
            public string TestName { get; set; }
            public Result? LargeCollections { get; set; }
            public Result? Enumerables { get; set; }

            public IEnumerable<object> Cells
            {
                get
                {
                    yield return TestName;
                    if(Enumerables.HasValue)
                    {
                        yield return Enumerables.Value.Duration;
                        yield return Enumerables.Value.MemoryUsage;
                    }
                    else
                    {
                        yield return "-----";
                        yield return "-----";
                    }
                    if (LargeCollections.HasValue)
                    {
                        yield return LargeCollections.Value.Duration;
                        yield return LargeCollections.Value.MemoryUsage;
                    }
                    else
                    {
                        yield return "-----";
                        yield return "-----";
                    }
                }
            }
        }

        public struct Duration
        {
            private TimeSpan duration;

            public Duration(TimeSpan duration)
            {
                this.duration = duration;
            }

            public override string ToString()
            {
                return String.Format("{0:00}:{1:00}:{2:00}", duration.Hours, duration.Minutes, duration.Seconds);
            }
        }

        public struct Result
        {
            public Duration Duration { get; set; }
            public MemoryUsage MemoryUsage { get; set; }
        }

        public static Comparison Compare(string name, long setASize, long setBSize, float sharedPercentage)
        {
            var sharedCount = (long)(Math.Min(setASize, setBSize)*sharedPercentage);

            var setB = GenerateGuids(setBSize - sharedCount);
            var setA = GenerateGuids(setASize - sharedCount);
            var sharedSet = GenerateGuids(sharedCount);

            List<IEnumerable<Guid>> resultSets = new List<IEnumerable<Guid>>();
            var enumerables = Profile(sharedSet.Concat(setA), sharedSet.Concat(setB), EnumerableDifference, resultSets);
            var largeCollections = Profile(sharedSet.Concat(setA), sharedSet.Concat(setB), LargeCollectionDifference, resultSets);

            if (enumerables.HasValue && largeCollections.HasValue)
            {
                if (!resultSets[0].OrderBy(i => i).SequenceEqual(resultSets[1]))
                {
                    Console.WriteLine("Mismatched results! enumerable : {0} elements, LC: {1} elements", resultSets[0].Count(), resultSets[1].Count());
                }
            }

            foreach(var disposable in resultSets.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            return new Comparison {
                TestName = name,
                Enumerables = enumerables,
                LargeCollections = largeCollections
            };
        }

        private static IEnumerable<Guid> EnumerableOfGuids(long count)
        {
            for (; count > 0; count--)
            {
                yield return Guid.NewGuid();
            }
        }

        private static IEnumerable<Guid> GenerateGuids(long count)
        {
            try
            {
                var list = new List<Guid>((int)count);
                list.AddRange(EnumerableOfGuids(count));
                return list;
            }
            catch(OutOfMemoryException)
            {
                return EnumerableOfGuids(count);
            }
        }

        private static Result? Profile(IEnumerable<Guid> setA, IEnumerable<Guid> setB, Func<IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>> process, List<IEnumerable<Guid>> resultSets)
        {
            GC.Collect();
            try
            {
                var monitor = new ProcessMonitor();
                monitor.Start();

                var result = process(setA, setB);
                resultSets.Add(result);

                monitor.Stop();

                return new Result() {
                    Duration = new Duration(monitor.GetDuration()),
                    MemoryUsage = new MemoryUsage(monitor.GetMemoryAverage())
                };
            }
            catch
            {
                return null;
            }
        }

        private static ILargeCollection<Guid> LargeCollectionDifference(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var operations = new LargeCollectionOperations(accumulatorSelector);

            using (var largeSetA = operations.Buffer(setA))
            {
                using (var largeSetB = operations.Buffer(setB))
                {
                    return operations.Difference(largeSetA.AsSinglePass(), largeSetB.AsSinglePass()).Buffer();
                }
            }
        }

        private static IEnumerable<Guid> EnumerableDifference(IEnumerable<Guid> index, IEnumerable<Guid> source)
        {
            var inIndex = new HashSet<Guid>(index, EqualityComparer<Guid>.Default);
            var inIndexAndSource = new HashSet<Guid>(EqualityComparer<Guid>.Default); // intersection
            var onlyInSource = new List<Guid>();


            foreach (var id in source)
            {
                if (inIndex.Contains(id))
                {
                    inIndexAndSource.Add(id);
                }
                else
                {
                    onlyInSource.Add(id);
                }
            }

            // grab the ids that exist only in the source
            var outputSet = onlyInSource;
            // add those that are only in the index
            foreach (var id in inIndex)
            {
                if (!inIndexAndSource.Contains(id))
                {
                    outputSet.Add(id);
                }
            }
            return outputSet;
        }
        
        class ProcessMonitor
        {
            public ProcessMonitor()
            {
                memoryMonitor = new Timer(100) { AutoReset = true };
                memoryMonitor.Elapsed += new ElapsedEventHandler(memoryMonitor_Elapsed);
            }

            void memoryMonitor_Elapsed(object sender, ElapsedEventArgs e)
            {
                var process = Process.GetCurrentProcess();
                maxMemoryUsage = Math.Max(process.PrivateMemorySize64, maxMemoryUsage);
                memoryUsageSampleTotal += process.PrivateMemorySize64 - initialMemoryUsage;
                memoryUsageSampleCount++;
            }

            private Stopwatch stopwatch = new Stopwatch();
            private Timer memoryMonitor;

            private long memoryUsageSampleTotal;
            private long memoryUsageSampleCount;
            private long maxMemoryUsage;
            private long initialMemoryUsage;
            private TimeSpan initialProcessorTime;
            private TimeSpan processorTime;

            public void Start()
            {
                var process = Process.GetCurrentProcess();
                initialMemoryUsage = process.PrivateMemorySize64;
                initialProcessorTime = process.TotalProcessorTime;
                memoryUsageSampleTotal = 0;
                memoryUsageSampleCount = 0;
                stopwatch.Reset();
                stopwatch.Start();
                memoryMonitor.Start();
            }

            public void Stop()
            {
                var process = Process.GetCurrentProcess();
                processorTime = process.TotalProcessorTime - initialProcessorTime;
                memoryMonitor.Stop();
                stopwatch.Stop();
            }

            public long GetMemoryDelta()
            {
                return Math.Max(maxMemoryUsage - initialMemoryUsage, 0);
            }

            public TimeSpan GetProcessorDelta()
            {
                return processorTime;
            }

            public long GetMemoryAverage()
            {
                return memoryUsageSampleCount > 0 ? memoryUsageSampleTotal / memoryUsageSampleCount : 0;
            }
            public TimeSpan GetDuration()
            {
                return stopwatch.Elapsed;
            }
        }
    }
}
