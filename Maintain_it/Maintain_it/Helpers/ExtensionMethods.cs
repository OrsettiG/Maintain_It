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

        internal static void Align( this HashSet<int> set, IEnumerable<int> targets, out List<int> Added, out List<int> Removed )
        {
            Console.WriteLine( "ALIGNING" );
            Added = new List<int>();
            Removed = new List<int>();

            foreach( int t in targets )
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

            List<int> temp = new List<int>(set);

            foreach( int t in temp )
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

        internal static void Align( this HashSet<int> set, IEnumerable<int> targets )
        {
            foreach( int t in targets )
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

            List<int> temp = new List<int>(set);

            foreach( int t in temp )
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

        internal static bool Contains( this IEnumerable<int> source, int value )
        {
            foreach( int t in source )
            {
                if( Equals( t, value ) )
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool Contains<T>(this IEnumerable<T> source, T value ) where T : IStorableObject
        {
            foreach(T t in source )
            {
                if( value.Id.Equals( t.Id ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
