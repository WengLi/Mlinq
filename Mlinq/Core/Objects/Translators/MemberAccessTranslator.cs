using Mlinq.Core.LinqPredicate;
using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed class MemberAccessTranslator : TypedTranslator<MemberExpression>
    {
        internal MemberAccessTranslator()
            : base(ExpressionType.MemberAccess)
        {
        }

        protected override Predicate TypedTranslate(PredicateConverter parent, MemberExpression linq)
        {
            Predicate propertyExpression;
            string memberName;
            Type memberType;
            var memberInfo = linq.Member.GetPropertyOrField(out memberName, out memberType);

            if (linq.Expression != null)
            {
                if (ExpressionType.Constant == linq.Expression.NodeType)
                {
                    var constantExpression = (ConstantExpression)linq.Expression;
                    if (constantExpression.Type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).FirstOrDefault() != null)
                    {
                        var valueDelegate = Expression.Lambda(linq).Compile();

                        return parent.TranslateExpression(Expression.Constant(valueDelegate.DynamicInvoke()));
                    }
                }
                var instance = parent.TranslateExpression(linq.Expression);
                if (TryResolveAsProperty(parent, memberInfo, instance, out propertyExpression))
                {
                    return propertyExpression;
                }
            }
            throw new Exception();
        }

        private bool TryResolveAsProperty(PredicateConverter parent, MemberInfo clrMember, Predicate instance, out Predicate propertyExpression)
        {
            var name = clrMember.Name;
            propertyExpression = instance.Property(name);
            return true;
        }
    }
}
