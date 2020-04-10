using System;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.ReferenceCounting
{
    public static class Extensions
    {
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
}
