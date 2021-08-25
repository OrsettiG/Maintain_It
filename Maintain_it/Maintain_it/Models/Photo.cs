using SQLite;

namespace Maintain_it.Models
{
    public class Photo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public byte[] Bytes { get; set; }
    }
}