using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class ServiceRecord : IStorableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get;
            set;
        }

        #region Many to One

        [ForeignKey( typeof( MaintenanceItem ) )]
        public int MaintenanceItemId
        {
            get;
            set;
        }

        [ManyToOne]
        public MaintenanceItem Item
        {
            get;
            set;
        }

        #endregion

        #region PROPERTIES
#nullable enable
        public string? Name { get; set; }
#nullable disable

        public bool ServiceCompleted
        {
            get;
            set;
        }

        public bool ServiceStarted
        {
            get;
            set;
        }

        public int CurrentStepIndex
        {
            get;
            set;
        }

        public DateTime ActualServiceCompletionDate
        {
            get;
            set;
        }
        
        public DateTime TargetServiceCompletionDate
        {
            get;
            set;
        }

        [NotNull]
        public DateTime CreatedOn
        {
            get;
            set;
        }
        #endregion
    }
}
