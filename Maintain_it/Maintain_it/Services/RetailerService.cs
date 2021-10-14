using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class RetailerService : Service<Retailer>
    {
        internal static Retailer defaultRetailer = new Retailer()

        {
            Name = "Default Retailer",
            Materials = new List<Material>()
            {
                MaterialService.defaultMaterial
            }
        };

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<Retailer>() == null )
            {
                _ = await db.CreateTableAsync<Retailer>();
            }

            if( await db.Table<Retailer>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( defaultRetailer );
            }
        }
    }
}
