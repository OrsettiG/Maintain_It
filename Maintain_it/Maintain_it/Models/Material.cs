using System.Collections.Generic;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Material
    {

        public Material() { }

        public Material( string Name )
        {
            this.Name = Name;
            //this.Retailer = Retailer;
            //this.UnitPrice = UnitPrice;
            //this.Quantity = QuantityOwned;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region One To Many Relationships
        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<StepMaterial> StepMaterials { get; set; }
        #endregion

        #region Many To One Relationships
        [ForeignKey( typeof( Quantity ) )]
        public int QuantityId { get; set; }
        [ManyToOne]
        public Quantity Quantity { get; set; }
        #endregion

        #region Many To Many Relationships
        [ManyToMany( typeof( ItemsToMaterials ) )]
        public List<MaintenanceItem> MaintenanceItem { get; set; }

        [ManyToMany( typeof( Retailer ) )]
        public List<Retailer> Retailers { get; set; }

        #endregion



        #region Properties
        public string Name { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => QuantityId * UnitPrice;
        #endregion
    }
}