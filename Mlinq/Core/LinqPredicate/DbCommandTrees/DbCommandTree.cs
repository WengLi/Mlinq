using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlinq.Core.LinqPredicate.DbCommandTrees;

namespace Mlinq.Core.LinqPredicate
{
    public abstract class DbCommandTree
    {
        public abstract DbCommandTreeKind CommandTreeKind { get; }

        private readonly bool _useDatabaseNullSemantics;

        public bool UseDatabaseNullSemantics
        {
            get { return _useDatabaseNullSemantics; }
        }

        internal DbCommandTree()
        {
            _useDatabaseNullSemantics = true;
        }

        internal DbCommandTree(bool useDatabaseNullSemantics = true)
        {
            _useDatabaseNullSemantics = useDatabaseNullSemantics;
        }
    }
}
