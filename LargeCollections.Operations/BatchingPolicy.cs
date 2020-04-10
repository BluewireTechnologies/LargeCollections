using System;
using System.Collections.Generic;
using LargeCollections.Core;

namespace LargeCollections.Operations
{
    public class BatchingPolicy
    {
        /// <summary>
        /// Smallest in-memory batch size.
        /// </summary>
        private readonly int minimumBatchSize = MIN_BATCH_SIZE;
        /// <summary>
        /// Largest in-memory batch size.
        /// </summary>
        private readonly int maximumBatchSize = MAX_BATCH_SIZE;
        /// <summary>
        /// Batch size used for collections of unknown total size.
        /// </summary>
        private readonly int fallbackBatchSize = FALLBACK_BATCH_SIZE;
        /// <summary>
        /// Weighting factor. If this is high, few large batches will be preferred. If this is low, many small ones will be preferred.
        /// </summary>
        private readonly double batchWeighting = BATCH_WEIGHTING;

        public BatchingPolicy(int minimumBatchSize): this()
        {
            this.minimumBatchSize = minimumBatchSize;
        }

        public BatchingPolicy()
        {
        }

        private const int MIN_BATCH_SIZE = 10000;
        private const int FALLBACK_BATCH_SIZE = 100000;
        private const int MAX_BATCH_SIZE = 1000000;
        private const double BATCH_WEIGHTING = 0.7;

        private int GetBatchSize(ICounted counted)
        {
            if (counted == null)
            {
                return fallbackBatchSize;
            }
            return (int)Math.Min(Math.Max(Math.Pow(counted.Count, batchWeighting), minimumBatchSize), maximumBatchSize);
        }

        public int GetBatchSize<T>(IEnumerator<T> source)
        {
            return GetBatchSize(source.GetUnderlying<ICounted>());
        }

        public int GetBatchSize<T>(IEnumerable<T> source)
        {
            return GetBatchSize(source.GetUnderlying<ICounted>());
        }
    }
}