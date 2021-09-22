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
    public class MaterialService : IDataStore<Material>
    {
        private SQLiteAsyncConnection db;

        private async Task Init()
        {
            if( db != null )
            {
                return;
            }

            string path = Path.Combine( FileSystem.AppDataDirectory, "Materials.db" );

            db = new SQLiteAsyncConnection( path );
            _ = await db.CreateTableAsync<Material>();

            if(await db.Table<Material>().CountAsync() < 1 )
            {
                db.InsertAsync( new Material())
            }
        }

        public Task AddItemAsync( Material item )
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Material>> GetAllItemsAsync( bool forceRefresh = false )
        {
            throw new NotImplementedException();
        }

        public Task<Material> GetItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemAsync( Material item )
        {
            throw new NotImplementedException();
        }
    }
}
