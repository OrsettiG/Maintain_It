using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class PhotoService : Service<Photo>
    {
        internal static Photo defaultPhoto = new Photo()
        {
            Comment = "Default Photo",
            Bytes = File.ReadAllBytes("Maintain_it/EmbeddedImages/HappyCup.jpg")
        };

        public override async Task Init()
        {
            await base.Init();
            if( db.Table<Photo>() == null )
            {
                _ = await db.CreateTableAsync<Photo>();
            }

            if( await db.Table<Photo>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( new Photo()
                {
                    Comment = "Default Photo"
                } );
            }
        }
    }
}
