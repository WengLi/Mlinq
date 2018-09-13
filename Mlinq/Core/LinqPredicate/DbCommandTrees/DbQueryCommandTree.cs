using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate.DbCommandTrees
{
    public sealed class DbQueryCommandTree : DbCommandTree
    {
        private readonly Predicate _query;

        public DbQueryCommandTree(Predicate query, bool useDatabaseNullSemantics)
            : base(useDatabaseNullSemantics)
        {
            Check.NotNull(query, "query");
            _query = query;
        }

        public Predicate Query
        {
            get { return _query; }
        }

        public override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Query; }
        }

        internal static DbQueryCommandTree FromValidExpression(Predicate query, bool useDatabaseNullSemantics)
        {
            return new DbQueryCommandTree(query, useDatabaseNullSemantics);
        }
    }
}
