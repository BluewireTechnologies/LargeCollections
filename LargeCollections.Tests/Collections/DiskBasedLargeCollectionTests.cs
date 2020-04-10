using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using LargeCollections.Core;
using LargeCollections.Core.Storage;
using LargeCollections.FileSystem;
using NUnit.Framework;

namespace LargeCollections.Tests.Collections
{
    [TestFixture, CheckResources]
    public class DiskBasedLargeCollectionTests : BaselineTestsForLargeCollectionWithBackingStore<FileReference>
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
        
        [Test]
        public void AccumulatorDoesNotOverwriteNonemptyExistingFile()
        {
            var fileName = Path.GetTempFileName();
            var content = "test";
            File.WriteAllText(fileName, content);

            Assert.Catch<InvalidOperationException>(() => new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()));
            Assert.AreEqual(content, File.ReadAllText(fileName));
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
        public void AccumulatorDoesNotLeakResources_If_ExceptionOccursInConstructor()
        {
            var fileName = GetUnwritableTempFile();
            
            Assert.Catch<UnauthorizedAccessException>(() =>
            {
                using (new FileAccumulator<int>(fileName, new DefaultItemSerialiser<int>()))
                {
                }
            });

            Assert.IsFalse(File.Exists(fileName));
            Utils.AssertReferencesDisposed();
        }

        protected override LargeCollectionTestHarness<FileReference> CreateHarness()
        {
            return new Harness();
        }
    }
}
