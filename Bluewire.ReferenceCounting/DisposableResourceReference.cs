using System;

namespace Bluewire.ReferenceCounting
{
    public class DisposableResourceReference<T> : ReferenceCountedResource where T : class, IDisposable
    {
        public T Resource {get; private set;}

        public DisposableResourceReference(T resource)
        {
            this.Resource = resource;
        }

        protected override void CleanUp()
        {
            Resource.Dispose();
            Resource = null;
            base.CleanUp();
        }
    }
}
