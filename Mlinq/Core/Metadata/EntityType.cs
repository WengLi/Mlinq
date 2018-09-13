using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class EntityType  : EdmType
    {
        private readonly List<EdmProperty> _properties = new List<EdmProperty>();

        public virtual ReadOnlyCollection<EdmProperty> Properties
        {
            get
            {
                return new ReadOnlyCollection<EdmProperty>(_properties);
            }
        }

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntityType; }
        }

        internal EntityType(string name, string namespaceName, IEnumerable<EdmProperty> members)
            : base(name, namespaceName)
        {
            if (null != members)
            {
                foreach (var member in members)
                {
                    _properties.Add(member);
                }
            }
        }

        public void AddProperty(EdmProperty property)
        {
            _properties.Add(property);
        }

        public static EntityType Create(string name, string namespaceName, IEnumerable<EdmProperty> members)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");
            var entity = new EntityType(name, namespaceName, members);
            return entity;
        }

        public static EntityType Create(string name, string namespaceName, EntityType baseType, IEnumerable<EdmProperty> members)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");
            Check.NotNull(baseType, "baseType");
            var entity = new EntityType(name, namespaceName, members) { BaseType = baseType };
            return entity;
        }
    }
}
