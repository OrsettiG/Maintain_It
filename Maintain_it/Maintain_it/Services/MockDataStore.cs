using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class MockDataStore : IDataStore<MaintenanceItem>
    {
        private readonly List<MaintenanceItem> items;

        public MockDataStore()
        {
            items = new List<MaintenanceItem>()
            {
                new MaintenanceItem(),
                new MaintenanceItem(),
                new MaintenanceItem(),
                new MaintenanceItem(),
                new MaintenanceItem(),
                new MaintenanceItem()
            };
        }

        public async Task<bool> AddItemAsync( MaintenanceItem item )
        {
            items.Add( item );

            return await Task.FromResult( true );
        }

        public async Task<bool> UpdateItemAsync( MaintenanceItem item )
        {
            var oldItem = items.Where( ( MaintenanceItem arg ) => arg.Id == item.Id ).FirstOrDefault();
            items.Remove( oldItem );
            items.Add( item );

            return await Task.FromResult( true );
        }

        public async Task<bool> DeleteItemAsync( int id )
        {
            var oldItem = items.Where( ( MaintenanceItem arg ) => arg.Id == id ).FirstOrDefault();
            items.Remove( oldItem );

            return await Task.FromResult( true );
        }

        public async Task<MaintenanceItem> GetItemAsync( int id )
        {
            return await Task.FromResult( items.FirstOrDefault( s => s.Id == id ) );
        }

        public async Task<IEnumerable<MaintenanceItem>> GetItemsAsync( bool forceRefresh = false )
        {
            return await Task.FromResult( items );
        }

        Task IDataStore<MaintenanceItem>.AddItemAsync( MaintenanceItem item )
        {
            throw new NotImplementedException();
        }

        Task IDataStore<MaintenanceItem>.UpdateItemAsync( MaintenanceItem item )
        {
            throw new NotImplementedException();
        }

        Task IDataStore<MaintenanceItem>.DeleteItemAsync( int id )
        {
            throw new NotImplementedException();
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MaintenanceItem>> GetAllItemsAsync( bool forceRefresh = false )
        {
            throw new NotImplementedException();
        }
    }
}