using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class ShoppingListItemService : Service<ShoppingListItem>
    {
        internal static ShoppingListItem defaultShoppingListItem = new ShoppingListItem()
        {
            ShoppingLists = new List<ShoppingList>()
            {
                ShoppingListService.defaultShoppingList
            },
            StepMaterial = StepMaterialService.defaultStepMaterial,
            Quantity = QuantityService.defaultQuantity,
            Purchased = false
        };

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<ShoppingListItem>() == null )
            {
                _ = await db.CreateTableAsync<ShoppingListItem>();
            }

            if( await db.Table<ShoppingListItem>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( defaultShoppingListItem );
            }
        }
    }
}
