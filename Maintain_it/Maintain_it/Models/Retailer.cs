using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Retailer : IStorableObject
    {
        public Retailer() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ManyToMany( typeof( MaterialsToRetailers ) )]
        public List<Material> Materials { get; set; }

        #region Properties
        public string Name { get; set; }
        #endregion
    }
}
