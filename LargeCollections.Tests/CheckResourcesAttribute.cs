using System;
using LargeCollections.Resources;
using NUnit.Framework;

namespace LargeCollections.Tests
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckResourcesAttribute : Attribute, ITestAction
    {
        public void AfterTest(TestDetails testDetails)
        {
            Utils.AssertReferencesDisposed();
        }

        public void BeforeTest(TestDetails testDetails)
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
