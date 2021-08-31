using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.ViewModels
{
    internal class HomeViewModel
    {
        public HomeViewModel()
        {
            _calendar = new Calendar( DateTime.Now );

        }

        private Calendar _calendar { get; set; }
        private ObservableCollection<MaintenanceItem> _upcomingMaintenance { get; set; }

        public Calendar Calendar => _calendar;
        public ObservableCollection<MaintenanceItem> UpcomingMaintenance => _upcomingMaintenance;


    }
}
