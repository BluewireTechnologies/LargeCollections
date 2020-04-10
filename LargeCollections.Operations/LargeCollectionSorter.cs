using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.ReferenceCounting;
using LargeCollections.Core;

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

        public LargeCollectionSorter(IAccumulatorSelector accumulatorSelector, int minBatchSize)
            : this(accumulatorSelector)
        {
            this.batchingPolicy = new BatchingPolicy(minBatchSize);
        }


        public IEnumerator<T> Sort<T>(IEnumerator<T> source, IComparer<T> comparison)
        {
            var sortedSource = source.GetUnderlying<ISorted<T>>();
            if (sortedSource != null && Equals(sortedSource.SortOrder, comparison))
            {
                // already sorted
                return source;
            }

            return source.UseSafely(s => SortInternal(comparison, s));
        }

        private IEnumerator<T> SortInternal<T>(IComparer<T> comparison, IEnumerator<T> source)
        {
            var batchSize = batchingPolicy.GetBatchSize(source);
            // prepare to read the source set in batches.
            return source.Batch(batchSize).UseSafely(batches =>
            {
                // for each batch, sort it.
                var sortedBatches = SortBatches(batches, comparison, () => accumulatorSelector.GetAccumulator<T>(source)).EvaluateSafely();
                if (!sortedBatches.Any()) return Enumerable.Empty<T>().GetEnumerator().UsesSortOrder(comparison);
                return MergeBatches(sortedBatches);
            });
        }

        private static IEnumerator<T> MergeBatches<T>(IList<IEnumerator<T>> sortedBatches)
        {
            return new SetUnionSortPreservingMerge<T>().Merge(sortedBatches);
        }

        private IEnumerable<IEnumerator<T>> SortBatches<T>(IEnumerator<IEnumerable<T>> batches, IComparer<T> comparison, Func<IAccumulator<T>> getBatchAccumulator)
        {
            while (batches.MoveNext())
            {
                yield return getBatchAccumulator().UseSafely(a =>
                    batches.Current
                        .OrderBy(i => i, comparison)
                        .GetEnumerator()
                        .BufferOnce(a)
                        .UsesSortOrder(comparison));
            }
        }


    }
}
