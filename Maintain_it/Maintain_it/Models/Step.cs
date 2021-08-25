using System.Collections.Generic;

namespace Maintain_it.Models
{
    public class Step
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Photo> Photos { get; set; }
        public bool IsCompleted { get; set; }
        public float CompletionTime { get; set; }
    }
}