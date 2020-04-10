using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Bluewire.ReferenceCounting.Tests
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckResourcesAttribute : Attribute, ITestAction
    {
        public void AfterTest(ITest testDetails)
        {
            Utils.AssertReferencesDisposed();
        }

        public void BeforeTest(ITest testDetails)
        {
            // cleanup leftovers from any previous tests.
            ReferenceCountedResource.Diagnostics.Reset();
        }

        public ActionTargets Targets
        {
            get { return ActionTargets.Test | ActionTargets.Suite; }
        }
    }
}
