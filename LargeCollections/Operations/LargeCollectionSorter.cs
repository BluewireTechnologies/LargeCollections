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
        private readonly BatchingPolicy batchingPolicy;

        public LargeCollectionSorter(IAccumulatorSelector accumulatorSelector)
        {
            this.accumulatorSelector = accumulatorSelector;
            batchingPolicy = new BatchingPolicy();
        }

        public LargeCollectionSorter(IAccumulatorSelector accumulatorSelector, int minBatchSize) : this(accumulatorSelector)
        {
            this.batchingPolicy = new BatchingPolicy(minBatchSize);
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
                var batchSize = batchingPolicy.GetBatchSize(source);
                // prepare to read the source set in batches.
                using (var batches = source.Batch(batchSize))
                {

                    // for each batch, sort it.
                    var sortedBatches = SortBatches(batches, comparison, () => accumulatorSelector.GetAccumulator<T>(source)).EvaluateSafely();
                    if (!sortedBatches.Any()) return Enumerable.Empty<T>().GetEnumerator().UsesSortOrder(comparison);
                    return MergeBatches(sortedBatches);
                }
            }
        }

        private static IEnumerator<T> MergeBatches<T>(IList<IEnumerator<T>> sortedBatches)
        {
            return new SetUnionSortPreservingMerge<T>().Merge(sortedBatches);
        }

        private IEnumerable<IEnumerator<T>> SortBatches<T>(IEnumerator<IEnumerable<T>> batches, IComparer<T> comparison, Func<IAccumulator<T>> getBatchAccumulator)
        {
            while (batches.MoveNext())
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
