using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it.Views.Custom_ViewCells
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CustomSwitchViewCell : ViewCell
	{
		public CustomSwitchViewCell ()
		{
			InitializeComponent ();
		}

        public static readonly BindableProperty TextProperty =
         BindableProperty.Create("Text", typeof(string), typeof(ViewCell), null,
             defaultBindingMode: BindingMode.OneWay);

        public string Text
        {
            get => GetValue( TextProperty ) as string;
            set => SetValue( TextProperty, value );
        }

        public static readonly BindableProperty TextColorProperty =
         BindableProperty.Create("TextColor", typeof(Color), typeof(ViewCell), (Color)App.Current.Resources["Secondary"],
             defaultBindingMode: BindingMode.OneWay);

        public Color TextColor
        {
            get => (Color)GetValue( TextColorProperty );
            set => SetValue( TextColorProperty, value );
        }

        public static readonly BindableProperty IsToggledProperty =
         BindableProperty.Create("IsToggled", typeof(bool), typeof(ViewCell), false,
             defaultBindingMode: BindingMode.TwoWay);

        public bool IsToggled
        {
            get => (bool)GetValue( IsToggledProperty );
            set => SetValue( IsToggledProperty, value );
        }
    }
}