using System.Collections.Generic;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class ShoppingListService : Service<ShoppingList>
    {
        internal static ShoppingList defaultShoppingList = new ShoppingList()
        {
            Name = "Default Shopping List",
            Items = new List<ShoppingListItem>()
            {
                ShoppingListItemService.defaultShoppingListItem
            }
        };

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<ShoppingList>() == null )
            {
                _ = await db.CreateTableAsync<ShoppingList>();
            }

            if( await db.Table<ShoppingList>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( defaultShoppingList );
            }
        }
    }
}