using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

namespace Maintain_it.Models
{
    public class ShutdownDateTime : IStorableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
