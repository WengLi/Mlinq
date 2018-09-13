using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class SkipClause : ISqlFragment
    {
        private readonly ISqlFragment skipCount;

        internal ISqlFragment SkipCount
        {
            get { return skipCount; }
        }

        internal SkipClause(ISqlFragment skipCount)
        {
            this.skipCount = skipCount;
        }

        internal SkipClause(int skipCount)
        {
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
            this.skipCount = sqlBuilder;
        }

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("OFFSET ");

            SkipCount.WriteSql(writer, sqlGenerator);

            writer.Write(" ROWS ");
        }
    }
}
