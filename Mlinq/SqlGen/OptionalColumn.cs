using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal sealed class OptionalColumn
    {
        private readonly SymbolUsageManager m_usageManager;

        private readonly SqlBuilder m_builder = new SqlBuilder();

        private readonly Symbol m_symbol;

        internal void Append(object s)
        {
            m_builder.Append(s);
        }

        internal void MarkAsUsed()
        {
            m_usageManager.MarkAsUsed(m_symbol);
        }

        internal OptionalColumn(SymbolUsageManager usageManager, Symbol symbol)
        {
            m_usageManager = usageManager;
            m_symbol = symbol;
        }

        public bool WriteSqlIfUsed(SqlWriter writer, SqlGenerator sqlGenerator, string separator)
        {
            if (m_usageManager.IsUsed(m_symbol))
            {
                writer.Write(separator);
                m_builder.WriteSql(writer, sqlGenerator);
                return true;
            }
            return false;
        }
    }
}
