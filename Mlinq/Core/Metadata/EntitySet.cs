using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class EntitySet : MetadataItem
    {
        public EntityContainer EntityContainer { get { return new EntityContainer(); } }
        public String Name { get; set; }
        public string Table { get; set; }
        public string Schema { get; set; }
        private EntityType _elementType;

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntitySet; }
        }

        public EntityType ElementType
        {
            get { return _elementType; }
            internal set
            {
                Check.NotNull(value, "value");
                _elementType = value;
            }
        }

        internal EntitySet()
        {
        }

        internal EntitySet(string name, string schema, string table)
        {
            Name = name;
            Schema = schema;
            Table = table;
        }

        internal EntitySet(string name, string schema, string table, EntityType entityType)
        {
            Name = name;
            Schema = schema;
            Table = table;
            _elementType = entityType;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EntityContainer
    {
        public string Name { get { return ""; } }
    }
}
