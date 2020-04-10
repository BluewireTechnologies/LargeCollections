using System;
using System.IO;

namespace LargeCollections.Storage.Database
{
    public class BackingStoreException : IOException
    {
        public BackingStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}