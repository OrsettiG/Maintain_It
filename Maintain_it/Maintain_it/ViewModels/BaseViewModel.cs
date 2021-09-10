using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public abstract class BaseViewModel : BindableObject
    {
        public INavigation Navigation { get; set; }

    }
}
