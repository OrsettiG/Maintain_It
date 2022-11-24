using System;
using System.Collections.Generic;
using System.Text;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ServiceItemShoppingList
    {
        [ForeignKey(typeof(ServiceItem))]
        public int ServiceItemId { get; set; }

        [ForeignKey(typeof(ShoppingList))]
        public int ShoppingListId { get; set; }
    }
}
