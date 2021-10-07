using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ShoppingListItemToShoppingList
    {
        [ForeignKey( typeof( ShoppingListItem ) )]
        public int ShoppingListItemId { get; set; }

        [ForeignKey( typeof( ShoppingList ) )]
        public int ShoppingListId { get; set; }
    }
}