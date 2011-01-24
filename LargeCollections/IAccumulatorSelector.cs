using System.Collections;

namespace LargeCollections
{
    public interface IAccumulatorSelector
    {
        /// <summary>
        /// Get an accumulator suitable for a set of the specified size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="totalSizeOfCollection"></param>
        /// <returns></returns>
        IAccumulator<T> GetAccumulator<T>(long totalSizeOfCollection);

        /// <summary>
        /// Get an accumulator for a set of unknown size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAccumulator<T> GetAccumulator<T>();

        /// <summary>
        /// Get an accumulator tailored to a set of similar size to that provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAccumulator<T> GetAccumulator<T>(IEnumerator source);
    }
}