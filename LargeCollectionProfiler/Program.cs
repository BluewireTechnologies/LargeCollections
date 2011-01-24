using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using LargeCollections;
using LargeCollections.Collections;
using LargeCollections.Operations;
using NConsoler;

namespace LargeCollectionProfiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run();// Consolery.Run(typeof(Program), args);
        }

        private static IAccumulatorSelector accumulatorSelector = new SizeBasedAccumulatorSelector();

        [Action]
        public static void Run()
        {
            var widths = new int[] {25, 14, 20, 14, 20};

            Console.WriteLine(String.Join("", Tabulate(new object[] { "", "Enumerables", "", "Large Collections", "" }, widths).ToArray()));
            Console.WriteLine(String.Join("", Tabulate(new object[] { "Test", "Duration", "Memory", "Duration", "Memory" }, widths).ToArray()));
            foreach (var comparison in Comparisons)
            {
                Console.WriteLine(String.Join("", Tabulate(
                    new object[] { comparison.TestName, comparison.Enumerables.Duration, comparison.Enumerables.MemoryUsage, comparison.LargeCollections.Duration, comparison.LargeCollections.MemoryUsage },
                    widths).ToArray()));
            }
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

        private static IEnumerable<Comparison> Comparisons
        {
            get
            {
                const long large = 3000000;
                const long midsize = 100000;
                const long small = 500;
                const float similar = 0.9f;
                const float dissimilar = 0.2f;

                yield return Compare("Large similar sets", large, large, similar);
                yield return Compare("Large dissimilar sets", large, large, dissimilar);
                yield return Compare("Midsize similar sets", midsize, midsize, 0.9f);
                yield return Compare("Midsize dissimilar sets", midsize, midsize, dissimilar);
                yield return Compare("Small similar sets", small, small, 0.9f);
                yield return Compare("Small dissimilar sets", small, small, dissimilar);
                yield return Compare("A bigger than B", large, small, similar);
                yield return Compare("B bigger than A", small, large, similar);
            }
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
                return bytes.Value + " bytes";
            }
        }

        public struct Comparison
        {
            public string TestName { get; set; }
            public Result LargeCollections { get; set; }
            public Result Enumerables { get; set; }
        }

        public struct Result
        {
            public TimeSpan Duration { get; set; }
            public MemoryUsage MemoryUsage { get; set; }
        }

        public static Comparison Compare(string name, long setASize, long setBSize, float sharedPercentage)
        {
            var sharedCount = (long)(Math.Min(setASize, setBSize)*sharedPercentage);

            var setB = GenerateGuids(setBSize - sharedCount).ToList();
            var setA = GenerateGuids(setASize - sharedCount).ToList();
            var sharedSet = GenerateGuids(sharedCount).ToList();

            List<IEnumerable<Guid>> resultSets = new List<IEnumerable<Guid>>();
            var enumerables = Profile(sharedSet.Concat(setA), sharedSet.Concat(setB), EnumerableDifference, resultSets);
            var largeCollections = Profile(sharedSet.Concat(setA), sharedSet.Concat(setB), LargeCollectionDifference, resultSets);

            if(!resultSets[0].OrderBy(i => i).SequenceEqual(resultSets[1]))
            {
                Console.WriteLine("Mismatched results! enumerable : {0} elements, LC: {1} elements", resultSets[0].Count(), resultSets[1].Count());
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

        private static IEnumerable<Guid> GenerateGuids(long count)
        {
            for(; count > 0; count--)
            {
                yield return Guid.NewGuid();
            }
        }

        private static Result Profile(IEnumerable<Guid> setA, IEnumerable<Guid> setB, Func<IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>> process, List<IEnumerable<Guid>> resultSets)
        {
            GC.Collect();
            var monitor = new ProcessMonitor();
            monitor.Start();

            var result = process(setA, setB);
            resultSets.Add(result);

            monitor.Stop();


            return new Result()
            {
                Duration = monitor.GetDuration(),
                MemoryUsage = new MemoryUsage(monitor.GetMemoryDelta())
            };
        }

        private static ILargeCollection<Guid> LargeCollectionDifference(IEnumerable<Guid> setA, IEnumerable<Guid> setB)
        {
            var largeSetA = Load(setA);
            var largeSetB = Load(setB);

            var sorter = new LargeCollectionSorter(accumulatorSelector);
            using (var sorted = sorter.Sort(largeSetA))
            {
                largeSetA = new SinglePassCollection<Guid>(sorted);
            }

            using (var sorted = sorter.Sort(largeSetB))
            {
                largeSetB = new SinglePassCollection<Guid>(sorted);
            }

            using (var accumulator = accumulatorSelector.GetAccumulator<Guid>(Math.Max(largeSetA.Count, largeSetB.Count)))
            {
                using (var difference = new SortedEnumeratorMerger<Guid>(new List<IEnumerator<Guid>> { largeSetA, largeSetB }, Comparer<Guid>.Default, new SetDifferenceMerge<Guid>()))
                {
                    while (difference.MoveNext())
                    {
                        accumulator.Add(difference.Current);
                    }
                }
                return accumulator.Complete();
            }
        }

        private static ISinglePassCollection<Guid> Load(IEnumerable<Guid> set)
        {
            using (var accumulator = accumulatorSelector.GetAccumulator<Guid>())
            {
                accumulator.AddRange(set);
                using (var collection = accumulator.Complete())
                {
                    return new SinglePassCollection<Guid>(collection);
                }
            }
        }


        private static IEnumerable<Guid> EnumerableDifference(IEnumerable<Guid> index, IEnumerable<Guid> source)
        {
            var inIndex = new HashSet<Guid>(index);
            var inIndexAndSource = new HashSet<Guid>(); // intersection
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
                process = Process.GetCurrentProcess();
            }

            void memoryMonitor_Elapsed(object sender, ElapsedEventArgs e)
            {
                maxMemoryUsage = Math.Max(process.PrivateMemorySize64, maxMemoryUsage);
            }

            private Stopwatch stopwatch = new Stopwatch();
            private Timer memoryMonitor;

            private long maxMemoryUsage;
            private long initialMemoryUsage;
            private TimeSpan initialProcessorTime;
            private TimeSpan processorTime;
            private Process process;

            public void Start()
            {
                initialMemoryUsage = process.PrivateMemorySize64;
                initialProcessorTime = process.TotalProcessorTime;
                stopwatch.Reset();
                stopwatch.Start();
                memoryMonitor.Start();
            }

            public void Stop()
            {
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

            public TimeSpan GetDuration()
            {
                return stopwatch.Elapsed;
            }
        }
    }
}
