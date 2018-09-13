using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal sealed class SqlSelectStatement : ISqlFragment
    {
        internal bool OutputColumnsRenamed { get; set; }

        internal Dictionary<string, Symbol> OutputColumns { get; set; }

        internal List<Symbol> AllJoinExtents { get; set; }

        private List<Symbol> fromExtents;

        internal List<Symbol> FromExtents
        {
            get
            {
                if (null == fromExtents)
                {
                    fromExtents = new List<Symbol>();
                }
                return fromExtents;
            }
        }

        private Dictionary<Symbol, bool> outerExtents;

        internal Dictionary<Symbol, bool> OuterExtents
        {
            get
            {
                if (null == outerExtents)
                {
                    outerExtents = new Dictionary<Symbol, bool>();
                }
                return outerExtents;
            }
        }

        private readonly SqlSelectClauseBuilder select;

        internal SqlSelectClauseBuilder Select
        {
            get { return select; }
        }

        private readonly SqlBuilder from = new SqlBuilder();

        internal SqlBuilder From
        {
            get { return from; }
        }

        private SqlBuilder where;

        internal SqlBuilder Where
        {
            get
            {
                if (null == where)
                {
                    where = new SqlBuilder();
                }
                return where;
            }
        }

        private SqlBuilder groupBy;

        internal SqlBuilder GroupBy
        {
            get
            {
                if (null == groupBy)
                {
                    groupBy = new SqlBuilder();
                }
                return groupBy;
            }
        }

        private SqlBuilder orderBy;

        public SqlBuilder OrderBy
        {
            get
            {
                if (null == orderBy)
                {
                    orderBy = new SqlBuilder();
                }
                return orderBy;
            }
        }

        internal bool IsTopMost { get; set; }

        internal SqlSelectStatement()
        {
            @select = new SqlSelectClauseBuilder(delegate { return IsTopMost; });
        }

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            List<string> outerExtentAliases = null;
            if ((null != outerExtents) && (0 < outerExtents.Count))
            {
                foreach (var outerExtent in outerExtents.Keys)
                {
                    var joinSymbol = outerExtent as JoinSymbol;
                    if (joinSymbol != null)
                    {
                        foreach (var symbol in joinSymbol.FlattenedExtentList)
                        {
                            if (null == outerExtentAliases)
                            {
                                outerExtentAliases = new List<string>();
                            }
                            outerExtentAliases.Add(symbol.NewName);
                        }
                    }
                    else
                    {
                        if (null == outerExtentAliases)
                        {
                            outerExtentAliases = new List<string>();
                        }
                        outerExtentAliases.Add(outerExtent.NewName);
                    }
                }
            }

            var extentList = AllJoinExtents ?? fromExtents;
            if (null != extentList)
            {
                foreach (var fromAlias in extentList)
                {
                    if ((null != outerExtentAliases) && outerExtentAliases.Contains(fromAlias.Name))
                    {
                        var i = sqlGenerator.AllExtentNames[fromAlias.Name];
                        string newName;
                        do
                        {
                            ++i;
                            newName = fromAlias.Name + i.ToString(CultureInfo.InvariantCulture);
                        }
                        while (sqlGenerator.AllExtentNames.ContainsKey(newName));
                        sqlGenerator.AllExtentNames[fromAlias.Name] = i;
                        fromAlias.NewName = newName;

                        sqlGenerator.AllExtentNames[newName] = 0;
                    }

                    if (null == outerExtentAliases)
                    {
                        outerExtentAliases = new List<string>();
                    }
                    outerExtentAliases.Add(fromAlias.NewName);
                }
            }

            writer.Indent += 1;

            @select.WriteSql(writer, sqlGenerator);

            writer.WriteLine();
            writer.Write("FROM ");
            From.WriteSql(writer, sqlGenerator);

            if ((null != @where) && !Where.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("WHERE ");
                Where.WriteSql(writer, sqlGenerator);
            }

            if ((null != groupBy) && !GroupBy.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("GROUP BY ");
                GroupBy.WriteSql(writer, sqlGenerator);
            }

            if ((null != orderBy) && !OrderBy.IsEmpty && (IsTopMost || Select.Top != null || Select.Skip != null))
            {
                writer.WriteLine();
                writer.Write("ORDER BY ");
                OrderBy.WriteSql(writer, sqlGenerator);
            }

            if (null != Select.Skip)
            {
                writer.WriteLine();
                WriteOffsetFetch(writer, Select.Top, Select.Skip, sqlGenerator);
            }

            --writer.Indent;
        }

        private static void WriteOffsetFetch(SqlWriter writer, TopClause top, SkipClause skip, SqlGenerator sqlGenerator)
        {
            skip.WriteSql(writer, sqlGenerator);
            if (top != null)
            {
                writer.Write("FETCH NEXT ");

                top.TopCount.WriteSql(writer, sqlGenerator);

                writer.Write(" ROWS ONLY ");
            }
        }

    }
}
