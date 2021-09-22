using System.Collections.Generic;

using SQLite;

namespace Maintain_it.Models
{
    public class Step
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Photo> Photos { get; set; }
        public bool IsCompleted { get; set; }
        public float CompletionTime { get; set; }
    }
}