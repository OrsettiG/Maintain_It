using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.ViewModels
{
    public class MaintenanceItemViewModel : BaseViewModel
    {
        public MaintenanceItemViewModel( MaintenanceItem item ) => Item = item;

        public MaintenanceItem Item { get; private set; }
        public event EventHandler ItemStatusChanged;
    }
}
