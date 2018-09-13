using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.Enumerators
{
    internal class SimpleEnumerator<T> : IEnumerator<T>
    {
        private readonly Shaper<T> _shaper;

        internal SimpleEnumerator(Shaper<T> shaper)
        {
            _shaper = shaper;
        }

        public T Current
        {
            get { return _shaper.Current; }
        }

        object IEnumerator.Current
        {
            get { return _shaper.Current; }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _shaper.SetCurrentToDefault();
            _shaper.Finally();
        }

        public bool MoveNext()
        {
            if (_shaper.StoreRead())
            {
                _shaper.ReadNextElement();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
