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

        public DbServiceLocator()
        {
            _maintenanceItemService = new MaintenanceItemService();
            _materialService = new MaterialService();
            _stepService = new StepService();
            _stepMaterialService = new StepMaterialService();
            _shoppingListItemService = new ShoppingListItemService();
            _shoppingListService = new ShoppingListService();
            _quantityService = new QuantityService();
            _noteService = new NoteService();
            _photoService = new PhotoService();
            _retailerService = new RetailerService();

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

        private readonly MaintenanceItemService  _maintenanceItemService;
        private readonly MaterialService         _materialService;
        private readonly StepService             _stepService;
        private readonly StepMaterialService     _stepMaterialService;
        private readonly ShoppingListItemService _shoppingListItemService;
        private readonly ShoppingListService     _shoppingListService;
        private readonly QuantityService         _quantityService;
        private readonly NoteService             _noteService;
        private readonly PhotoService            _photoService;
        private readonly RetailerService         _retailerService;

        public async Task Init()
        {
            await _maintenanceItemService.Init();
            await _materialService.Init();
            await _stepService.Init();
            await _stepMaterialService.Init();
            await _shoppingListItemService.Init();
            await _shoppingListService.Init();
            await _quantityService.Init();
            await _noteService.Init();
            await _photoService.Init();
            await _retailerService.Init();
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
