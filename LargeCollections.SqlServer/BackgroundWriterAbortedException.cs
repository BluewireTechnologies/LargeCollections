using System;

namespace LargeCollections.SqlServer
{
    public class BackgroundWriterAbortedException : Exception
    {
        public BackgroundWriterAbortedException(Exception inner) : base("Background writer task aborted due to an exception.", inner)
        {
        }
    }
}
