using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public class CollectionType : EdmType
    {
        private readonly TypeUsage _typeUsage;

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.CollectionType; }
        }

        public virtual TypeUsage TypeUsage
        {
            get { return _typeUsage; }
        }

        internal CollectionType()
        {
        }

        internal CollectionType(EdmType elementType)
            : this(TypeUsage.Create(elementType))
        {
        }

        internal CollectionType(TypeUsage elementType)
            : base(GetIdentity(Check.NotNull(elementType, "elementType")), EdmConstants.TransientNamespace)
        {
            _typeUsage = elementType;
        }

        private static string GetIdentity(Metadata.TypeUsage typeUsage)
        {
            var builder = new StringBuilder(50);
            builder.Append("collection[");
            builder.Append("]");
            return builder.ToString();
        }
    }
}
