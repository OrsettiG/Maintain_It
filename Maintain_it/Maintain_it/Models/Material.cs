using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Material : IStorableObject, IEquatable<Material>
    {

        public Material() { }

        public Material( string Name )
        {
            this.Name = Name;
        }



        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Relationships

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true )]
        public List<StepMaterial> StepMaterials { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true )]
        public List<RetailerMaterial> RetailerMaterials { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true )]
        public List<ShoppingListMaterial> ShoppingListMaterials { get; set; }

        [ManyToMany( typeof( MaterialTag ) )]
        public List<Tag> Tags { get; set; }

        [ForeignKey(typeof(PreferredRetailer))]
        public int PreferredRetailerId { get; set; }
        [ManyToOne]
        public PreferredRetailer PreferredRetailer { get; set; }

        #endregion

        #region Properties
        public string Name { get; set; }

#nullable enable
        public double? Size { get; set; }
        public string? Units { get; set; }
        public string? Description { get; set; }
        public string? PartNumber { get; set; }
#nullable disable

        public int LifeExpectancy { get; set; }
        public int LifeExpectancyTimeframe { get; set; }
        public byte[] ImageBytes { get; set; }
        public int QuantityOwned { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool Equals( Material other )
        {
            return other?.Id == Id;
        }
        #endregion
    }
}