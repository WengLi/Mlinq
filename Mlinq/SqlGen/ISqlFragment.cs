using System.Text;

namespace Mlinq.SqlGen
{
    internal interface ISqlFragment
    {
        void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
    }
}
