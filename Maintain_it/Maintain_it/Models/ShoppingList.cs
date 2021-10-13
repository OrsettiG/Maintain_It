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

        [ManyToMany( typeof( ShoppingListItem ) )]
        public List<ShoppingListItem> Items { get; set; }

        #endregion

        #region Properties
        public string Name { get; set; }
        #endregion
    }
}