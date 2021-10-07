using System.Collections.Generic;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingListItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships

        [ForeignKey( typeof( StepMaterial ) )]
        public int StepMaterialId { get; set; }
        [ManyToOne]
        public StepMaterial StepMaterial { get; set; }

        [ForeignKey( typeof( Quantity ) )]
        public int QuantityId { get; set; }
        [ManyToOne]
        public Quantity Quantity { get; set; }

        #endregion

        #region Many To Many Relationships

        [ManyToMany( typeof( ShoppingListItemToShoppingList ) )]
        public List<ShoppingList> ShoppingLists { get; set; }

        #endregion

        #region Properties

        public bool Purchased { get; set; }

        #endregion

    }
}