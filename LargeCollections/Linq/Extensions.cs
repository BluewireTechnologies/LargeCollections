using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LargeCollections.Collections;
using LargeCollections.Operations;
using LargeCollections.Resources;

namespace LargeCollections.Linq
{
    public static class Extensions
    {
        public static IEnumerator<IEnumerable<T>> Batch<T>(this IEnumerator<T> enumerator, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(enumerator, batchSize).InheritsCount(enumerator);
        }

        public static IEnumerator<IEnumerable<T>> Batch<T>(this ILargeCollection<T> collection, int batchSize)
        {
            return new BatchedSinglePassCollection<T>(collection.GetEnumerator(), batchSize).InheritsCount(collection);
        }

        
        public static ILargeCollection<T> Concat<T>(this ILargeCollection<T> first, ILargeCollection<T> second)
        {
            return new ConcatenatedLargeCollection<T>(first, second);
        }

        public static IEnumerator<T> Concat<T>(this IEnumerator<T> first, IEnumerator<T> second)
        {
            return new ConcatenatedEnumerator<T>(first, second).InheritsCount(first, second);
        }

        public static IDisposableEnumerable<T> BufferInMemory<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.Buffer(new InMemoryAccumulator<T>());
        }

        public static IDisposableEnumerable<T> Buffer<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            return accumulator.UseSafely(a =>
                enumerator.UseSafely(e =>
                {
                    while (e.MoveNext())
                    {
                        a.Add(e.Current);
                    }
                    return a.Complete().InheritsSortOrder(e);
                }));
        }

        public static IEnumerator<T> BufferOnce<T>(this IEnumerator<T> enumerator, IAccumulator<T> accumulator)
        {
            return enumerator.Buffer(accumulator).UseSafely(b => b.GetEnumerator());
        }

        public static IDisposableEnumerable<T> AsDisposable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IDisposableEnumerable<T> ?? new DisposableEnumerable<T>(enumerable);
        }

        public static IList<T> EvaluateSafely<T>(this IEnumerable<T> enumerable)
        {
            using(var list = new DisposableList<T>())
            {
                foreach(var entry in enumerable)
                {
                    list.Add(entry);
                }
                var allEntries = list.ToArray();
                list.Clear(); // prevent disposing, since we successfully evaluated the enumerable.
                return allEntries;
            }
        }

        public static IList<TReturn> EvaluateSafely<T, TReturn>(this IEnumerable<T> enumerable, Func<T, TReturn> map)
        {
            using (var list = new DisposableList<object>())
            {
                var firstStageEvaluation = enumerable.EvaluateSafely();
                list.AddRange(firstStageEvaluation.Cast<object>());
                var outputList = new List<TReturn>();
                foreach (var entry in firstStageEvaluation)
                {
                    var mapped = map(entry);
                    list.Add(mapped);
                    outputList.Add(mapped);
                }
                list.Clear(); // prevent disposing, since we successfully evaluated the enumerable.
                return outputList;
            }
        }

        public static long Count<T>(this IEnumerator<T> enumerator)
        {
            var counted = enumerator.GetUnderlying<ICounted>();
            if(counted == null) throw new InvalidOperationException("Not a counted enumerator.");
            return counted.Count;
        }

        /// <summary>
        /// Safely implements a 'using' block returning an IDisposable result.
        /// Note that this cannot easily be used to replace coroutines! Try using GuardedDisposalEnumerator instead.
        /// </summary>
        /// <remarks>
        /// Given:
        ///      using(a) { return new B(); }
        /// Where:
        ///      class B : IDisposable
        /// Then:
        ///      The instance of B will never be disposed if a.Dispose() throws.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TReturn UseSafely<T, TReturn>(this T disposable, Func<T, TReturn> func) where T : IDisposable where TReturn : class, IDisposable
        {
            TReturn result = null;
            try
            {
                using (disposable)
                {
                    result = func(disposable);
                }
            }
            catch (Exception)
            {
                if(result != null) result.Dispose();
                throw;
            }
            return result;
        }
    }

    public class DisposableEnumerable<T> : IDisposableEnumerable<T>, IHasUnderlying
    {
        private readonly IEnumerable<T> enumerable;

        public DisposableEnumerable(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if(enumerable is IDisposable) ((IDisposable)enumerable).Dispose();
        }

        public object Underlying
        {
            get { return enumerable; }
        }
    }
}
