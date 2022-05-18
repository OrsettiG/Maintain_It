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
        public DateTime CreatedOn { get; set; }
        public string Comment { get; set; }
        public DateTime FirstServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public int RecursEvery { get; set; }
        public int Timeframe { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }
        #endregion

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<Step> Steps { get; set; }


        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public List<ServiceRecord> ServiceRecords { get; set; }
    }
}
