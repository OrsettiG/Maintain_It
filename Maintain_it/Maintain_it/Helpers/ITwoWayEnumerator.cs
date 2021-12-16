using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public interface ITwoWayEnumerator<T> : IEnumerator<T>
    {
        bool MovePrevious();
        bool CanMovePrevious();
        //bool CanMoveNext();
    }
}
