using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingListMaterial : IStorableObject
    {
        public ShoppingListMaterial() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships

        [ForeignKey(typeof(Material))]
        public int MaterialId { get; set; }
        [ManyToOne]
        public Material Material { get; set; }

        [ForeignKey(typeof(ShoppingList))]
        public int ShoppingListId { get; set; }
        [ManyToOne]
        public ShoppingList ShoppingList { get; set; }

        #endregion

        #region Properties
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public int Quantity { get; set; }
        public bool Purchased { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion

    }
}