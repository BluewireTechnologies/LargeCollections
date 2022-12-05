using System.IO;

namespace LargeCollections.Core.Storage
{
    public class SerialisedSizeEstimator
    {
        private readonly SerialiserSelector selector;

        public SerialisedSizeEstimator(SerialiserSelector selector)
        {
            this.selector = selector;
        }

        public long EstimateRecordSize<T>() where T : new()
        {
            var obj = new T();
            return EstimateRecordSize(obj);
        }

        public long EstimateRecordSize<T>(params T[] sampleRecords)
        {
            var serialiser = selector.Get<T>();
            using (var ms = new MemoryStream())
            {
                foreach (var record in sampleRecords)
                {
                    serialiser.Write(ms, record);
                }
                return ms.Length / sampleRecords.Length;
            }
        }
    }
}
