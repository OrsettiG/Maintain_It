﻿using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Tag : IStorableObject, IEquatable<Tag>
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ManyToMany(typeof(MaterialTag))]
        public List<Material> Materials { get; set; }

        [Unique]
        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool Equals( Tag other )
        {
            return other?.Id == Id;
        }
    }
}
