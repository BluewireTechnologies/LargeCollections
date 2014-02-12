using System;

namespace LargeCollections.Storage.Database
{
    public class BackgroundWriterAbortedException : Exception
    {
        public BackgroundWriterAbortedException(Exception inner) : base("Background writer task aborted due to an exception.", inner)
        {
        }
    }
}