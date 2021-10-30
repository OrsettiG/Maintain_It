﻿using System.Collections.Generic;

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
        #endregion

        #region Many To Many Relationships
        
        [ManyToMany( typeof( ItemsToMaterials ) )]
        public List<MaintenanceItem> MaintenanceItem { get; set; }

        [ManyToMany( typeof( MaterialsToRetailers ) )]
        public List<Retailer> Retailers { get; set; }

        #endregion

        #region Properties
        
        public string Name { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        
        #endregion
    }
}