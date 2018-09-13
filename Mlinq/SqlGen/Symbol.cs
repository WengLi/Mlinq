using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class Symbol : ISqlFragment
    {
        private Dictionary<string, Symbol> columns;

        internal Dictionary<string, Symbol> Columns
        {
            get
            {
                if (null == columns)
                {
                    columns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
                }
                return columns;
            }
        }

        private Dictionary<string, Symbol> outputColumns;

        internal Dictionary<string, Symbol> OutputColumns
        {
            get
            {
                if (null == outputColumns)
                {
                    outputColumns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
                }
                return outputColumns;
            }
        }

        internal bool NeedsRenaming { get; set; }

        internal bool OutputColumnsRenamed { get; set; }

        private readonly string name;

        public string Name
        {
            get { return name; }
        }

        public string NewName { get; set; }

        internal TypeUsage Type { get; set; }

        public Symbol(string name, TypeUsage type)
        {
            this.name = name;
            NewName = name;
            Type = type;
        }

        public Symbol(string name, TypeUsage type, Dictionary<string, Symbol> outputColumns, bool outputColumnsRenamed)
        {
            this.name = name;
            NewName = name;
            Type = type;
            this.outputColumns = outputColumns;
            OutputColumnsRenamed = outputColumnsRenamed;
        }

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (NeedsRenaming)
            {
                int i;

                if (sqlGenerator.AllColumnNames.TryGetValue(NewName, out i))
                {
                    string newNameCandidate;
                    do
                    {
                        ++i;
                        newNameCandidate = NewName + i.ToString(CultureInfo.InvariantCulture);
                    }
                    while (sqlGenerator.AllColumnNames.ContainsKey(newNameCandidate));

                    sqlGenerator.AllColumnNames[NewName] = i;

                    NewName = newNameCandidate;
                }

                sqlGenerator.AllColumnNames[NewName] = 0;

                NeedsRenaming = false;
            }
            writer.Write(SqlGenerator.QuoteIdentifier(NewName));
        }
    }
}
