using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Photo : IStorableObject
    {
        public Photo() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Many To One Relationships
        [ForeignKey( typeof( Step ) )]
        public int StepId { get; set; }
        [ManyToOne]
        public Step Step { get; set; }
        #endregion

        #region Properties
        public byte[] Bytes { get; set; }
        public string Comment { get; set; }
        #endregion
    }
}