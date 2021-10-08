using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLiteNetExtensionsAsync.Extensions;

namespace Maintain_it.Services
{
    public class StepService : Service<Step>, IDataStore<Step>
    {
        public override async Task Init()
        {
            await base.Init();

            _ = await db.CreateTableAsync<Step>();

            if( await db.Table<Step>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( new Step()
                {
                    Name = "Default Step"
                } );
            }
        }

        public async Task AddItemAsync( Step item )
        {
            await Init();
            _ = await db.InsertAsync( item );
        }

        public async Task DeleteItemAsync( int id )
        {
            await Init();
            _ = await db.Table<Step>().DeleteAsync( x => x.Id == id );
        }

        public async Task<IEnumerable<Step>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();

            return await db.Table<Step>().ToListAsync();
        }

        public async Task<Step> GetItemAsync( int id )
        {
            await Init();

            return await db.Table<Step>().Where( x => x.Id == id ).FirstOrDefaultAsync();
        }

        public async Task UpdateItemAsync( Step item )
        {
            await Init();
            
            _ = await db.UpdateAsync( item );
        }
    }
}
