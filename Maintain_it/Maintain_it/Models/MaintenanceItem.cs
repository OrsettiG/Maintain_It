using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public enum Timeframe { DAYS, WEEKS, MONTHS, YEARS }

    public class MaintenanceItem
    {
        public MaintenanceItem() { }

        public MaintenanceItem( string Name )
        {
            this.Name = Name;
            FirstServiceDate = DateTime.Now;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Unique]
        public string Name { get; set; }
        
        [ManyToMany( typeof( ItemsToMaterials ) )]
        public List<Material> Materials { get; set; }
        
        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<Step> Process { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<string> Notes { get; set; }
        
        public DateTime FirstServiceDate { get; set; }
        public DateTime PreviousServiceDate { get; set; }
        public DateTime NextServiceDate { get; set; }
        public bool IsRecurring { get; set; }
        public int RecursEvery { get; set; }
        public Timeframe Timeframe { get; set; }
        public int TimesServiced { get; set; }
        public bool PreviousServiceCompleted { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }

    }
}
