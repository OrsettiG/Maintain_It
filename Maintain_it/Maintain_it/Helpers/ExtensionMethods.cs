using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

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

        internal static void Align<T>( this HashSet<T> set, IEnumerable<T> targets, out List<T> Added, out List<T> Removed )
        {
            Console.WriteLine( "ALIGNING" );
            Added = new List<T>();
            Removed = new List<T>();

            foreach( T t in targets )
            {
                if( set.Contains( t ) )
                {
                    continue;
                }
                else
                {
                    _ = set.Add( t );
                    Added.Add( t );
                    Console.WriteLine( $"Added {t}" );
                }

                // TODO: Should be able to make this loop into the following:
                //if( set.Add( t ) )
                //{
                //    Added.Add( t );
                //    Console.WriteLine( $"Added {t}" );
                //}
            }

            List<T> temp = new List<T>(set);

            foreach( T t in temp )
            {
                if( targets.Contains( t ) )
                {
                    continue;
                }
                else
                {
                    _ = set.Remove( t );
                    Removed.Add( t );
                    Console.WriteLine( $"Removed {t}" );
                }
            }

            Console.WriteLine( "ALIGNMENT COMPLETE" );
        }

        internal static IEnumerable<int> GetIds( this IEnumerable<IStorableObject> storableObjects )
        {
            List<int> ids = new List<int>();
            foreach( IStorableObject storableObject in storableObjects )
            {
                ids.Add( storableObject.Id );
            }
            return ids;
        }

        internal static void Align<T>( this HashSet<T> set, IEnumerable<T> targets )
        {
            foreach( T t in targets )
            {
                if( set.Contains( t ) )
                {
                    continue;
                }
                else
                {
                    _ = set.Add( t );
                }
            }

            List<T> temp = new List<T>(set);

            foreach( T t in temp )
            {
                if( targets.Contains( t ) )
                {
                    continue;
                }
                else
                {
                    _ = set.Remove( t );
                }
            }
        }

        internal static bool Contains<T>( this IEnumerable<T> source, T value )
        {
            foreach( T t in source )
            {
                if( Equals( t, value ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
