using System;
using System.Collections.Generic;
using System.ComponentModel;

using Maintain_it.Models;
using Maintain_it.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}