using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Maintain_it.Models;

using SQLite;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public class MaintenanceItemService : Service<MaintenanceItem>
    {
        internal static MaintenanceItem defaultMaintenanceItem = new MaintenanceItem()
        {
            Name = "Default MaintenanceItem",
            NextServiceDate = DateTime.Now.AddDays( 10 ),
            FirstServiceDate = DateTime.Now.AddDays( -10 ),
            PreviousServiceDate = DateTime.Now,
            PreviousServiceCompleted = true,
            IsRecurring = true,
            RecursEvery = 10,
            Frequency = Timeframe.DAYS,
            TimesServiced = 2,
            NotifyOfNextServiceDate = true,
            Comment = "Default Maintenance Item Comment",

            Materials = new List<Material>()
            {
                MaterialService.defaultMaterial
            },
            Steps = new List<Step>()
            {
                StepService.defaultStep
            }
        };

        public MaintenanceItem GetDefaultItem()
        {
            return defaultMaintenanceItem;
        }

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<MaintenanceItem>() == null )
            {
                _ = await db.CreateTableAsync<MaintenanceItem>();
            }

            if( await db.Table<MaintenanceItem>().CountAsync() < 1 )
            {
                _ = db.InsertAsync( defaultMaintenanceItem );
            }
        }

        //public async Task AddItemAsync( MaintenanceItem item )
        //{
        //    await Init();
        //    _ = await db.InsertAsync( item );
        //}

        //public async Task DeleteItemAsync( int id )
        //{
        //    await Init();
        //    _ = await db.Table<MaintenanceItem>().DeleteAsync( x => x.Id == id );
        //}

        //public async Task<MaintenanceItem> GetItemAsync( int id )
        //{
        //    await Init();
        //    return await db.Table<MaintenanceItem>().Where( x => x.Id == id ).FirstAsync();

        //}

        //public async Task<IEnumerable<MaintenanceItem>> GetAllItemsAsync( bool forceRefresh = false )
        //{
        //    await Init();

        //    List<MaintenanceItem> data = await db.Table<MaintenanceItem>().ToListAsync();

        //    return data;
        //}

        //public async Task UpdateItemAsync( MaintenanceItem item )
        //{
        //    await Init();
        //    _ = await db.InsertOrReplaceAsync( item );
        //}
    }

}
