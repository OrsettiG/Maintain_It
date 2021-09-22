﻿using System;
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
    public class MaintenanceItemService : IDataStore<MaintenanceItem>
    {
        private SQLiteAsyncConnection db;

        //private readonly List<MaintenanceItem> items = new List<MaintenanceItem>()
        //{
        //    new MaintenanceItem( "Item 1", DateTime.Now )
        //    {
        //        NextServiceDate = DateTime.Now.AddDays( 1 ),
        //        MaterialsAndEquipment = new List<Material>()
        //        {
        //            new Material( "Mat1", "Store1", 10.00d, 1 ),
        //            new Material( "Mat2", "Store2", 11.00d, 2 ),
        //            new Material( "Mat3", "Store3", 12.00d, 3 ),
        //            new Material( "Mat4", "Store4", 13.00d, 4 ),
        //            new Material( "Mat5", "Store5", 13.00d, 5 ),
        //            new Material( "Mat6", "Store6", 13.00d, 6 ),
        //            new Material( "Mat7", "Store7", 13.00d, 7 ),
        //            new Material( "Mat8", "Store8", 13.00d, 8 ),
        //            new Material( "Mat9", "Store9", 13.00d, 9 ),
        //            new Material( "Mat10", "Store10", 13.00d, 10 )
        //        }
        //    },
        //    new MaintenanceItem( "Item 2", DateTime.Now.AddDays(1) )
        //    {
        //        NextServiceDate = DateTime.Now.AddDays( 2 ),
        //        MaterialsAndEquipment = new List<Material>()
        //        {
        //            new Material( "Mat1", "Store1", 10.00d, 1 ),
        //            new Material( "Mat2", "Store2", 11.00d, 2 ),
        //            new Material( "Mat3", "Store3", 12.00d, 3 ),
        //            new Material( "Mat4", "Store4", 13.00d, 4 )
        //        }
        //    },
        //    new MaintenanceItem( "Item 3", DateTime.Now.AddDays(2) )
        //    {
        //        NextServiceDate = DateTime.Now.AddDays( 3 ),
        //        MaterialsAndEquipment = new List<Material>()
        //        {
        //            new Material( "Mat1", "Store1", 10.00d, 1 ),
        //            new Material( "Mat2", "Store2", 11.00d, 2 ),
        //            new Material( "Mat3", "Store3", 12.00d, 3 ),
        //            new Material( "Mat4", "Store4", 13.00d, 4 )
        //        }
        //    },
        //    new MaintenanceItem( "Item 4", DateTime.Now.AddDays(3) )
        //    {
        //        NextServiceDate = DateTime.Now.AddDays( 4 ),
        //        MaterialsAndEquipment = new List<Material>()
        //        {
        //            new Material( "Mat1", "Store1", 10.00d, 1 ),
        //            new Material( "Mat2", "Store2", 11.00d, 2 ),
        //            new Material( "Mat3", "Store3", 12.00d, 3 ),
        //            new Material( "Mat4", "Store4", 13.00d, 4 )
        //        }
        //    },
        //    new MaintenanceItem( "Item 5", DateTime.Now.AddDays(4) )
        //    {
        //        NextServiceDate = DateTime.Now.AddDays( 5 ),
        //        MaterialsAndEquipment = new List<Material>()
        //        {
        //            new Material( "Mat1", "Store1", 10.00d, 1 ),
        //            new Material( "Mat2", "Store2", 11.00d, 2 ),
        //            new Material( "Mat3", "Store3", 12.00d, 3 ),
        //            new Material( "Mat4", "Store4", 13.00d, 4 )
        //        }
        //    }

        //};

        private readonly MaintenanceItem defaultItem = new MaintenanceItem( "Item 1" )
        {
            NextServiceDate = DateTime.Now.AddDays( 1 ),
            //MaterialsAndEquipment = new List<Material>()
            //    {
            //        new Material( "Mat1", "Store1", 10.00d, 1 ),
            //        new Material( "Mat2", "Store2", 11.00d, 2 ),
            //        new Material( "Mat3", "Store3", 12.00d, 3 ),
            //        new Material( "Mat4", "Store4", 13.00d, 4 ),
            //        new Material( "Mat5", "Store5", 13.00d, 5 ),
            //        new Material( "Mat6", "Store6", 13.00d, 6 ),
            //        new Material( "Mat7", "Store7", 13.00d, 7 ),
            //        new Material( "Mat8", "Store8", 13.00d, 8 ),
            //        new Material( "Mat9", "Store9", 13.00d, 9 ),
            //        new Material( "Mat10", "Store10", 13.00d, 10 )
            //    }
        };

        public async Task Init()
        {
            if( db != null )
            {
                return;
            }

            string dbPath = Path.Combine( FileSystem.AppDataDirectory, "MaintenanceItems.db" );

            db = new SQLiteAsyncConnection( dbPath );

            _ = await db.CreateTableAsync<MaintenanceItem>();

            if( await db.Table<MaintenanceItem>().CountAsync() < 1 )
            {
                string json = JsonConvert.SerializeObject( defaultItem );
                _ = db.InsertAsync( json );
            }

        }

        public async Task AddItemAsync( MaintenanceItem item )
        {
            await Init();
            
            _ = await db.InsertAsync( JsonConvert.SerializeObject( item ) );
        }

        public async Task DeleteItemAsync( int id )
        {
            await Init();
            _ = await db.Table<MaintenanceItem>().DeleteAsync( x => x.Id == id );
        }

        public async Task<MaintenanceItem> GetItemAsync( int id )
        {
            await Init();
            return await db.Table<MaintenanceItem>().Where( x => x.Id == id ).FirstAsync();

        }

        public async Task<IEnumerable<MaintenanceItem>> GetAllItemsAsync( bool forceRefresh = false )
        {
            await Init();

            List<MaintenanceItem> data = await db.Table<MaintenanceItem>().ToListAsync();

            return data;
        }

        public async Task UpdateItemAsync( MaintenanceItem item )
        {
            await Init();
            _ = await db.InsertOrReplaceAsync( item );
        }
    }

    //new MaintenanceItem( "Item 1", DateTime.Now )
    //{
    //    NextServiceDate = DateTime.Now.AddDays( 1 ),
    //            MaterialsAndEquipment = new List<Material>()
    //            {
    //                new Material( "Mat1", "Store1", 10.00d, 1 ),
    //                new Material( "Mat2", "Store2", 11.00d, 2 ),
    //                new Material( "Mat3", "Store3", 12.00d, 3 ),
    //                new Material( "Mat4", "Store4", 13.00d, 4 ),
    //                new Material( "Mat5", "Store5", 13.00d, 5 ),
    //                new Material( "Mat6", "Store6", 13.00d, 6 ),
    //                new Material( "Mat7", "Store7", 13.00d, 7 ),
    //                new Material( "Mat8", "Store8", 13.00d, 8 ),
    //                new Material( "Mat9", "Store9", 13.00d, 9 ),
    //                new Material( "Mat10", "Store10", 13.00d, 10 )
    //            }
    //        }
}
