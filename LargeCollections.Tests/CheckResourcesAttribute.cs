using System;
using Gallio.Common.Reflection;
using Gallio.Framework.Pattern;
using LargeCollections.Resources;

namespace LargeCollections.Tests
{
    [AttributeUsage(PatternAttributeTargets.TestType, AllowMultiple = true, Inherited = true)]
    public class CheckResourcesAttribute : TestTypeDecoratorPatternAttribute
    {
        protected override void DecorateTest(IPatternScope scope, ITypeInfo type)
        {
            scope.TestBuilder.TestInstanceActions.DecorateChildTestChain.Before((state, actions) =>
            {
                // cleanup leftovers from any previous tests.
                actions.TestInstanceActions.SetUpTestInstanceChain.Before(_ => ReferenceCountedResource.Diagnostics.Reset());

                actions.TestInstanceActions.TearDownTestInstanceChain.After(_ => Utils.AssertReferencesDisposed());
            });
        }
    }
}
