using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using Xamarin.Forms;

namespace Maintain_it.Helpers
{
    public static class ShoppingListMaterialManager
    {
        /// <summary>
        /// Creates a new instance of a ShoppingListMaterial and adds it to the database.
        /// </summary>
        /// <returns>The Id of the newly created ShoppingListMaterial</returns>
        public static async Task<int> NewShoppingListMaterial( int materialId, int shoppingListId, string name, int quantityRequired = 1, bool purchased = false )
        {
            Material m = await MaterialManager.GetItemAsync( materialId );
            ShoppingList list = await ShoppingListManager.GetItemAsync( shoppingListId );

            ShoppingListMaterial mat = new ShoppingListMaterial()
            {
                Material = m,
                ShoppingList = list,
                Name = name,
                Quantity = quantityRequired,
                Purchased = purchased,
                CreatedOn = DateTime.Now
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( mat );
        }

        /// <summary>
        /// Retrieves the ShoppingListMaterial with the passed in Id from the Db and returns it.
        /// </summary>
        public static async Task<ShoppingListMaterial> GetItemAsync( int id )
        {

            return await DbServiceLocator.GetItemAsync<ShoppingListMaterial>( id );
        }

        public static async Task<ShoppingListMaterial> GetItemRecursiveAsync( int id )
        {
            throw new NotImplementedException();
        }

        public static async Task<ShoppingListMaterial> GetItemRangeAsync( IEnumerable<int> ids )
        {
            throw new NotImplementedException();
        }

        public static async Task<ShoppingListMaterial> GetItemRangeRecursiveAsync( IEnumerable<int> ids )
        {
            throw new NotImplementedException();
        }
    }
}
