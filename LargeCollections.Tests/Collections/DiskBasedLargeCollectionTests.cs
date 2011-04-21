using System;
using System.Collections.Generic;
using System.Data;
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
        class Harness : LargeCollectionTestHarness<FileReference>
        {
            public override IAccumulator<int> GetAccumulator()
            {
                var fileName = Path.GetTempFileName();
                return new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>());
            }

            public override bool BackingStoreExists(IAccumulator<int> accumulator)
            {
                return File.Exists(((FileAccumulator<int>)accumulator).FileName);
            }

            public override bool BackingStoreExists(ILargeCollection<int> collection)
            {
                return collection.GetUnderlying<IHasBackingStore<FileReference>>().BackingStore.File.Exists;
            }
        }

        [DynamicTestFactory]
        public IEnumerable<Test> BaselineTests()
        {
            return new BaselineTestsForLargeCollectionWithBackingStore<FileReference>().GetTests(() => new Harness());
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
    }
}
