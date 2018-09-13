using Mlinq.Core.Metadata;
using System;

namespace Mlinq.Internal
{
    internal class EntitySetTypePair : Tuple<EntitySet, Type>
    {
        public EntitySetTypePair(EntitySet entitySet, Type type)
            : base(entitySet, type)
        {
        }

        public EntitySet EntitySet
        {
            get { return Item1; }
        }

        public Type BaseType
        {
            get { return Item2; }
        }
    }
}
