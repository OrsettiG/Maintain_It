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

        public virtual async Task Init()
        {
            if( db != null )
            {
                return;
            }

            db = AsyncDatabaseConnection.Db;

            _ = await db.DeleteAllAsync<T>();

            _ = await db.CreateTableAsync<T>();
            _ = await db.CreateTableAsync<ItemsToMaterials>();
            _ = await db.CreateTableAsync<MaterialsToRetailers>();
            _ = await db.CreateTableAsync<ShoppingListItemToShoppingList>();
            _ = await db.CreateTableAsync<StepsToStepMaterials>();
        }

        public virtual async Task AddItemAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item );
            //_ = await db.InsertAsync( item );
        }

        public virtual async Task DeleteItemAsync( int id )
        {
            await Init();
            _ = await db.Table<T>().DeleteAsync( x => x.Id == id );
        }

        public virtual async Task<IEnumerable<T>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();
            List<T> data = await db.GetAllWithChildrenAsync<T>(); //db.Table<T>().ToListAsync();
            return data;
        }

        public virtual async Task<T> GetItemAsync( int id )
        {
            await Init();

            return await db.GetWithChildrenAsync<T>( id ); //db.Table<T>().Where( x => x.Id == id ).FirstOrDefaultAsync();
        }


        public virtual async Task UpdateItemAsync( T item )
        {
            await Init();
            await db.InsertWithChildrenAsync( item ); //db.InsertOrReplaceAsync( item );
        }
    }
}
