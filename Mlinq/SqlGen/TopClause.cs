using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class TopClause : ISqlFragment
    {
        private readonly ISqlFragment topCount;
        private readonly bool withTies;

        internal bool WithTies
        {
            get { return withTies; }
        }

        internal ISqlFragment TopCount
        {
            get { return topCount; }
        }

        internal TopClause(ISqlFragment topCount, bool withTies)
        {
            this.topCount = topCount;
            this.withTies = withTies;
        }

        internal TopClause(int topCount, bool withTies)
        {
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
            this.topCount = sqlBuilder;
            this.withTies = withTies;
        }

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("TOP ");

            writer.Write("(");

            TopCount.WriteSql(writer, sqlGenerator);

            writer.Write(")");

            writer.Write(" ");

            if (WithTies)
            {
                writer.Write("WITH TIES ");
            }
        }
    }
}
