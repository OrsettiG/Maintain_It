using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public static class TwoWayEnumeratorHelper
    {
        public static ITwoWayEnumerator<T> GetTwoWayEnumerator<T>( this IEnumerable<T> source )
        {
            return source == null ? throw new ArgumentNullException("source") : (ITwoWayEnumerator<T>)new TwoWayEnumerator<T>( source.GetEnumerator() );
        }
    }
}
