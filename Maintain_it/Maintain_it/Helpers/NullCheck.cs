using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    internal static class NullCheck
    {
        public static bool NotNull<T>( T nullable )
        {
            return nullable != null;
        }
    }
}
