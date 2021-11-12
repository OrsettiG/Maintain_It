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
        private protected static SQLiteAsyncConnection db = AsyncDatabaseConnection.Db;

        private readonly string _SQLiteCommandString_last_insert_rowid = "select last_insert_rowid()";
        private readonly string _SQLiteCommandString_last_insert_rowid_per_table = $"select seq from sqlite_sequence where name = \"{typeof(T).Name}\"";

        public virtual async Task Init()
        {                
            //Create Table
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

        /// <summary>
        /// !!!USE WITH CAUTION!!! Completely deletes all tables from the database. There is no recovering from this. !!!USE WITH CAUTION!!!
        /// </summary>
        internal static async Task DropAllTables()
        {
            _ = await db.DropTableAsync<MaintenanceItem>();
            _ = await db.DropTableAsync<Step>();
            _ = await db.DropTableAsync<StepMaterial>();
            _ = await db.DropTableAsync<Material>();
            _ = await db.DropTableAsync<Note>();
            _ = await db.DropTableAsync<Retailer>();
            _ = await db.DropTableAsync<ShoppingList>();
            _ = await db.DropTableAsync<ShoppingListItem>();
        }

        public virtual bool IsInitialized()
        {
            return db.Table<T>() != null;
        }

        public virtual async Task<int> AddItemAndReturnRowIdAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item );
            int id = await db.ExecuteScalarAsync<int>( _SQLiteCommandString_last_insert_rowid );
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
        
        public virtual async Task<IEnumerable<T>> GetItemsInDateRangeAsync( DateTime newestDateCreated, DateTime oldestDateCreated, bool returnAll = true, int returnCount = 0 )
        {
            await Init();
            List<T> data;
            
            data = await db.Table<T>().Where(x => x.CreatedOn <= newestDateCreated && x.CreatedOn >= oldestDateCreated ).ToListAsync();


            return !returnAll && returnCount > 0 ? data.Take( returnCount ) : data;
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
