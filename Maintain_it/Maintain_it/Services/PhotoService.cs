using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class PhotoService : Service<Photo>
    {
        public override async Task Init()
        {
            if( await db.Table<Photo>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( new Photo()
                {
                    Comment = "Default Photo"
                } );
            }
        }

        public Task AddItemAsync( Photo item )
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Photo>> GetAllItemsAsync( bool forceRefresh = false )
        {
            throw new NotImplementedException();
        }

        public Task<Photo> GetItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemAsync( Photo item )
        {
            throw new NotImplementedException();
        }
    }
}
