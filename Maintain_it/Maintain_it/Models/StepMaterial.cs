using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class StepMaterial : IStorableObject
    {
        public StepMaterial() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships
        // Quantity
        [ForeignKey( typeof( Quantity ) )]
        public int QuantityId { get; set; }
        [ManyToOne]
        public Quantity Quantity { get; set; }
        
        // Material
        [ForeignKey( typeof( Material ) )]
        public int MaterialId { get; set; }
        [ManyToOne]
        public Material Material { get; set; }
        #endregion

        #region One to Many Relationships
        // ShoppingListItem
        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<ShoppingListItem> ShoppingListItems { get; set; }
        #endregion

        #region Many To Many Relationships
        // Step
        [ManyToMany( typeof( StepsToStepMaterials ) )]
        public List<Step> Steps { get; set; }
        #endregion
    }
}
