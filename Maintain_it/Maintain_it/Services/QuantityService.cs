using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class QuantityService : Service<Quantity>
    {
        internal static Quantity defaultQuantity = new Quantity()
        {
            Materials = new List<Material>()
            {
                MaterialService.defaultMaterial
            },
            StepMaterials = new List<StepMaterial>()
            {
                StepMaterialService.defaultStepMaterial
            },
            ShoppingListItems = new List<ShoppingListItem>()
            {
                ShoppingListItemService.defaultShoppingListItem
            },
            Count = 1
        };

        public override async Task Init()
        {
            await base.Init();

            if( db.Table<Quantity>() == null )
            {
                _ = await db.CreateTableAsync<Quantity>();
            }

            if( await db.Table<Quantity>().CountAsync() < 1 )
            {
                _ = await db.InsertAsync( defaultQuantity );
            }
        }
    }
}
