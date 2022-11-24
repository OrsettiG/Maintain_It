using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class PreferredRetailer : IStorableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true)]
        public List<Material> Materials { get; set; }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
