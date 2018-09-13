using Mlinq.Core.Objects.Enumerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Mlinq.Core.Objects
{
    public class ObjectResult<T> : IEnumerable<T>
    {
        private Shaper<T> _shaper;

        internal ObjectResult(Shaper<T> shaper)
        {
            _shaper = shaper;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var shaper = _shaper;
            _shaper = null;
            var result = new SimpleEnumerator<T>(shaper);
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Dispose()
        {
            if (_shaper != null)
            {
                _shaper = null;
            }
        }
    }
}