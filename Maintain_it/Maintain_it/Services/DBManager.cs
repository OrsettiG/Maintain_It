using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

namespace Maintain_it.Services
{
    public class DBManager : IDataStore<MaintenanceItem>
    {
        private SQLiteAsyncConnection connection;
        private bool firstTimeSetup = true;
        private readonly List<MaintenanceItem> items = new List<MaintenanceItem>()
        {
            new MaintenanceItem( "Item 1", DateTime.Now )
            {
                NextServiceDate = DateTime.Now.AddDays( 1 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                }
            },
            new MaintenanceItem( "Item 2", DateTime.Now.AddDays(1) )
            {
                NextServiceDate = DateTime.Now.AddDays( 2 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                }
            },
            new MaintenanceItem( "Item 3", DateTime.Now.AddDays(2) )
            {
                NextServiceDate = DateTime.Now.AddDays( 3 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                }
            },
            new MaintenanceItem( "Item 4", DateTime.Now.AddDays(3) )
            {
                NextServiceDate = DateTime.Now.AddDays( 4 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                }
            },
            new MaintenanceItem( "Item 5", DateTime.Now.AddDays(4) )
            {
                NextServiceDate = DateTime.Now.AddDays( 5 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                }
            }

        };

        private async Task CreateConnection()
        {
            if( connection == null )
            {
                string documentPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
                string dbPath = Path.Combine( documentPath, "MaintenanceItems.db" );

                connection = new SQLiteAsyncConnection( dbPath );

                _ = await connection.CreateTableAsync<MaintenanceItem>();

                if( await connection.Table<MaintenanceItem>().CountAsync() != 0 && firstTimeSetup )
                {
                    _ = connection.DeleteAllAsync<MaintenanceItem>();

                    foreach(MaintenanceItem item in items )
                    {
                        _ = await connection.InsertAsync( item );

                    }


                    firstTimeSetup = false;
                }
                
                Console.WriteLine( $"------------------------------ Rows {await connection.Table<MaintenanceItem>().CountAsync()} ------------------------------" );
            }
        }

        public async Task<bool> AddItemAsync( MaintenanceItem item )
        {
            await CreateConnection();

            return await connection.InsertAsync( item ) != 0;
        }

        public async Task<bool> DeleteItemAsync( int id )
        {
            await CreateConnection();

            return await connection.Table<MaintenanceItem>().DeleteAsync( x => x.Id == id ) == (int)SQLite3.Result.Done;
        }

        public async Task<MaintenanceItem> GetItemAsync( int id )
        {
            await CreateConnection();

            return await connection.Table<MaintenanceItem>().Where( mi => mi.Id == id ).FirstAsync();
        }

        public async Task<IEnumerable<MaintenanceItem>> GetItemsAsync( bool forceRefresh = true )
        {
            await CreateConnection();

            return await connection.Table<MaintenanceItem>().OrderBy( x => x.NextServiceDate ).ToListAsync();
        }

        public async Task<bool> UpdateItemAsync( MaintenanceItem item )
        {
            await CreateConnection();
            return await connection.UpdateAsync( item ) != 0;
        }
    }
}
