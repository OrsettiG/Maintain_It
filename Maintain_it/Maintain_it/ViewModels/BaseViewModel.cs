using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Maintain_it.Services;

using MvvmHelpers;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        public INavigation Navigation { get; set; }
    }
}
