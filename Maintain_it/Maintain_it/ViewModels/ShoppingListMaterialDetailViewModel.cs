using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialDetailViewModel : BaseViewModel
    {
        #region Properties
        private ShoppingListMaterial shoppingListMaterial;

        private int id;
        public int Id
        {
            get => id;
            set => SetProperty( ref id, value );
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set => SetProperty( ref quantity, value );
        }

        private int quantityOwned;
        public int QuantityOwned
        {
            get => quantityOwned;
            set => SetProperty( ref quantityOwned, value );
        }

        private bool purchased;
        public bool Purchased
        {
            get => purchased;
            set => SetProperty( ref purchased, value );
        }

        private DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetProperty( ref createdOn, value );
        }


        #endregion
        #region Commands
        private AsyncCommand saveCommand;
        public ICommand SaveCommand
        {
            get => saveCommand ??= new AsyncCommand( Save );
        }

        private async Task Save()
        {
            shoppingListMaterial.Name = Name;
            shoppingListMaterial.Quantity = Quantity;
            shoppingListMaterial.Purchased = Purchased;

            await DbServiceLocator.UpdateItemAsync( shoppingListMaterial );
            await Shell.Current.GoToAsync( $"..?{RoutingPath.Refresh}=true" );
        }

        private AsyncCommand backCommand;
        public ICommand BackCommand
        {
            get => backCommand ??= new AsyncCommand( Back );
        }

        private async Task Back()
        {
            await Shell.Current.GoToAsync( $"..?{RoutingPath.Refresh}=true" );
        }

        private AsyncCommand openMaterialCommand;
        public ICommand OpenMaterialCommand
        {
            get => openMaterialCommand ??= new AsyncCommand( OpenMaterial );
        }

        private async Task OpenMaterial()
        {
            string encodedId = HttpUtility.UrlEncode($"{shoppingListMaterial.MaterialId}");

            await Shell.Current.GoToAsync( $"{nameof( MaterialDetailView )}?{RoutingPath.MaterialID}={encodedId}" );
        }

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
                        Quantity = shoppingListMaterial.Quantity;
                        Purchased = shoppingListMaterial.Purchased;
                        CreatedOn = shoppingListMaterial.CreatedOn;
                        Id = shoppingListMaterial.Id;

                        Material mat = await DbServiceLocator.GetItemAsync<Material>(shoppingListMaterial.MaterialId);

                        QuantityOwned = mat.QuantityOwned;
                    }
                    break;
            }
        }
        #endregion
        #endregion
    }
}
