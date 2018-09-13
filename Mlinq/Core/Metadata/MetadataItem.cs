using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Metadata
{
    public abstract partial class MetadataItem
    {
        public abstract BuiltInTypeKind BuiltInTypeKind { get; }

        internal MetadataItem()
        {
        }
    }
}
