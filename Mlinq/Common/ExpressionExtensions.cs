using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Common
{
    internal static class ExpressionExtensions
    {
        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        public static bool IsNullConstant(this Expression expression)
        {
            expression = expression.RemoveConvert();

            if (expression.NodeType != ExpressionType.Constant)
            {
                return false;
            }

            return ((ConstantExpression)expression).Value == null;
        }

        public static bool IsStringAddExpression(this Expression expression)
        {
            var linq = expression as BinaryExpression;
            if (linq == null)
            {
                return false;
            }

            if (linq.Method == null || linq.NodeType != ExpressionType.Add)
            {
                return false;
            }

            return linq.Method.DeclaringType == typeof(string) && string.Equals(linq.Method.Name, "Concat", StringComparison.Ordinal);
        }
    }
}
