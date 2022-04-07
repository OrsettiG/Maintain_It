using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Material : IStorableObject
    {

        public Material() { }

        public Material( string Name )
        {
            this.Name = Name;
        }

        

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region One To Many Relationships

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<StepMaterial> StepMaterials { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<RetailerMaterial> RetailerMaterials { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All )]
        public List<ShoppingListMaterial> ShoppingListMaterials { get; set; }

        #endregion

        #region Properties
        public string Name { get; set; }

#nullable enable
        public double? Size { get; set; }
        public string? Tag { get; set; }
        public string? Units { get; set; }
        public string? Description { get; set; }
#nullable disable

        public int QuantityOwned { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion

        #region Methods
        
        #endregion
    }
}