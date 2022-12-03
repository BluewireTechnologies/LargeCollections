using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LargeCollections.SqlServer
{
    class Utils
    {
        public static MemberInfo GetReferencedMemberOrNull<TObject, TMember>(Expression<Func<TObject, TMember>> getFromMember)
        {
            var body = getFromMember.Body;
            if (body is UnaryExpression)
            {
                body = ((UnaryExpression)body).Operand;
            }
            if (body is MemberExpression)
            {
                return ((MemberExpression) body).Member;
            }
            if (body is MethodCallExpression)
            {
                return ((MethodCallExpression)body).Method;
            }
            return null;
        }

        public static Action<T, TProp> TryCreateSetter<T, TProp>(MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)member;
                if (field.IsStatic || !field.IsPublic || field.IsInitOnly) return null;
                return (obj, prop) => field.SetValue(obj, prop);
            }
            if (member.MemberType == MemberTypes.Property)
            {
                var property = ((PropertyInfo)member);
                var propertySetter = property.GetSetMethod();
                if (propertySetter == null || propertySetter.IsStatic || !propertySetter.IsPublic) return null;
                return (obj, prop) => property.SetValue(obj, prop, new object[0]);
            }
            return null;
        }
    }
}
