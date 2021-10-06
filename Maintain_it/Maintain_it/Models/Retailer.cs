using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace Maintain_it.Models
{
    public class Retailer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
