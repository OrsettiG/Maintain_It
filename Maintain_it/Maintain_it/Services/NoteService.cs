using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class NoteService : Service<Note>
    {
        internal static Note defaultNote = new Note()
        {
            Title = "Default Note",
            Description = "A default note to ensure the database is working correctly",
            Created = DateTime.Now,
            LastUpdated = DateTime.Now
        };

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<Note>() == null )
            {
                _ = await db.CreateTableAsync<Note>();
            }

            if( await db.Table<Note>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( defaultNote );
            }
        }
    }
}
