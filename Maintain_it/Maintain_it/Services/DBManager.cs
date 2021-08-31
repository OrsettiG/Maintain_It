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

        private async Task CreateConnection()
        {
            if( connection == null )
            {
                string documentPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
                string dbPath = Path.Combine( documentPath, "MaintenanceItems.db" );

                connection = new SQLiteAsyncConnection( dbPath );

                _ = await connection.CreateTableAsync<MaintenanceItem>();

                if( await connection.Table<MaintenanceItem>().CountAsync() == 0 )
                {
                    _ = await connection.InsertAsync( new MaintenanceItem( "Create new maintenance jobs", DateTime.Now.AddDays( 3 ) ) );
                }
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

        public async Task<IEnumerable<MaintenanceItem>> GetItemsAsync( bool forceRefresh = false )
        {
            await CreateConnection();

            return await connection.Table<MaintenanceItem>().OrderBy( x => x.NextServiceDate ).ToListAsync();
        }

        public async Task<bool> UpdateItemAsync( MaintenanceItem item )
        {
            throw new NotImplementedException();
        }
    }
}
