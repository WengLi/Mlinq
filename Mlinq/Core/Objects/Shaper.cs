using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects
{
    internal abstract class Shaper
    {
        public readonly DbDataReader Reader;

        internal Shaper(DbDataReader reader)
        {
            Reader = reader;
        }
    }
    internal class Shaper<T> : Shaper
    {
        private Func<Shaper, T> _element;

        private T _current;

        internal T Current
        {
            get { return _current; }
        }

        internal Shaper(DbDataReader reader, Func<Shaper, T> element)
            : base(reader)
        {
            _element = element;
        }

        internal bool StoreRead()
        {
            return Reader.Read();
        }

        internal void SetCurrentToDefault()
        {
            _current = default(T);
        }

        internal void Finally()
        {
            Reader.Dispose();
        }

        internal void ReadNextElement()
        {
            _current = _element(this);
        }
    }
}
