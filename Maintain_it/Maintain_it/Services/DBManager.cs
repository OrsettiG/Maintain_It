using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

namespace Maintain_it.Services
{
    public class DBManager : IDataStore<MaintenanceItem>
    {
        private SQLiteAsyncConnection connection;

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
                    new Material( "Mat5", "Store5", 13.00d, 5 ),
                    new Material( "Mat6", "Store6", 13.00d, 6 ),
                    new Material( "Mat7", "Store7", 13.00d, 7 ),
                    new Material( "Mat8", "Store8", 13.00d, 8 ),
                    new Material( "Mat9", "Store9", 13.00d, 9 ),
                    new Material( "Mat10", "Store10", 13.00d, 10 )
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
                    new Material( "Mat4", "Store4", 13.00d, 4 )
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
                    new Material( "Mat4", "Store4", 13.00d, 4 )
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
                    new Material( "Mat4", "Store4", 13.00d, 4 )
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
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            }

        };

        private async Task CreateConnection()
        {
            if( connection != null )
            {
                return;
            }

            string documentPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            string dbPath = Path.Combine( documentPath, "MaintenanceItems.db" );

            connection = new SQLiteAsyncConnection( dbPath );
            _ = await connection.CreateTableAsync<MaintenanceItem>();

            if( await connection.Table<MaintenanceItem>().CountAsync() < 10 )
            {
                _ = await connection.InsertAsync( new MaintenanceItem( "Default Item", DateTime.Now )
                {
                    NextServiceDate = DateTime.Now.AddDays( 1 ),
                    MaterialsAndEquipment = new List<Material>()
                    {
                        new Material( "This is a long item that might cause problems", "And a long store name that might equally cause problems", 10.00d, 1 ),
                        new Material( "Mat2", "Store2", 11.00d, 2 ),
                        new Material( "Medium material name", "Store3", 12.00d, 3 ),
                        new Material( "Mat4", "medium store name", 13.00d, 4 ),
                        new Material( "Mat5", "Store5", 13.00d, 5 ),
                        new Material( "Mat6", "Store6", 13.00d, 6 ),
                        new Material( "Mat7", "Store7", 13.00d, 7 ),
                        new Material( "Mat8", "Store8", 13.00d, 8 ),
                        new Material( "Mat9", "Store9", 13.00d, 9 ),
                        new Material( "Mat10", "Store10", 13.00d, 10 )
                    }
                } );
            }
        }

        public async Task<bool> AddItemAsync( MaintenanceItem item )
        {
            await CreateConnection();
            int id = await connection.InsertAsync( item );

            return id > 0;
        }

        public async Task<bool> DeleteItemAsync( int id )
        {
            await CreateConnection();
            int Id = await connection.Table<MaintenanceItem>().DeleteAsync( x => x.Id == id );

            return Id > 0;
        }

        public IEnumerable<MaintenanceItem> GetFakeData()
        {
            return items;
        }

        public async Task<MaintenanceItem> GetItemAsync( int id )
        {
            await CreateConnection();
            return await connection.Table<MaintenanceItem>().Where( x => x.Id == id ).FirstAsync();

        }

        public async Task<IEnumerable<MaintenanceItem>> GetItemsAsync( bool forceRefresh = false )
        {
            await CreateConnection();
            return await connection.Table<MaintenanceItem>().ToListAsync();
        }

        public async Task<bool> UpdateItemAsync( MaintenanceItem item )
        {
            await CreateConnection();
            int rows = await connection.InsertOrReplaceAsync( item );

            return rows > 0;
        }
    }
}
