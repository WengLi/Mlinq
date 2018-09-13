using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class EdmProperty : MetadataItem
    {
        public EntityType DeclaringType { get; set; }
        public TypeUsage TypeUsage { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public Type EntityDeclaringType { get; set; }
        public string Name { get; set; }
        public Func<object, object> ValueGetter { get; set; }
        public Action<object, object> ValueSetter { get; set; }
        public bool Nullable { get; set; }
        public object DefaultValue { get; set; }

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EdmProperty; }
        }

        internal EdmProperty()
        { }

        internal EdmProperty(string name, TypeUsage typeUsage)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");

            Name = name;
            TypeUsage = typeUsage;
        }

    }
}
