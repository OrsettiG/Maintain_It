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
	public partial class StartEndDatePicker : ViewCell
	{
		public StartEndDatePicker ()
		{
			InitializeComponent ();
		}
	}
}