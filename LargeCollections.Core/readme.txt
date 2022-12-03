
Large Collections Library
=========================

 * ILargeCollection<T> is immutable.
 * ILargeCollection<T> is created by an IAccumulator<T>
 * IAccumulator<T> is used to gather data which will be baked into the collection.
 * It is always the responsibility of the caller to dispose an IEnumerable<T>.
 * It is always the responsibility of the callee to dispose an IEnumerator<T> argument, UNLESS the same instance is returned unchanged.
 
 * If a class wraps a disposable collection object:
   * If the class disposes the underlying collection, nothing else must do that, and the class must not live longer than the underlying collection.
      For example: using (var sorted = GetEnumerator().UsesSortOrder(Comparer<T>.Default)) { ... }
   * If the class does not dispose the underlying collection, it should acquire references to its resources and dispose them at the appropriate time.
      For example: return accumulator.Complete().UseSafely(a => new SinglePassLargeCollection<T>(a));

 * Be very wary of disjoint 'using' blocks. Returning a disposable instance from inside a 'using' block is generally unsafe since
   the returned disposable will never be disposed if the 'used' instance's Dispose() method throws. This is obvious when one considers
   how 'using' translates to try..finally.
 * If you must return a disposable from a 'using' block, replace the block with a .UseSafely(u => ...) call instead.