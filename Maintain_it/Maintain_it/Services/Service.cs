using System;
using System.Collections.Generic;
using System.IO;
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

        public virtual async Task Init()
        {
            if( db != null )
            {
                return;
            }

            db = AsyncDatabaseConnection.Db;

            _ = await db.CreateTableAsync<T>();
            
            // Checks to make sure that the Join tables are created the first time any Service is Initted, but then only checks one every time after that. Just prevents us from unnecessarily pinging the db a bunch of times every time a service is created.
            if(db.Table<StepsToStepMaterials>() == null )
            {
                if(db.Table<MaterialsToRetailers>() == null )
                {
                    if( db.Table<ShoppingListItemToShoppingList>() == null )
                    {
                        _ = await db.CreateTableAsync<ShoppingListItemToShoppingList>();
                    }
                    _ = await db.CreateTableAsync<MaterialsToRetailers>();
                }
                _ = await db.CreateTableAsync<StepsToStepMaterials>();
            }
        }

        public virtual async Task<int> AddItemAndReturnRowIdAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item );
            int id = await db.ExecuteScalarAsync<int>( _SQLiteCommandString_last_insert_rowid );
            return id;
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

        public virtual async Task<T> GetItemAsync( int id )
        {
            await Init();

            return await db.GetWithChildrenAsync<T>( id );
        }

        public virtual async Task<IEnumerable<T>> GetItemRangeAsync( List<int> ids )
        {
            await Init();

            List<T> data = await db.Table<T>().Where(x => ids.Contains(x.Id) ).ToListAsync();
            return data;
        }

        public virtual async Task UpdateItemAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item ); //db.InsertOrReplaceAsync( item );
        }

        public virtual async Task<int> DeleteAllAsync<T>()
        {
            await Init();
            return await db.DeleteAllAsync<T>();
        }
    }
}
