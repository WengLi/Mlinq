using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class SqlSelectClauseBuilder : SqlBuilder
    {
        private List<OptionalColumn> m_optionalColumns;

        internal void AddOptionalColumn(OptionalColumn column)
        {
            if (m_optionalColumns == null)
            {
                m_optionalColumns = new List<OptionalColumn>();
            }
            m_optionalColumns.Add(column);
        }

        private TopClause m_top;

        internal TopClause Top
        {
            get { return m_top; }
            set
            {
                m_top = value;
            }
        }

        private SkipClause m_skip;

        internal SkipClause Skip
        {
            get { return m_skip; }
            set
            {
                m_skip = value;
            }
        }

        internal bool IsDistinct { get; set; }

        public override bool IsEmpty
        {
            get { return (base.IsEmpty) && (m_optionalColumns == null || m_optionalColumns.Count == 0); }
        }

        private readonly Func<bool> m_isPartOfTopMostStatement;

        internal SqlSelectClauseBuilder(Func<bool> isPartOfTopMostStatement)
        {
            m_isPartOfTopMostStatement = isPartOfTopMostStatement;
        }

        public override void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("SELECT ");
            if (IsDistinct)
            {
                writer.Write("DISTINCT ");
            }

            if (Top != null && Skip == null)
            {
                Top.WriteSql(writer, sqlGenerator);
            }

            if (IsEmpty)
            {
                writer.Write("*");
            }
            else
            {
                var printedAny = WriteOptionalColumns(writer, sqlGenerator);

                if (!base.IsEmpty)
                {
                    if (printedAny)
                    {
                        writer.Write(", ");
                    }
                    base.WriteSql(writer, sqlGenerator);
                }
                else if (!printedAny)
                {
                    m_optionalColumns[0].MarkAsUsed();
                    m_optionalColumns[0].WriteSqlIfUsed(writer, sqlGenerator, "");
                }
            }
        }

        private bool WriteOptionalColumns(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (m_optionalColumns == null)
            {
                return false;
            }

            if (m_isPartOfTopMostStatement() || IsDistinct)
            {
                foreach (var column in m_optionalColumns)
                {
                    column.MarkAsUsed();
                }
            }

            var separator = "";
            var printedAny = false;
            foreach (var column in m_optionalColumns)
            {
                if (column.WriteSqlIfUsed(writer, sqlGenerator, separator))
                {
                    printedAny = true;
                    separator = ", ";
                }
            }
            return printedAny;
        }

    }
}
