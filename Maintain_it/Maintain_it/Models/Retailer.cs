﻿using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Retailer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ManyToMany( typeof( MaterialsToRetailers ) )]
        public List<Material> Materials { get; set; }

        #region Properties
        public string Name { get; set; }
        #endregion
    }
}
