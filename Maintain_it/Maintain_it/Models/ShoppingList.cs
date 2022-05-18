using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingList : IStorableObject
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To Many Relationships

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<ShoppingListMaterial> Materials { get; set; }

        #endregion

        #region Properties
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public DateTime CreatedOn { get; set; }
        public bool Active { get; set; }
        #endregion
    }
}