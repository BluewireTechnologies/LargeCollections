using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using LargeCollections.Collections;
using LargeCollections.Resources;
using LargeCollections.Storage;
using MbUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    [TestFixture, CheckResources]
    public class DiskBasedLargeCollectionTests
    {
        private static ILargeCollection<int> GetCollection(IEnumerable<int> values)
        {
            using(var accumulator = new FileAccumulator<int>(Path.GetTempFileName(), new DefaultItemSerialiser<int>()))
            {
                accumulator.AddRange(values);
                return accumulator.Complete();
            }
        }

        [Test]
        public void AccumulatorCleansUpBackingStore_If_NoCollectionIsCreated()
        {
            var fileName = Path.GetTempFileName();
            using (var accumulator = new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()))
            {
                accumulator.Add(1);
            }
            Assert.IsFalse(File.Exists(fileName));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccumulatorDoesNotOverwriteNonemptyExistingFile()
        {
            var fileName = Path.GetTempFileName();
            var content = "test";
            File.WriteAllText(fileName, content);
            try
            {
                using(new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()))
                {
                }
            }
            finally
            {
                Assert.AreEqual(content, File.ReadAllText(fileName));
            }
        }

        private string GetUnwritableTempFile()
        {
            var fileName = Path.GetTempFileName();
            var security = new FileSecurity();
            security.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.WriteData, AccessControlType.Deny));
            File.SetAccessControl(fileName, security);
            return fileName;
        }

        [Test]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void AccumulatorDoesNotLeakResources_If_ExceptionOccursInConstructor()
        {
            var fileName = GetUnwritableTempFile();
            try
            {
                using (new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()))
                {
                }
            }
            finally
            {
                Assert.IsFalse(File.Exists(fileName));
                Utils.AssertReferencesDisposed();
            }
        }

        [Test]
        public void CanSafelyDisposeMultipleTimes()
        {
            using (var collection = GetCollection(new[] { 1, 2, 3 }))
            {
                collection.Dispose();
            }
        }


        [Test]
        public void CleansUpBackingStore_WhenDisposed()
        {
            using (var collection =  GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
                collection.Dispose();
                Assert.IsFalse(collection.GetBackingStore<FileReference>().File.Exists);
            }
        }


        [Test]
        public void DoesNotCleanUpBackingStore_WhenIterationIsComplete()
        {
            using (var collection = GetCollection(new[] { 1, 2, 3 }))
            {
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
                collection.ToArray();
                Assert.IsTrue(collection.GetBackingStore<FileReference>().File.Exists);
            }
        }
    }
}
