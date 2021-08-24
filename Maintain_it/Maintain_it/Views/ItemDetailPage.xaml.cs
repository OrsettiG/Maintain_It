using System.ComponentModel;

using Maintain_it.ViewModels;

using Xamarin.Forms;

namespace Maintain_it.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}