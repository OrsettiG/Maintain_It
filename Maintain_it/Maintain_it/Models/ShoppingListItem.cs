using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingListItem : IStorableObject
    {
        public ShoppingListItem() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships

        [ForeignKey( typeof( StepMaterial ) )]
        public int StepMaterialId { get; set; }
        [ManyToOne]
        public StepMaterial StepMaterial { get; set; }



        #endregion

        #region Many To Many Relationships

        [ManyToMany( typeof( ShoppingListItemToShoppingList ) )]
        public List<ShoppingList> ShoppingLists { get; set; }

        #endregion

        #region Properties
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public int Quantity { get; set; }
        public bool Purchased { get; set; }

        #endregion

    }
}