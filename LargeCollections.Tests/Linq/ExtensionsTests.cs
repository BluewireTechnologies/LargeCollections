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
                if (item is Exception) throw (Exception)item;
                yield return item;
            }
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
                    b.Object,
                    new InvalidOperationException()
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

        [Test]
        public void SafelyEvaluatingMappedEnumerable_DoesNotDisposeAnything_IfNoExceptionIsThrown()
        {
            var a = MockDisposable();
            var b = MockDisposable();

            new[] {
                a.Object,
                new object(), 
                b.Object
            }.EvaluateSafely(e => e);

            a.Verify(d => d.Dispose(), Times.Never());
            b.Verify(d => d.Dispose(), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SafelyEvaluatingMappedEnumerable_DisposesEverything_IfExceptionIsThrownBySourceEnumeration()
        {
            var a = MockDisposable();
            var b = MockDisposable();
            var mapped = MockDisposable();

            try
            {
                CreateEnumerableThrowingException(
                    a.Object,
                    new InvalidOperationException(),
                    b.Object
                ).EvaluateSafely(e => mapped.Object);
            }
            finally
            {
                a.Verify(d => d.Dispose());
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SafelyEvaluatingMappedEnumerable_DisposesEverything_IncludingMappedValues_IfExceptionIsThrownByMapping()
        {
            var a = MockDisposable();
            var b = MockDisposable();
            var mapped = MockDisposable();

            try
            {
                CreateEnumerableThrowingException(
                    a.Object,
                    new object(),
                    b.Object
                    ).EvaluateSafely(e =>
                    {
                        if (e is IDisposable) return mapped.Object;
                        throw new InvalidOperationException();
                    });
            }
            finally
            {
                a.Verify(d => d.Dispose());
                b.Verify(d => d.Dispose());
                mapped.Verify(d => d.Dispose());
            }
        }
    }
}
