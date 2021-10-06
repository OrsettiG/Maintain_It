using System;
using System.Collections.Generic;
using System.Text;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    internal class MaterialsToRetailers
    {
        [ForeignKey( typeof( Material ) )]
        public int MaterialId { get; set; }

        [ForeignKey( typeof( Retailer ) )]
        public int RetailerId { get; set; }
    }
}
