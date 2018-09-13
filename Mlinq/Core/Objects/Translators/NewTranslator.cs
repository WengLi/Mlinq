using System;
using Mlinq.Common;
using Mlinq.Core.LinqPredicate;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Mlinq.Core.Metadata;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed class NewTranslator : TypedTranslator<NewExpression>
    {
        internal NewTranslator()
            : base(ExpressionType.New)
        {
        }

        protected override Predicate TypedTranslate(PredicateConverter parent, NewExpression linq)
        {
            var memberCount = null == linq.Members ? 0 : linq.Members.Count;

            if (null == linq.Constructor || linq.Arguments.Count != memberCount)
            {
                throw new Exception();
            }

            var recordColumns = new List<KeyValuePair<string, Predicate>>(memberCount + 1);

            var memberNames = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < memberCount; i++)
            {
                string memberName;
                Type memberType;
                var memberInfo = linq.Members[i].GetPropertyOrField(out memberName, out memberType);
                var memberValue = parent.TranslateExpression(linq.Arguments[i]);
                memberNames.Add(memberName);
                recordColumns.Add(new KeyValuePair<string, Predicate>(memberName, memberValue));
            }

            var projection = CreateNewRowExpression(recordColumns, linq);

            return projection;
        }

        private NewInstancePredicate CreateNewRowExpression(List<KeyValuePair<string, Predicate>> columns, NewExpression linq)
        {
            //var propertyValues = new List<Predicate>(columns.Count);
            //for (var i = 0; i < columns.Count; i++)
            //{
            //    var column = columns[i];
            //    propertyValues.Add(column.Value);
            //}
            //var rowType = new RowType(properties, initializerMetadata)
            //return new NewInstancePredicate(null, new PredicateList(propertyValues));

            var propertyValues = new List<Predicate>(columns.Count);
            var properties = new List<EdmProperty>(columns.Count);
            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                propertyValues.Add(column.Value);
                properties.Add(new EdmProperty(column.Key, column.Value.ResultType));
            }
            var rowType = new RowType(properties, linq);
            var typeUsage = TypeUsage.Create(rowType);
            return typeUsage.New(propertyValues);
        }
    }
}
