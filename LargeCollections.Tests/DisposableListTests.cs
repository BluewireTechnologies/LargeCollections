using System;
using System.Linq;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests
{
    [TestFixture]
    public class DisposableListTests
    {
        [Test]
        public void AllItemsAreDisposed_When_ListIsDisposed()
        {
            var disposables = new [] {
                MockDisposable(),
                MockDisposable(),
                MockDisposable()
            };
            var list = new DisposableList<IDisposable>(disposables.Select(d => d.Object));
            
            list.Dispose();

            foreach(var d in disposables)
            {
                d.Verify();
            }
        }

        [Test]
        public void ExceptionDuringDispose_DoesNotPrevent_AllItemsBeingDisposed()
        {
            var disposables = new[] {
                MockDisposable(),
                MockDisposableThrowingException(),
                MockDisposable()
            };
            var list = new DisposableList<IDisposable>(disposables.Select(d => d.Object));

            list.Dispose();

            foreach (var d in disposables)
            {
                d.Verify();
            }
        }

        private Mock<IDisposable> MockDisposableThrowingException()
        {
            var mock = new Mock<IDisposable>();
            mock.Setup(m => m.Dispose()).Throws(new Exception());
            return mock;
        }

        private Mock<IDisposable> MockDisposable()
        {
            var mock = new Mock<IDisposable>();
            mock.Setup(m => m.Dispose()).Verifiable();
            return mock;
        }
    }
}
