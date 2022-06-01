using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.ViewModels;

namespace Maintain_it.Helpers
{
    internal static class RoutingPath
    {
        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="AddMaterialsViewModel"/></item>
        /// </list>
        /// </summary>
        public const string Refresh = "refresh";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="AddStepMaterialToStepViewModel"/></item>
        /// </list>
        /// </summary>
        public const string MaterialID = "materialId";
        
        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="AddStepMaterialToStepViewModel"/></item>
        /// </list>
        /// </summary>
        public const string MaterialIds = "materialIds";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="AddMaterialsToShoppingListViewModel"/></item>
        /// </list>
        /// </summary>
        public const string ShoppingListId = "shoppingListId";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="CreateNewMaterialViewModel"/></item>
        /// <item><see cref="AddMaterialsToShoppingListViewModel"/></item>
        /// </list>
        /// </summary>
        public const string ShoppingListMaterialIds = "shoppingListMaterialIds";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="ShoppingListMaterialDetailViewModel"/></item>
        /// </list>
        /// </summary>
        public const string ShoppingListMaterialId = "shoppingListMaterialId";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="PerformMaintenanceViewModel"/></item>
        /// </list>
        /// </summary>
        public const string MaintenanceItemId = "maintenanceItemId";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="CreateNewShoppingListViewModel"/></item>
        /// </list>
        /// </summary>
        public const string PreSelectedMaterialIds = "preSelectedMaterialIds";

        /// <summary>
        /// Query Param for:
        /// <list type="bullet">
        /// <item><see cref="CreateNewShoppingListViewModel"/></item>
        /// </list>
        /// </summary>
        public const string ItemName = "itemName";
    }
}
