using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class RowType : EdmType
    {
        private readonly List<EdmProperty> _properties = new List<EdmProperty>();
         private readonly NewExpression _newExpression;

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.RowType; }
        }

        public NewExpression NewExpression
        {
            get { return _newExpression; }
        }

        public virtual ReadOnlyCollection<EdmProperty> Properties
        {
            get { return new ReadOnlyCollection<EdmProperty>(_properties); }
        }

        internal RowType()
        {
        }

        internal RowType(IEnumerable<EdmProperty> properties, NewExpression newExpression)
            : base("", EdmConstants.TransientNamespace)
        {
            _newExpression = newExpression;
            if (null != properties)
            {
                foreach (var property in properties)
                {
                    AddProperty(property);
                }
            }
        }

        private void AddProperty(EdmProperty property)
        {
            Check.NotNull(property, "property");
            _properties.Add(property);
        }

    }
}
