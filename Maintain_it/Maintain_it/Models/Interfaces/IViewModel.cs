using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Maintain_it.Models.Interfaces
{
    public interface IViewModel
    {
        #region Commands
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }
        #endregion
    }
}
