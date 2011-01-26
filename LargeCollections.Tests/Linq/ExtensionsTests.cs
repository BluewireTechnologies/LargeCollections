using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Linq;
using MbUnit.Framework;
using Moq;

namespace LargeCollections.Tests.Linq
{
    [TestFixture, CheckResources]
    public class ExtensionsTests
    {
        private IEnumerable<object> CreateEnumerableThrowingException(params object[] items)
        {
            foreach(var item in items)
            {
                yield return item;
            }
            throw new InvalidOperationException();
        }

        public Mock<IDisposable> MockDisposable()
        {
            var disposable = new Mock<IDisposable>();
            disposable.Setup(d => d.Dispose());
            return disposable;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SafelyEvaluatingEnumerable_DisposesEverything_IfExceptionIsThrown()
        {
            var a = MockDisposable();
            var b = MockDisposable();

            try
            {
                CreateEnumerableThrowingException(
                    a.Object,
                    new object(), 
                    b.Object
                ).EvaluateSafely();
            }
            finally
            {
                a.Verify(d => d.Dispose());
                b.Verify(d => d.Dispose());
            }
        }

        [Test]
        public void SafelyEvaluatingEnumerable_DoesNotDisposeAnything_IfNoExceptionIsThrown()
        {
            var a = MockDisposable();
            var b = MockDisposable();

            new[] {
                a.Object,
                new object(), 
                b.Object
            }.EvaluateSafely();

            a.Verify(d => d.Dispose(), Times.Never());
            b.Verify(d => d.Dispose(), Times.Never());
        }
    }
}
