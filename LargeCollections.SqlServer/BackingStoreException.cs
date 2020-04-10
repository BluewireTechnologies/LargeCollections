using System;
using System.IO;

namespace LargeCollections.SqlServer
{
    public class BackingStoreException : IOException
    {
        public BackingStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}