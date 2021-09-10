using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace Maintain_it.Models
{
    public class TestItemModel
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string Name { get; set; }
    }
}
