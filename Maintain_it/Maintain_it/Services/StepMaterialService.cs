using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class StepMaterialService : Service<StepMaterial>
    {
        internal static StepMaterial defaultStepMaterial = new StepMaterial()
        {
            ShoppingListItems = new List<ShoppingListItem>()
            {
                ShoppingListItemService.defaultShoppingListItem
            },
            Steps = new List<Step>()
            {
                StepService.defaultStep
            },
            Quantity = QuantityService.defaultQuantity,
            Material = MaterialService.defaultMaterial
        };

        public override async Task Init()
        {
            await base.Init();

            if(db.Table<StepMaterial>() == null )
            {
                _ = await db.CreateTableAsync<StepMaterial>();
            }

            if( await db.Table<StepMaterial>().CountAsync() < 1 )
            {
                _ = db.InsertAsync( new StepMaterial()
                {

                } );
            }
        }
    }
}
