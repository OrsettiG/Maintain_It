using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class RetailerMaterial : IStorableObject
    {
        #region ManyToOne Relationships
        
        [ForeignKey( typeof( Material ) )]
        public int MaterialId { get; set; }
        [ManyToOne]
        public Material Material { get; set; }

        [ForeignKey( typeof( Retailer ) )]
        public int RetailerId { get; set; }
        [ManyToOne]
        public Retailer Retailer { get; set; }

        #endregion

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion
    }
}
