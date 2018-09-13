using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal sealed class JoinSymbol : Symbol
    {
        private List<Symbol> columnList;

        internal List<Symbol> ColumnList
        {
            get
            {
                if (null == columnList)
                {
                    columnList = new List<Symbol>();
                }
                return columnList;
            }
            set { columnList = value; }
        }

        private readonly List<Symbol> extentList;

        internal List<Symbol> ExtentList
        {
            get { return extentList; }
        }

        private List<Symbol> flattenedExtentList;

        internal List<Symbol> FlattenedExtentList
        {
            get
            {
                if (null == flattenedExtentList)
                {
                    flattenedExtentList = new List<Symbol>();
                }
                return flattenedExtentList;
            }
            set { flattenedExtentList = value; }
        }

        private readonly Dictionary<string, Symbol> nameToExtent;

        internal Dictionary<string, Symbol> NameToExtent
        {
            get { return nameToExtent; }
        }

        internal bool IsNestedJoin { get; set; }

        public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
            : base(name, type)
        {
            extentList = new List<Symbol>(extents.Count);
            nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var symbol in extents)
            {
                nameToExtent[symbol.Name] = symbol;
                ExtentList.Add(symbol);
            }
        }
    }
}
