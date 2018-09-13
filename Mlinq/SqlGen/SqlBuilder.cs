using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class SqlBuilder : ISqlFragment
    {
        private List<object> _sqlFragments;

        private List<object> sqlFragments
        {
            get
            {
                if (null == _sqlFragments)
                {
                    _sqlFragments = new List<object>();
                }
                return _sqlFragments;
            }
        }

        public void Append(object s)
        {
            sqlFragments.Add(s);
        }

        public void AppendLine()
        {
            sqlFragments.Add("\r\n");
        }

        public virtual bool IsEmpty
        {
            get { return ((null == _sqlFragments) || (0 == _sqlFragments.Count)); }
        }

        public virtual void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (null != _sqlFragments)
            {
                foreach (var o in _sqlFragments)
                {
                    var str = (o as String);
                    if (null != str)
                    {
                        writer.Write(str);
                    }
                    else
                    {
                        var sqlFragment = (o as ISqlFragment);
                        if (null != sqlFragment)
                        {
                            sqlFragment.WriteSql(writer, sqlGenerator);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

    }
}
