using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

using SQLiteNetExtensionsAsync.Extensions;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public class Service<T> : IDataStore<T> where T : IStorableObject, new()
    {
        private protected static SQLiteAsyncConnection db;

        private readonly string _SQLiteCommandString_last_insert_rowid = "select last_insert_rowid()";
        private readonly string _SQLiteCommandString_last_insert_rowid_per_table = $"select seq from sqlite_sequence where name = \"{typeof(T).Name}\"";

        public virtual async Task Init()
        {
            //Create connection to db
            db = await AsyncDatabaseConnection.Db();
        }

        public virtual bool IsInitialized()
        {
            return db.Table<T>() != null;
        }

        public virtual async Task<int> AddItemAndReturnRowIdAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item );
            List<int> lastIds = await db.QueryScalarsAsync<int>(_SQLiteCommandString_last_insert_rowid_per_table);

            return lastIds[0];
        }

        public virtual async Task<int> AddItemAndReturnRowIdRecursiveAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item, recursive: true );
            List<int> lastIds = await db.QueryScalarsAsync<int>(_SQLiteCommandString_last_insert_rowid_per_table);

            return lastIds[0];
        }

        public virtual async Task AddItemAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item );
        }

        public virtual async Task DeleteItemAsync( int id )
        {
            await Init();
            _ = await db.Table<T>().DeleteAsync( x => x.Id == id );
        }

        public virtual async Task<IEnumerable<T>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();
            List<T> data = await db.GetAllWithChildrenAsync<T>();
            return data;
        }

        public virtual async Task<IEnumerable<T>> GetAllItemsRecursiveAsync( bool forceRefresh = false )
        {
            await Init();
            List<T> data = await db.GetAllWithChildrenAsync<T>(recursive: true).ConfigureAwait(false);

            return data;
        }

        public virtual async Task<T> GetItemAsync( int id )
        {
            await Init();

            T item = await db.GetWithChildrenAsync<T>( id ).ConfigureAwait( false );
            return item;
        }

        public virtual async Task<T> GetItemRecursiveAsync( int id )
        {
            await Init();

            return await db.GetWithChildrenAsync<T>( id, recursive: true ).ConfigureAwait( false );
        }

        public virtual async Task<IEnumerable<T>> GetItemRangeAsync( IEnumerable<int> ids )
        {
            await Init();

            List<T> data = await db.Table<T>().Where(x => ids.Contains(x.Id) ).ToListAsync();
            return data;
        }

        public virtual async Task<IEnumerable<T>> GetItemRangeRecursiveAsync( IEnumerable<int> ids )
        {
            await Init();

            List<T> filteredData = await db.GetAllWithChildrenAsync<T>( x => ids.Contains(x.Id) , recursive: true ).ConfigureAwait( false );

            return filteredData;
        }

        public virtual async Task<IEnumerable<T>> GetItemRangeBasedOnSearchTermAsync( string searchTerm )
        {
            await Init();

            List<T> data = await db.Table<T>().Where(x => x.Name.StartsWith(searchTerm) ).ToListAsync();
            return data;
        }

        public virtual async Task<IEnumerable<T>> GetItemRangeBasedOnSearchTermRecursiveAsync( string searchTerm )
        {
            await Init();

            List<T> data = await db.GetAllWithChildrenAsync<T>(x => x.Name.StartsWith(searchTerm), recursive: true).ConfigureAwait(false);
            return data;
        }

        public virtual async Task<IEnumerable<T>> GetItemsInDateRangeAsync( DateTime newestDateCreated, DateTime oldestDateCreated, bool returnAll = true, int returnCount = 0 )
        {
            await Init();

            List<T> data = await db.Table<T>().Where( x => x.CreatedOn <= newestDateCreated && x.CreatedOn >= oldestDateCreated ).ToListAsync();

            return !returnAll && returnCount > 0 ? data.Take( returnCount ) : data;
        }

        public virtual async Task<IEnumerable<T>> GetItemsInDateRangeRecursiveAsync( DateTime newestDateCreated, DateTime oldestDateCreated, bool returnAll = true, int returnCount = 0 )
        {
            await Init();
            List<T> data = await db.GetAllWithChildrenAsync<T>( x => x.CreatedOn <= newestDateCreated && x.CreatedOn >= oldestDateCreated ).ConfigureAwait(false);

            return !returnAll && returnCount > 0 ? data.Take( returnCount ) : data;
        }

        public virtual async Task UpdateItemAsync( T item )
        {
            await Init();
            await db.UpdateWithChildrenAsync( item );
        }

        public virtual async Task AddOrUpdate( T item )
        {
            await Init();
            if( item.Id != 0 )
            {
                await db.UpdateWithChildrenAsync( item );
            }
            else
            {
                await db.InsertWithChildrenAsync( item );
            }
        }
        public virtual async Task<int> AddOrUpdateAndReturnId( T item )
        {
            await Init();
            if( item.Id != 0 )
            {
                await db.UpdateWithChildrenAsync( item );
            }
            else
            {
                await db.InsertWithChildrenAsync( item );
            }

            List<int> lastIds = await db.QueryScalarsAsync<int>(_SQLiteCommandString_last_insert_rowid_per_table);
            return lastIds[0];
        }

        public virtual async Task<int> DeleteAllAsync<T>()
        {
            await Init();
            return await db.DeleteAllAsync<T>();
        }
    }
}
