using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using MvvmHelpers;

using SQLite;

using SQLiteNetExtensions.Attributes;


namespace Maintain_it.Models
{
    public class MaintenanceItem : IStorableObject
    {
        public MaintenanceItem(){}

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Properties
#nullable enable
        [NotNull]
        public string? Name { get; set; }
#nullable disable
        public bool IsActive { get; set; }
        public string Comment { get; set; }
        public DateTime FirstServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public int RecursEvery { get; set; }
        public int ServiceTimeframe { get; set; }
        public bool HasServiceLimit { get; set; }
        public int TimesToRepeatService { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }
        public int AdvanceNotice { get; set; }
        public int NoticeTimeframe { get; set; }
        public int NotificationEventArgsId { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion

        #region One to Many

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<Step> Steps { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public List<ServiceRecord> ServiceRecords { get; set; }

        #endregion
    }
}
