
Large Collections Library
=========================

 * ILargeCollection<T> is immutable.
 * ILargeCollection<T> is created by an IAccumulator<T>
 * IAccumulator<T> is used to gather data which will be baked into the collection.
 * It is always the responsibility of the caller to dispose an IEnumerable<T>.
 * It is always the responsibility of the callee to dispose an IEnumerator<T> argument, UNLESS the same instance is returned unchanged.
 
 * If a class wraps a disposable collection object:
   * If the class disposes the underlying collection, nothing else must do that, and the class must not live longer than the underlying collection.
      For example: using(var sorted = GetEnumerator().UsesSortOrder(Comparer<T>.Default)) { ... }
   * If the class does not dispose the underlying collection, it should acquire references to its resources and dispose them at the appropriate time.
      For example: using(var collection = accumulator.Complete()) { return new SinglePassLargeCollection<T>(collection); }