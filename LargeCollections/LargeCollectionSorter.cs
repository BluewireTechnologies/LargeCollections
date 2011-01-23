using System;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections
{
    public interface ISortOrder<T> : IEquatable<ISortOrder<T>>, IComparer<T>
    {
        bool Reversed { get; }

        IEnumerable<T> Sort(IEnumerable<T> set);
    }

    public class LargeCollectionSorter
    {
        private readonly IAccumulatorSelector accumulatorSelector;
        private readonly long minBatchSize;

        public LargeCollectionSorter(IAccumulatorSelector accumulatorSelector) : this(accumulatorSelector, MIN_BATCH_SIZE)
        {
        }

        public LargeCollectionSorter(IAccumulatorSelector accumulatorSelector, long minBatchSize)
        {
            this.accumulatorSelector = accumulatorSelector;
            this.minBatchSize = minBatchSize;
        }

        private const long MIN_BATCH_SIZE = 10000;

        public ILargeCollection<T> Sort<T>(ISinglePassCollection<T> source)
        {
            return Sort(source, Comparer<T>.Default);
        }

        public ILargeCollection<T> Sort<T>(ISinglePassCollection<T> source, IComparer<T> comparison)
        {
            if(source.Count == 0) return InMemoryAccumulator<T>.Empty();

            var batchSize = (int)Math.Max(Math.Sqrt(source.Count), minBatchSize);
            // prepare to read the source set in batches.
            var batches = new BatchedSinglePassCollection<T>(source, batchSize);

            // for each batch, sort it and store as an ILargeCollection.
            var sortedBatches = SortBatches(batches, comparison, () => accumulatorSelector.GetAccumulator<T>(source.Count)).ToList();
            if (batches.Count == 1)
            {
                // just one batch. return it.
                return sortedBatches.Single();
            }
            return Merge(sortedBatches, comparison, source.Count);
        }

        private ILargeCollection<T> Merge<T>(IEnumerable<ILargeCollection<T>> sortedBatches, IComparer<T> comparison, long totalSize)
        {
            using(var sortedBatchList = new DisposableList<ILargeCollection<T>>(sortedBatches))
            {
                using (var accumulator = accumulatorSelector.GetAccumulator<T>(totalSize))
                {
                    foreach (var item in new SortedEnumerableMerger<T>(sortedBatchList.Cast<IEnumerable<T>>().ToList(), comparison, new SetUnionSortPreservingMerge<T>()))
                    {
                        accumulator.Add(item);
                    }
                    return accumulator.Complete();
                }
            }
        }

        private IEnumerable<ILargeCollection<T>> SortBatches<T>(ISinglePassCollection<IEnumerable<T>> batches, IComparer<T> comparison, Func<IAccumulator<T>> getBatchAccumulator)
        {
            while(batches.MoveNext())
            {
                using (var accumulator = getBatchAccumulator())
                {
                    accumulator.AddRange(batches.Current.OrderBy(i => i, comparison));
                    yield return accumulator.Complete();
                }
            }
        }


    }
}
