using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Common.Reflection;
using Gallio.Framework.Pattern;
using LargeCollections.Resources;
using MbUnit.Framework;

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
                actions.TestInstanceActions.SetUpTestInstanceChain.Before(_ => ReferenceCountedResource.GetLeakedResources());

                actions.TestInstanceActions.TearDownTestInstanceChain.After(_ => Utils.AssertReferencesDisposed());
            });
        }
    }
}
