using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    public static class ShoppingListManager
    {
        public static async Task<int> NewShoppingList( string name = DefaultStrings.DefaultShoppingListName, bool active = true )
        {
            List<ShoppingList> allLists = await DbServiceLocator.GetAllItemsAsync<ShoppingList>() as List<ShoppingList>;

            foreach( ShoppingList list in allLists )
            {
                if( list.Name == name )
                {
                    Console.WriteLine( $"{list.Name} == {name}" );
                }

                if( list.Name.StartsWith( name ) )
                {
                    Console.WriteLine( $"{list.Name} STARTSWITH {name}" );
                }
            }
            List<ShoppingList> nameMatchLists = allLists.Where( x => x.Name.StartsWith( name ) || x.Name == name ).ToList();

            if( nameMatchLists != null  && nameMatchLists.Count > 0 )
            {
                name = $"{name} ({nameMatchLists.Count})";
            }

            ShoppingList newList = new ShoppingList()
            {
                Name = name,
                Active = active,
                CreatedOn = DateTime.UtcNow,
                LooseMaterials = new List<ShoppingListMaterial>(),
                ServiceItems = new List<ServiceItem>()
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync( newList );
            return id;
        }

        public static async Task<ShoppingList> GetItemAsync( int shoppingListId )
        {
            return await DbServiceLocator.GetItemAsync<ShoppingList>( shoppingListId );
        }

        public static async Task<ShoppingList> GetItemRecursiveAsync( int shoppingListId )
        {
            ShoppingList list =  await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( shoppingListId );

            return list;
        }

        public static async Task<List<ShoppingList>> GetAllItemsAsync()
        {
            List<ShoppingList> list = await DbServiceLocator.GetAllItemsAsync<ShoppingList>() as List<ShoppingList>;
            return list;
        }

        /// <summary>
        /// Updates the ShoppingList with the passed in Id and values. ShoppingLists are never allowed to have the same name, so if the passed in name matches another ShoppingList in the database the name will be appended with in integer corresponding to the number of matches found. i.e. if there is already a ShoppingList with the name "New List" then attempting to add or change the name of a nameMatchLists to "New List" will result in a nameMatchLists named "New List 1". If a user attempts to add another nameMatchLists with the name "New List" they will get a nameMatchLists named "New List 2" back.
        /// </summary>
        public static async Task<bool> UpdateShoppingListAsync( int id, string? name = null, bool? active = null, List<ShoppingListMaterial>? looseMaterials = null, IEnumerable<ServiceItem>? serviceItems = null, int modifier = 0 )
        {
            List<ShoppingList> allLists = await GetAllItemsAsync();

            List<ShoppingList> listsWithSameName = null;

            if( name != null )
            {
                listsWithSameName = allLists?.Where( x => x.Name.StartsWith( name ) && x.Id != id ).ToList();

                if( listsWithSameName != null && listsWithSameName.Count > 0 )
                {
                    name = $"{name} ({listsWithSameName.Count})";
                }
            }

            ShoppingList list = allLists?.Where( x => x.Id == id ).FirstOrDefault();

            // Updates the name if it is not null. Adds a modifier # if it has been changed to a name that already exists in the db
            list.Name = name ?? list.Name;
            list.Active = active ?? list.Active;
            list.LooseMaterials = looseMaterials ?? list.LooseMaterials;
            list.ServiceItems = (List<ServiceItem>)( serviceItems ?? list.ServiceItems );

            await DbServiceLocator.UpdateItemAsync( list );
            return true;
        }

        public static async Task InsertNewShoppingListMaterial( int shoppingListId, int shoppingListItemId )
        {
            ShoppingList list = await GetItemRecursiveAsync(shoppingListId);
            ShoppingListMaterial item = await ShoppingListMaterialManager.GetItemAsync(shoppingListItemId);

            if( !list.LooseMaterials.Contains( item ) )
                list.LooseMaterials.Add( item );

            if( item.ShoppingListId != shoppingListId )
                await ShoppingListMaterialManager.UpdateItemAsync( item.Id, list.Id );

            await DbServiceLocator.UpdateItemAsync( list );
        }

        public static async Task AddShoppingListItemsToListAsync( int shoppingListId, IDictionary<int, int> matsAndQuants )
        {
            ShoppingList list = await GetItemRecursiveAsync(shoppingListId);

            if( list.LooseMaterials != null && list.LooseMaterials.Count > 0 )
            {
                foreach( ShoppingListMaterial mat in list.LooseMaterials )
                {
                    if( matsAndQuants.ContainsKey( mat.MaterialId ) )
                    {
                        int quant = mat.Quantity + matsAndQuants[mat.MaterialId];
                        await ShoppingListMaterialManager.UpdateItemAsync( mat.Id, quantity: quant );
                    }

                    _ = matsAndQuants.Remove( mat.MaterialId );
                }
            }

            List<Material> mats = await DbServiceLocator.GetItemRangeAsync<Material>( matsAndQuants.Keys.ToArray() ) as List<Material>;

            foreach( Material mat in mats )
            {
                _ = await ShoppingListMaterialManager.NewShoppingListMaterial( mat.Id, list.Id, mat.Name, matsAndQuants[mat.Id] );
            }
        }

        /// <summary>
        /// Adds the passed in ServiceItem to the passed in ShoppingList if it does not already exist. Otherwise just returns.
        /// </summary>
        public static async Task AddServiceItemToShoppingList( int shoppingListId, int serviceItemId )
        {
            ShoppingList list = await GetItemRecursiveAsync( shoppingListId );
            HashSet<int> ids = new HashSet<int>( list.ServiceItems.GetIds() );

            if( !ids.Contains( serviceItemId ) )
            {
                ServiceItem item = await ServiceItemManager.GetItemRecursiveAsync( serviceItemId );

                list.ServiceItems.Add( item );
                item.ShoppingLists.Add( list );

                await DbServiceLocator.UpdateItemAsync( list );
                await DbServiceLocator.UpdateItemAsync( item );
            }
        }

        public static async Task<Dictionary<string, int>> GetActiveShoppingListNamesAndIds()
        {

            List<ShoppingList> allLists = await GetAllItemsAsync();

            List<KeyValuePair<string, int>> values = allLists.Select( x => KeyValuePair.Create( x.Name, x.Id ) ).ToList();

            Dictionary<string, int> pairs = new Dictionary<string, int>( values );

            return pairs;
        }

        public static async Task<int> GetItemIdByName( string name )
        {
            List<ShoppingList> shoppingLists = await DbServiceLocator.GetAllItemsAsync<ShoppingList>() as List<ShoppingList>;

            int id = 0;

            if( shoppingLists != null )
            {
                ShoppingList sl = shoppingLists.Where(x => x.Name == name).FirstOrDefault();

                id = sl != null ? sl.Id : 0;
            }

            return id;
        }
    }
}
