using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class DbServiceLocator
    {
        public DbServiceLocator()
        {
            locator = this;
            Register( new Service<MaintenanceItem>() );
            Register( new Service<Material>() );
            Register( new Service<Step>() );
            Register( new Service<StepMaterial>() );
            Register( new Service<ShoppingList>() );
            Register( new Service<ShoppingListItem>() );
            Register( new Service<Retailer>() );
            Register( new Service<Quantity>() );
            Register( new Service<Note>() );
            Register( new Service<Photo>() );
        }

        public static DbServiceLocator locator = null;

        private static class LocatorEntry<T, U> where T : Service<U> where U : IStorableObject, new()
        {
            public static Service<U> Instance { get; set; }
        }

        public static void Register<T>( Service<T> instance ) where T : IStorableObject, new()
        {
            LocatorEntry<Service<T>, T>.Instance = instance;
        }

        public static Service<T> GetService<T>() where T : IStorableObject, new()
        {
            return LocatorEntry<Service<T>, T>.Instance;
        }

        public async Task Init()
        {
            await GetService<MaintenanceItem>().Init();
            await GetService<Material>().Init();
            await GetService<Step>().Init();
            await GetService<StepMaterial>().Init();
            await GetService<ShoppingListItem>().Init();
            await GetService<ShoppingList>().Init();
            await GetService<Retailer>().Init();
            await GetService<Quantity>().Init();
            await GetService<Note>().Init();
            await GetService<Photo>().Init();
        }

        public async Task AddItemAsync<T>( T item ) where T : IStorableObject, new()
        {
            Service<T> instance = GetService<T>();
            await instance.AddItemAsync( item );
        }

        public async Task DeleteItemAsync<T>( int id ) where T : IStorableObject, new()
        {
            Service<T> instance = GetService<T>();
            await instance.DeleteItemAsync( id );
        }

        public async Task<IEnumerable<T>> GetAllItemsAsync<T>() where T : IStorableObject, new()
        {
            Service<T> instance = GetService<T>();
            IEnumerable<T> data = await instance.GetAllItemsAsync();
            return data as List<T>;
        }

        public async Task<T> GetItemAsync<T>( int id ) where T : IStorableObject, new()
        {
            Service<T> instance = GetService<T>();
            return await instance.GetItemAsync( id );
        }

        public async Task UpdateItemAsync<T>( T item ) where T : IStorableObject, new()
        {
            Service<T> instance = GetService<T>();
            await instance.UpdateItemAsync( item );
        }
    }
}
