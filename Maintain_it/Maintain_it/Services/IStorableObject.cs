using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace Maintain_it.Services
{
    public interface IStorableObject
    {
        [PrimaryKey, AutoIncrement]
        int Id { get; set; }
#nullable enable
        [Unique]
        string? Name { get; set; }
#nullable disable
    }
}
