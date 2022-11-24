using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingList : IStorableObject, IEquatable<ShoppingList>
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To Many Relationships

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<ShoppingListMaterial> LooseMaterials { get; set; }

        [ManyToMany( typeof( ServiceItemShoppingList ), CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true )]
        public List<ServiceItem> ServiceItems { get; set; }

        #endregion

        #region Properties

        [Unique]
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Active { get; set; }

        #endregion

        public bool Equals( ShoppingList other )
        {
            return other?.Id == Id;
        }
    }
}