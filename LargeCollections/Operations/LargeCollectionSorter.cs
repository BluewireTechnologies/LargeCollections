using System;
using System.Collections.Generic;
using System.Linq;
using LargeCollections.Collections;
using LargeCollections.Linq;
using LargeCollections.Resources;

namespace LargeCollections.Operations
{
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

        private const int MIN_BATCH_SIZE = 10000;
        private int GetBatchSize<T>(IEnumerator<T> source)
        {
            var countable = source.GetUnderlying<ICounted>();
            return (int)(countable == null ? minBatchSize : Math.Max(Math.Sqrt(countable.Count), minBatchSize));
        }

        public IEnumerator<T> Sort<T>(IEnumerator<T> source, IComparer<T> comparison)
        {
            var sortedSource = source.GetUnderlying<ISorted<T>>();
            if(sortedSource != null && sortedSource.SortOrder == comparison)
            {
                // already sorted
                return source;
            }

            using (source)
            {
                var batchSize = GetBatchSize(source);
                // prepare to read the source set in batches.
                using (var batches = source.Batch(batchSize))
                {
                    // for each batch, sort it.
                    var sortedBatches = SortBatches(batches, comparison, () => accumulatorSelector.GetAccumulator<T>(source)).ToList();
                    if (!sortedBatches.Any()) return Enumerable.Empty<T>().GetEnumerator();
                    return Merge(sortedBatches);
                }
            }
        }

        private IEnumerator<T> Merge<T>(IEnumerable<IEnumerator<T>> sortedBatches)
        {
            
            return new SortedEnumeratorMerger<T>(sortedBatches.ToList(), new SetUnionSortPreservingMerge<T>());
        }

        private IEnumerable<IEnumerator<T>> SortBatches<T>(IEnumerator<IEnumerable<T>> batches, IComparer<T> comparison, Func<IAccumulator<T>> getBatchAccumulator)
        {
            while(batches.MoveNext())
            {
                using (var accumulator = getBatchAccumulator())
                {
                    yield return 
                        batches.Current
                            .OrderBy(i => i, comparison)
                            .GetEnumerator()
                            .BufferOnce(accumulator)
                            .UsesSortOrder(comparison);
                }
            }
        }


    }
}
