﻿using System;
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

        public MaintenanceItem( string Name )
        {
            this.Name = Name;
            FirstServiceDate = DateTime.Now;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Properties
        [NotNull]
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public string Comment { get; set; }
        public DateTime FirstServiceDate { get; set; }
        public DateTime PreviousServiceDate { get; set; }
        public DateTime NextServiceDate { get; set; }
        public bool IsRecurring { get; set; }
        public int RecursEvery { get; set; }
        public Timeframe Frequency { get; set; }
        public int TimesServiced { get; set; }
        public bool PreviousServiceCompleted { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }
        #endregion

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<Step> Steps { get; set; }
    }
}
