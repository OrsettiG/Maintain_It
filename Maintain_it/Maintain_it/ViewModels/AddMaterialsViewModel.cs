using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Essentials;
using Xamarin.Forms;

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public abstract class AddMaterialsViewModel : BaseViewModel
    {
        #region Properties
        private protected bool locked = false;
        private protected List<Material> _materials = new List<Material>();

        private string materialNameSearch;
        public string MaterialNameSearch
        {
            get => materialNameSearch;
            set
            {
                if( SetProperty( ref materialNameSearch, value, validateValue: ValidateString ) )
                {
                    if( value != string.Empty || value != null )
                    {
                        DisplayedMaterials.Clear();
                        DisplayedMaterials.AddRange( _materials.Where( x => FilterMaterials( x ) ) );
                    }
                    else
                    {
                        DisplayedMaterials.Clear();
                        DisplayedMaterials.AddRange( _materials );
                    }

                }
            }
        }

        private ObservableRangeCollection<Material> _displayedMaterials;
        public ObservableRangeCollection<Material> DisplayedMaterials
        {
            get => _displayedMaterials ??= new ObservableRangeCollection<Material>();
            set => SetProperty( ref _displayedMaterials, value );
        }
        #endregion

        #region Commands

        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );
        private protected abstract Task Refresh();

        private AsyncCommand createNewMaterialCommand;
        public ICommand CreateNewMaterialCommand => createNewMaterialCommand ??= new AsyncCommand( CreateNewMaterial );
        
        /// <summary>
        /// Sends the view to the CreateNewMaterialView after encoding and passing the search term to the material name field using the query parameter "materialName"
        /// </summary>
        /// <returns></returns>
        private protected virtual async Task CreateNewMaterial()
        {
            string encodedName = HttpUtility.UrlEncode( MaterialNameSearch );
            
            await MainThread.InvokeOnMainThreadAsync( async () =>
            {
                await Shell.Current.GoToAsync( $"/{nameof( CreateNewMaterialView )}?materialName={encodedName}" );

            } );
        }

        private Command materialSelectionChangedCommand;
        public ICommand MaterialSelectionChangedCommand => materialSelectionChangedCommand ??= new Command( MaterialSelectionChanged );
        private protected abstract void MaterialSelectionChanged( object obj );

        #endregion

        #region Methods

        /// <summary>
        /// Converts the search term and the material name/tag to lower case and returns true if the name or tag starts with the search term
        /// </summary>
        /// <param name="material">The material to check</param>
        /// <returns>True if search term matches the start of the name or tag, false otherwise.</returns>
        private protected bool FilterMaterials( Material material )
        {
            return ( material.Name != null && material.Name.ToLowerInvariant().StartsWith( MaterialNameSearch.ToLowerInvariant() ) ) ||
                   ( material.Tag != null && material.Tag.ToLowerInvariant().StartsWith( MaterialNameSearch.ToLowerInvariant() ) );
        }

        #region Query Handling

        private protected async override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

        #endregion
        #endregion

    }
}