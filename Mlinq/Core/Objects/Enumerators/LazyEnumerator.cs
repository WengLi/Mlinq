using System;
using System.Collections;
using System.Collections.Generic;

namespace Mlinq.Core.Objects.Enumerators
{
    internal class LazyEnumerator<T> : IEnumerator<T>
    {
        private readonly Func<ObjectResult<T>> _getObjectResult;
        private IEnumerator<T> _objectResultEnumerator;

        public LazyEnumerator(Func<ObjectResult<T>> getObjectResult)
        {
            _getObjectResult = getObjectResult;
        }

        public T Current
        {
            get
            {
                return _objectResultEnumerator == null ? default(T) : _objectResultEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            if (_objectResultEnumerator != null)
            {
                _objectResultEnumerator.Dispose();
            }
        }

        public bool MoveNext()
        {
            if (_objectResultEnumerator == null)
            {
                var objectResult = _getObjectResult();
                try
                {
                    _objectResultEnumerator = objectResult.GetEnumerator();
                }
                catch
                {
                    objectResult.Dispose();
                    throw;
                }
            }
            return _objectResultEnumerator.MoveNext();
        }

        public void Reset()
        {
            if (_objectResultEnumerator != null)
            {
                _objectResultEnumerator.Reset();
            }
        }
    }
}
