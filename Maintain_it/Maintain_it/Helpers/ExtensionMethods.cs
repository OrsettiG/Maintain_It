using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Helpers
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Converts the objects in the IEnumerable to a List<T> if possible. If no conversion exists then this will probably fail and crash the program.
        /// </summary>
        /// <typeparam name="T">Type to convert objects to</typeparam>
        /// <param name="list">The resulting list</param>
        /// <returns>True if conversion is successful, false otherwise.</returns>
        internal static bool ToListWithCast<T>( this IEnumerable<object> objs, out List<T> list ) where T : class
        {
            list = new List<T>();
            List<object> original = new List<object>(objs);
            ConcurrentBag<T> bag = new ConcurrentBag<T>();

            ParallelLoopResult result = Parallel.ForEach( objs, item =>
            {
                //TODO: Error/type conversion checking and crash handling.
                bag.Add( item as T );
            } );

            list.AddRange( bag );

            return list.Count == original.Count;
        }

        internal static void Align<T>( this HashSet<T> set, IEnumerable<T> targets, out List<T> Added, out List<T> Removed)
        {
            Added = new List<T>();
            Removed = new List<T>();

            foreach( T target in targets )
            {
                if( !set.Remove( target ) )
                {
                    _ = set.Add( target );
                    Added.Add( target );
                }
                else
                {
                    Removed.Add( target );
                }
            }
        }
    }
}
