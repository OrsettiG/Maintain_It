using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    public static class Cache
    {
        internal enum CacheType { Shallow, Deep }
        private static class CacheInstance<T> where T : IStorableObject
        {
            private static Dictionary<int, (TimeSpan,T)> shallowCache;
            public static Dictionary<int, (TimeSpan, T)> ShallowCache
            {
                get => shallowCache ??= new Dictionary<int, (TimeSpan, T)>();
            }

            private static Dictionary<int, (TimeSpan,T)> deepCache;
            public static Dictionary<int, (TimeSpan, T)> DeepCache
            {
                get => deepCache ??= new Dictionary<int, (TimeSpan, T)>();
            }
        }

        internal static async Task TrimCache<T>( CacheType cType ) where T : IStorableObject, new()
        {
            if( cType == CacheType.Shallow )
            {
                foreach( int key in CacheInstance<T>.ShallowCache.Keys.ToList() )
                {
                    if( DateTime.Now.Ticks - CacheInstance<T>.ShallowCache[key].Item1.Ticks >= TimeSpan.TicksPerMinute * 20 )
                    {
                        await DbServiceLocator.UpdateItemAsync<T>( CacheInstance<T>.ShallowCache[key].Item2 );
                        _ = CacheInstance<T>.ShallowCache.Remove( key );
                    }
                }
            }

            if( cType == CacheType.Deep )
            {
                foreach( int key in CacheInstance<T>.DeepCache.Keys.ToList() )
                {
                    if( DateTime.Now.Ticks - CacheInstance<T>.DeepCache[key].Item1.Ticks >= TimeSpan.TicksPerMinute * 20 )
                    {
                        await DbServiceLocator.UpdateItemAsync<T>( CacheInstance<T>.DeepCache[key].Item2 );
                        _ = CacheInstance<T>.DeepCache.Remove( key );
                    }
                }
            }
        }

        internal static async Task AddShallow<T>( T obj ) where T : IStorableObject, new()
        {
            await TrimCache<T>( CacheType.Shallow );

            CacheInstance<T>.ShallowCache.Add( obj.Id, (new TimeSpan( DateTime.Now.Ticks ), obj) );
        }

        /// <summary>
        /// Adds the passed in object to the DeepCache and stamps it with the time it was added.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        internal static async Task AddDeep<T>( T obj ) where T : IStorableObject, new()
        {
            await TrimCache<T>( CacheType.Deep );

            CacheInstance<T>.DeepCache.Add( obj.Id, (new TimeSpan( DateTime.Now.Ticks ), obj) );
        }

        /// <summary>
        /// Checks both the Deep and Shallow caches for objects with matching Ids and either returns the true while outing the object, or returns false while outing a default object.
        /// </summary>
        internal static bool GetItem<T>( int id, out T item ) where T : IStorableObject, new()
        {
            return GetShallowItem( id, out item ) || GetDeepItem( id, out item );
        }

        /// <summary>
        /// Checks only the DeepCache for matching items and returns the true while outing the item if found, otherwise returns false and outs a default T
        /// </summary>
        internal static bool GetDeepItem<T>( int id, out T item ) where T : IStorableObject, new()
        {
            item = default;

            if( CacheInstance<T>.DeepCache.ContainsKey( id ) )
            {
                item = CacheInstance<T>.DeepCache[id].Item2;
                CacheInstance<T>.DeepCache[id] = ( new TimeSpan( DateTime.Now.Ticks ), item );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the ShallowCache for objects with matching Ids and either returns the true while outing the object, or returns false while outing a default object.
        /// </summary>
        internal static bool GetShallowItem<T>( int id, out T item ) where T : IStorableObject, new()
        {
            item = default;

            if( CacheInstance<T>.ShallowCache.ContainsKey( id ) )
            {
                item = CacheInstance<T>.ShallowCache[id].Item2;
                CacheInstance<T>.ShallowCache[id] = (new TimeSpan( DateTime.Now.Ticks ), item);
                return true;
            }

            return false;
        }

        internal static void UpdateItem<T>( int id, T item ) where T : IStorableObject, new()
        {
            if( CacheInstance<T>.ShallowCache.ContainsKey(id) )
            {
                CacheInstance<T>.ShallowCache[id] = ( new TimeSpan( DateTime.Now.Ticks), item );
            }

            if( CacheInstance<T>.DeepCache.ContainsKey( id ) )
            {
                CacheInstance<T>.DeepCache[id] = (new TimeSpan( DateTime.Now.Ticks ), item);
            }
        }

        /// <summary>
        /// Removes the item from the cache and returns true if successful, false otherwise.
        /// </summary>
        internal static bool RemoveItem<T>( int id ) where T : IStorableObject, new()
        {
            return CacheInstance<T>.ShallowCache.Remove( id ) || CacheInstance<T>.DeepCache.Remove( id );
        }
    }
}
