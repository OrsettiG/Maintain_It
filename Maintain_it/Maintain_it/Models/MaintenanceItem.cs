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


        #region Properties
        #region Uncontained Data
        // IStorableObject
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
#nullable enable
        [NotNull]
        public string? Name { get; set; }
#nullable disable
        public DateTime CreatedOn { get; set; }

        // Service Schedule
        public DateTime NextServiceDate { get; set; }
        public int RecursEvery { get; set; }
        public int ServiceTimeframe { get; set; }
        public bool HasServiceLimit { get; set; }
        public int TimesToRepeatService { get; set; }

        // Notifications
        public int ActiveState { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }
        public int AdvanceNotice { get; set; }
        public int NoticeTimeframe { get; set; }
        public int NotificationEventArgsId { get; set; }
        
        // Information
        public string Comment { get; set; }
        
        // Life Expectancy
        public int LifeExpectancy { get; set; }
        public int LifeExpectancyTimeframe { get; set; }
        public DateTime Birthday { get; set; }
        #endregion Uncontained Data

        #region Summary Data
        // Step Data Summary
        public double ServiceCompletionTimeEst { get; set; }
        public int ServiceCompletionTimeEstTimeframe { get; set; }
        
        // Service Record Summary
        public int TimesServiced { get; set; }
        public DateTime LastServiceDate { get; set; }
        #endregion Summary Data
        #endregion

        #region Contained Data
        // Steps
        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<Step> Steps { get; set; }

        // Service Records
        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public List<ServiceRecord> ServiceRecords { get; set; }
        #endregion Contained Data
    }
}
