using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ManyToMany(typeof(Material))]
        public List<Material> Materials { get; set; }

        public string Value { get; set; }

        public TagType TagType { get; set; }
    }
}
