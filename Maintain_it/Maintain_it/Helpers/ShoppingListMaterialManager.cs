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
        /// Creates a new instance of a ShoppingListMaterial and adds it to the database. Also adds the newly created SLM to the ShoppingList with the passed in Id.
        /// </summary>
        /// <returns>The Id of the newly created ShoppingListMaterial</returns>
        public static async Task<int> NewShoppingListMaterial( int materialId, int shoppingListId, string name = null, int quantityRequired = 1, bool purchased = false )
        {
            Material m = await MaterialManager.GetItemAsync( materialId );
            ShoppingList list = await ShoppingListManager.GetItemAsync( shoppingListId );

            ShoppingListMaterial mat = new ShoppingListMaterial()
            {
                Material = m,
                ShoppingList = list,
                Name = m.Name,
                Quantity = quantityRequired,
                Purchased = purchased,
                CreatedOn = DateTime.UtcNow
            };

            int matId = await DbServiceLocator.AddItemAndReturnIdAsync( mat );
            await ShoppingListManager.InsertNewShoppingListMaterial( list.Id, matId );

            return matId;
        }

        #region Item Modification
        /// <summary>
        /// Updates the ShoppingListMaterial with the passed in Id with the passed in values.
        /// </summary>
        public static async Task UpdateItemAsync( int id, int? materialId = null, int? shoppingListId = null, string? name = null, int? quantity = null, bool? purchased = null )
        {
            ShoppingListMaterial item = await GetItemRecursiveAsync(id);

            if( materialId != null && materialId != 0 )
                item.Material = await MaterialManager.GetItemAsync( materialId.Value );

            if( shoppingListId != null && shoppingListId != 0 )
                item.ShoppingList = await ShoppingListManager.GetItemAsync( shoppingListId.Value );

            item.Name = name ?? item.Name;
            item.Quantity = quantity ?? item.Quantity;
            item.Purchased = purchased ?? item.Purchased;
        }

        public static async Task PurchaseItemAsync( int id, bool purchased, int? alternateQuantity = null )
        {
            ShoppingListMaterial item = await GetItemRecursiveAsync(id);
            
            item.Purchased = purchased;

            int amount = alternateQuantity != null && alternateQuantity.Value > 0 ? alternateQuantity.Value : item.Quantity;

            switch( purchased )
            {
                case true:
                    await MaterialManager.IncreaseMaterialQuantity( item.MaterialId, amount);
                    break;

                case false:
                    await MaterialManager.DecreaseMaterialQuantity( item.MaterialId, amount);
                    break;
            }

            await DbServiceLocator.UpdateItemAsync( item );
        }
        #endregion Item Modification

        #region Item Retrieval

        /// <summary>
        /// Retrieves the ShoppingListMaterial with the passed in Id from the Db and returns it.
        /// </summary>
        public static async Task<ShoppingListMaterial> GetItemAsync( int id )
        {
            return await DbServiceLocator.GetItemAsync<ShoppingListMaterial>( id );
        }

        public static async Task<ShoppingListMaterial> GetItemRecursiveAsync( int id )
        {
            ShoppingListMaterial item = await DbServiceLocator.GetItemRecursiveAsync<ShoppingListMaterial>(id);

            item.Material ??= await DbServiceLocator.GetItemAsync<Material>( item.MaterialId );

            item.ShoppingList ??= await DbServiceLocator.GetItemAsync<ShoppingList>( item.ShoppingListId );

            return item;
        }

        public static async Task<ShoppingListMaterial> GetItemRangeAsync( IEnumerable<int> ids )
        {
            throw new NotImplementedException();
        }

        public static async Task<ShoppingListMaterial> GetItemRangeRecursiveAsync( IEnumerable<int> ids )
        {
            throw new NotImplementedException();
        }

        #endregion Item Retrieval
    }
}
