using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.Core.Metadata
{
    public class TypeUsage : MetadataItem
    {
        private readonly EdmType _edmType;

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.TypeUsage; }
        }

        public virtual EdmType EdmType
        {
            get { return _edmType; }
        }

        internal TypeUsage()
        {
        }

        private TypeUsage(EdmType edmType)
        {
            Check.NotNull(edmType, "edmType");

            _edmType = edmType;
        }

        internal static TypeUsage Create(EdmType edmType)
        {
            return new TypeUsage(edmType);
        }

        internal TypeUsage GetElementTypeUsage()
        {
            return Create(EdmType);
        }

        internal IEnumerable<EdmProperty> GetProperties()
        {
            switch (EdmType.BuiltInTypeKind)
            {
                case Metadata.BuiltInTypeKind.EntityType:
                    return ((EntityType)EdmType).Properties;
                default:
                    return Enumerable.Empty<EdmProperty>();
            }
        }

        internal PrimitiveTypeKind GetPrimitiveTypeKind()
        {
            return ((PrimitiveType)EdmType).PrimitiveTypeKind;
        }

        internal bool IsPrimitiveType()
        {
            return EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType;
        }

        internal bool TryGetIsUnicode(out bool isUnicode)
        {
            throw new NotImplementedException();
        }
    }
}
