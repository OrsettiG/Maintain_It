using System;
using System.Collections.Generic;
using System.Text;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class MaterialTag
    {
        [ForeignKey(typeof(Material))]
        public int MaterialId { get; set; }

        [ForeignKey(typeof(Tag))]
        public int TagId { get; set; }
    }
}
