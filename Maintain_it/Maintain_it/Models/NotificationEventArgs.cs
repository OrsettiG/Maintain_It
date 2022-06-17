using System;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class NotificationEventArgs : EventArgs, IStorableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Properties
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime NotifyTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Active { get; set; }
        public int TimesCalled { get; set; }
        #endregion
    }
}
