using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it.Views
{
    [XamlCompilation( XamlCompilationOptions.Compile )]
    public partial class AddNewStepView : ContentPage
    {
        public AddNewStepView()
        {
            InitializeComponent();
            BindingContext = new StepViewModel();
        }
    }
}