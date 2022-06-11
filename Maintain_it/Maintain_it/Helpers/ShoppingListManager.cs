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
        private static List<ShoppingList> _lists_;
        private static List<ShoppingList> AllLists
        {
            get => _lists_ ??= Task.Run( async () => await GetAllItemsAsync() ).Result;
            set => _lists_ = value;
        }

        public static async Task<int> NewShoppingList( string name = DefaultStrings.DefaultShoppingListName, bool active = true )
        {
            ShoppingList list = AllLists?.Where( x => x.Name == name ).FirstOrDefault();
            
            if( AllLists == null || list == null )
            {
                list = new ShoppingList()
                {
                    Name = name,
                    Active = active,
                    CreatedOn = DateTime.UtcNow,
                    Materials = new List<ShoppingListMaterial>()
                };

                int id = await DbServiceLocator.AddItemAndReturnIdAsync( list );
                AllLists = await GetAllItemsAsync();
                return id;
            }

            return list.Id;
        }

        public static async Task<ShoppingList> GetItemAsync( int shoppingListId )
        {
            return await DbServiceLocator.GetItemAsync<ShoppingList>( shoppingListId );
        }

        public static async Task<List<ShoppingList>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<ShoppingList>() as List<ShoppingList>;
        }


        public static async Task<bool> UpdateShoppingListAsync( int id, string? name = null, bool? active = null, List<ShoppingListMaterial>? materials = null, int modifier = 0 )
        {
            List<ShoppingList> listsWithSameName = null;

            if( name != null )
            {
                listsWithSameName = modifier == 0 ? AllLists?.Where( x => x.Name == name ).ToList() : AllLists?.Where( x => x.Name == $"{name} {modifier}" ).ToList();
                
                if( listsWithSameName != null && listsWithSameName.Count > 0 )
                {
                    modifier++;
                    return await UpdateShoppingListAsync( id, name, active, materials, modifier );
                }
            }

            ShoppingList list = AllLists?.Where( x => x.Id == id ).FirstOrDefault();

            if( modifier != 0 )
            {
                list.Name = name != null ? $"{name} {modifier}" : list.Name;
                list.Active = active ?? list.Active;
                list.Materials = materials ?? list.Materials;

                //await Task.Delay( 1 );

                await DbServiceLocator.UpdateItemAsync( list );
                return true;
            }
            else
            {
                list.Name = name ?? list.Name;
                list.Active = active ?? list.Active;
                list.Materials = materials ?? list.Materials;

                await DbServiceLocator.UpdateItemAsync( list );
                return true;
            }

            return false;
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
