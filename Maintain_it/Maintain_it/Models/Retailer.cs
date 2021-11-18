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

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<RetailerMaterial> Materials { get; set; }

        #region Properties
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public DateTime CreatedOn { get; set; }
        #endregion
    }
}
