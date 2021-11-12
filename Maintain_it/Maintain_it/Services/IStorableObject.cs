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
        string? Name { get; set; }
#nullable disable
        [NotNull]
        DateTime CreatedOn { get; set; }
    }
}
