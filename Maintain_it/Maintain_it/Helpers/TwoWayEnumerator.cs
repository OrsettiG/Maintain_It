using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public class TwoWayEnumerator<T> : ITwoWayEnumerator<T>
    {
        public TwoWayEnumerator( IEnumerator<T> enumerator )
        {
            _enumerator = enumerator ?? throw new ArgumentNullException( "enumerator" );
            _buffer = new List<T>();
            _index = -1;
        }
        
        
        private readonly IEnumerator<T> _enumerator;
        private readonly List<T> _buffer;
        private int _index;

        object IEnumerator.Current => Current;
        public T Current
        {
            get
            {
                return _index < 0 || _index >= _buffer.Count ? throw new InvalidOperationException() : _buffer[_index];
            }
        }

        public bool CanMovePrevious()
        {
            return _index !<= 0;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if(_index < _buffer.Count - 1 )
            {
                ++_index;
                return true;
            }

            if( _enumerator.MoveNext() )
            {
                _buffer.Add( _enumerator.Current );
                ++_index;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if( _index <= 0 )
            {
                return false;
            }

            --_index;
            return true;
        }

        public void Reset()
        {
            _enumerator.Reset();
            _buffer.Clear();
            _index = -1;
        }
    }
}
