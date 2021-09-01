using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace Maintain_it.Models
{
    public enum Timeframe { DAYS, WEEKS, MONTHS, YEARS }

    public class MaintenanceItem
    {
        public MaintenanceItem() { }

        public MaintenanceItem( string Name, DateTime FirstServiceDate )
        {
            this.Name = Name;
            this.FirstServiceDate = FirstServiceDate;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Material> MaterialsAndEquipment { get; set; }
        public List<Step> Process { get; set; }
        public List<string> Notes { get; set; }
        public DateTime FirstServiceDate { get; set; }
        public DateTime LastServiceDate { get; set; }
        public DateTime NextServiceDate { get; set; }
        public bool Repeats { get; set; }
        public int Frequency { get; set; }
        public Timeframe Timeframe { get; set; }
        public bool Recurring { get; set; }
        public int Times { get; set; }
        public bool IsComplete { get; set; }
        public bool NotifyOfNextServiceDate { get; set; }

    }
}
