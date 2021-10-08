using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public class MaterialService : Service<Material>, IDataStore<Material>
    {
        public override async Task Init()
        {
            await base.Init();
            
            if( db.Table<Material>() == null )
            {
                _ = await db.CreateTableAsync<Material>();

                if(await db.Table<Material>().CountAsync() < 1 )
                {
                    _ = db.InsertAsync( new Material()
                    {
                        Name = "Default Material"
                    } );
                }
            }
        }

        public async Task AddItemAsync( Material item )
        {
            await Init();

            _ = db.InsertAsync( item );
        }

        public async Task DeleteItemAsync( int id )
        {
            await Init();

            _ = db.Table<Material>().DeleteAsync( x => x.Id == id );
        }

        public async Task<IEnumerable<Material>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();

            List<Material> items = await db.Table<Material>().ToListAsync();

            return items;
        }

        public async Task<Material> GetItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public async Task UpdateItemAsync( Material item )
        {
            throw new NotImplementedException();
        }
    }
}
