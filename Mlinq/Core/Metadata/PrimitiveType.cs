using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class PrimitiveType : EdmType
    {
        private PrimitiveTypeKind _primitiveTypeKind;

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.PrimitiveType; }
        }

        internal Type ClrType
        {
            get { return ClrEquivalentType; }
        }

        public virtual PrimitiveTypeKind PrimitiveTypeKind
        {
            get { return _primitiveTypeKind; }
            internal set { _primitiveTypeKind = value; }
        }

        public Type ClrEquivalentType
        {
            get
            {
                switch (PrimitiveTypeKind)
                {
                    case PrimitiveTypeKind.Binary:
                        return typeof(byte[]);
                    case PrimitiveTypeKind.Boolean:
                        return typeof(bool);
                    case PrimitiveTypeKind.Byte:
                        return typeof(byte);
                    case PrimitiveTypeKind.DateTime:
                        return typeof(DateTime);
                    case PrimitiveTypeKind.Time:
                        return typeof(TimeSpan);
                    case PrimitiveTypeKind.DateTimeOffset:
                        return typeof(DateTimeOffset);
                    case PrimitiveTypeKind.Decimal:
                        return typeof(decimal);
                    case PrimitiveTypeKind.Double:
                        return typeof(double);
                    case PrimitiveTypeKind.Guid:
                        return typeof(Guid);
                    case PrimitiveTypeKind.Single:
                        return typeof(Single);
                    case PrimitiveTypeKind.SByte:
                        return typeof(sbyte);
                    case PrimitiveTypeKind.Int16:
                        return typeof(short);
                    case PrimitiveTypeKind.Int32:
                        return typeof(int);
                    case PrimitiveTypeKind.Int64:
                        return typeof(long);
                    case PrimitiveTypeKind.String:
                        return typeof(string);
                }

                return null;
            }
        }

        internal PrimitiveType()
        {
        }

        internal PrimitiveType(string name, string namespaceName, PrimitiveType baseType)
            : base(name, namespaceName)
        {
            Check.NotNull(baseType, "baseType");
            BaseType = baseType;
            Initialize(this, baseType.PrimitiveTypeKind);
        }

        internal PrimitiveType(Type clrType, PrimitiveType baseType)
            : this(Check.NotNull(clrType, "clrType").Name, clrType.NestingNamespace(), baseType)
        {
        }

        internal static void Initialize(PrimitiveType primitiveType, PrimitiveTypeKind primitiveTypeKind)
        {
            primitiveType._primitiveTypeKind = primitiveTypeKind;
        }

    }
}
