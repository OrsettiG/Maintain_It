using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialDetailViewModel: BaseViewModel
    {
        #region Properties
        private ShoppingListMaterial shoppingListMaterial;

        private string name;
        public string Name 
        { 
            get => name; 
            set => SetProperty( ref name, value ); 
        }

        #endregion
        #region Commands
        #endregion
        #region Methods
        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case RoutingPath.ShoppingListMaterialId:
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out int id ) )
                    {
                        shoppingListMaterial = await DbServiceLocator.GetItemRecursiveAsync<ShoppingListMaterial>( id );
                        Name = shoppingListMaterial.Name;
                    }
                    break;
            }
        }
        #endregion
        #endregion
    }
}
