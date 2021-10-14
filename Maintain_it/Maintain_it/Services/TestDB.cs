using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public class TestDB //: IDataStore<TestItemModel>
    {
        private SQLiteAsyncConnection db;

        public async Task Init()
        {
            if( db != null )
            {
                return;
            }

            db = new SQLiteAsyncConnection( Path.Combine( FileSystem.AppDataDirectory, "MaintainIt.db" ) );

            _ = db.CreateTableAsync<TestItemModel>();

            if( await db.Table<TestItemModel>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( new TestItemModel() { Name = "Default Item" } );
            }
        }

        public async Task AddItemAsync( TestItemModel item )
        {
            await Init();
            _ = db.InsertAsync( item );
        }

        public async Task DeleteItemAsync( int id )
        {
            await Init();

            _ = db.Table<TestItemModel>().DeleteAsync( x => x.id == id );
        }

        public async Task<TestItemModel> GetItemAsync( int id )
        {
            await Init();
            TestItemModel item = await db.Table<TestItemModel>().Where( x => x.id == id ).FirstAsync();
            return item;
        }

        public async Task<IEnumerable<TestItemModel>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();
            return await db.Table<TestItemModel>().ToListAsync();
        }

        public async Task UpdateItemAsync( TestItemModel item )
        {
            await Init();

            _ = await db.UpdateAsync( item );
        }
    }
}
