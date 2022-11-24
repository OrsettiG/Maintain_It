using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingListMaterial : IStorableObject, IEquatable<ShoppingListMaterial>
    {
        public ShoppingListMaterial() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships

        //Material
        [ForeignKey(typeof(Material))]
        public int MaterialId { get; set; }
        [ManyToOne( CascadeOperations = CascadeOperation.CascadeRead )]
        public Material Material { get; set; }

        //ShoppingList
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

        public bool Equals( ShoppingListMaterial other )
        {
            return other?.Id == Id;
        }
        #endregion

    }
}