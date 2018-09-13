using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public abstract class EdmType : MetadataItem
    {
        private string _name;
        private string _namespace;
        private EdmType _baseType;
        public bool Abstract { get; set; }

        public virtual string Name
        {
            get { return _name; }
            internal set
            {
                _name = value;
            }
        }

        public virtual String NamespaceName
        {
            get { return _namespace; }
            internal set
            {
                _namespace = value;
            }
        }

        public virtual EdmType BaseType
        {
            get { return _baseType; }
            internal set
            {
                CheckBaseType(value);

                _baseType = value;
            }
        }

        internal EdmType()
        {
        }

        internal EdmType(string name, string namespaceName)
        {
            Check.NotNull(name, "name");
            Check.NotNull(namespaceName, "namespaceName");
            _name = name;
            _namespace = namespaceName;
        }

        internal static void Initialize(EdmType type, string name, string namespaceName, bool isAbstract, EdmType baseType)
        {
            type._baseType = baseType;
            type._name = name;
            type._namespace = namespaceName;
            type.Abstract = isAbstract;
        }


        private void CheckBaseType(EdmType baseType)
        {
            for (var type = baseType; type != null; type = type.BaseType)
            {
                if (type == this)
                {
                    throw new ArgumentException();
                }
            }

            if (baseType != null && baseType.BuiltInTypeKind == BuiltInTypeKind.EntityType && ((EntityType)baseType).Properties.Count != 0 && ((EntityType)this).Properties.Count != 0)
            {
                throw new ArgumentException();
            }
        }
    }
}
