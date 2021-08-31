using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maintain_it.Models
{
    public class Calendar
    {
        public Calendar( DateTime startDate )
        {
            // Loop Through DB and find all MaintenanceItems with due dates this month then add them to the dictionary using the due date as key and the item as value
            CalendarData = new Dictionary<DateTime, List<MaintenanceItem>>()
            {
                { new DateTime( startDate.Year, startDate.Month, 1 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 2 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 3 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 4 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 5 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 6 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
                { new DateTime( startDate.Year, startDate.Month, 7 ), new List<MaintenanceItem>(){ new MaintenanceItem() } },
            };
        }
        public Dictionary<DateTime, List<MaintenanceItem>> CalendarData { get; set; }
    }
}
