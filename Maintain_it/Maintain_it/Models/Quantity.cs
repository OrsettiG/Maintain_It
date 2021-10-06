using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Quantity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<Material> Materials { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<StepMaterial> StepMaterials { get; set; }
        
        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<ShoppingListItem> ShoppingListItems { get; set; }
        
        public int Value { get; set; }
    }
}
