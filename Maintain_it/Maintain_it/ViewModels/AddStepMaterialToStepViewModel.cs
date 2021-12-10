﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;


namespace Maintain_it.ViewModels
{
    internal class AddStepMaterialToStepViewModel : BaseViewModel
    {
        public AddStepMaterialToStepViewModel() { }

        #region Properties

        private bool locked = false;

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

        private List<StepMaterial> _stepMaterials = new List<StepMaterial>();

        private List<Material> _materials = new List<Material>();

        private ObservableRangeCollection<Material> _displayedMaterials;
        public ObservableRangeCollection<Material> DisplayedMaterials { get => _displayedMaterials ??= new ObservableRangeCollection<Material>(); set => SetProperty( ref _displayedMaterials, value ); }

        private HashSet<int> _selectedMaterialIds;
        public HashSet<int> SelectedMaterialIds { get => _selectedMaterialIds ??= new HashSet<int>( 100 ); set => SetProperty( ref _selectedMaterialIds, value ); }

        private ObservableRangeCollection<StepMaterialViewModel> _selectedMaterials;
        public ObservableRangeCollection<StepMaterialViewModel> SelectedMaterials { get => _selectedMaterials ??= new ObservableRangeCollection<StepMaterialViewModel>(); set => SetProperty( ref _selectedMaterials, value ); }

        private ObservableRangeCollection<object> _displayedMaterialSelections;
        public ObservableRangeCollection<object> DisplayedMaterialSelections { get => _displayedMaterialSelections ??= new ObservableRangeCollection<object>(); set => SetProperty( ref _displayedMaterialSelections, value ); }

        #region Query Parameters
        private int addMaterialId;
        private int[] preselectedStepMaterialIds;
        #endregion

        #endregion

        #region Commands

        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );

        private AsyncCommand createNewMaterialCommand;
        public ICommand CreateNewMaterialCommand => createNewMaterialCommand ??= new AsyncCommand( CreateNewMaterial );

        private Command materialSelectionChangedCommand;
        public ICommand MaterialSelectionChangedCommand => materialSelectionChangedCommand ??= new Command( MaterialSelectionChanged );

        private AsyncCommand addStepMaterialsCommand;
        public ICommand AddStepMaterialsCommand => addStepMaterialsCommand ??= new AsyncCommand( AddSelectedStepMaterials );

        #endregion

        #region Methods
        /// <summary>
        /// Refreshes all the data in the view asyncronously. Uses recursive database calls.
        /// </summary>
        /// <returns></returns>
        private async Task Refresh()
        {
            Console.WriteLine( "Refreshed" );
            if( !locked )
            {
                locked = true;

                // We need to call recursive so that the child data gets fully loaded. If we don't call recursive we end up with an 'collection empty' exception because the child collections don't get loaded in.
                List<Material> mats = await DbServiceLocator.GetAllItemsRecursiveAsync<Material>() as List<Material>;

                // Gotta clear these so we don't end up with duplicate values
                _materials.Clear();
                DisplayedMaterials.Clear();
                DisplayedMaterialSelections.Clear();

                // We can just straight up add all the materials to these collections because we want to allow the user to see every material they have added.
                _materials.AddRange( mats );
                DisplayedMaterials.AddRange( mats );

                // We need to make sure that our SelectedMaterials are properly initialized before we display them or else we can end up with odd errors.
                if( SelectedMaterials.Count > 0 )
                {
                    foreach( StepMaterialViewModel sMaterialVM in SelectedMaterials )
                    {
                        // I was getting an exception that didn't make any sense but when I wrapped it in a try/catch block and did nothing with the exception it worked. Gonna have to come back and figure out what is going on here at some point
                        try
                        {
                            await sMaterialVM.AsyncInit( sMaterialVM.StepMaterial.Id );
                        }
                        catch( Exception ex )
                        {
                            //TODO: Figure out what is causing this exception and fix it.
                            Console.WriteLine( ex.StackTrace );
                        }
                    }
                    // Add all our selected materials into our DisplayedMaterialSelections collection so that the user can easily see/interact with what they have selected.
                    DisplayedMaterialSelections.AddRange( DisplayedMaterials.Where( x => SelectedMaterialIds.Contains( x.Id ) ) );
                }

                locked = false;
            }
        }

        /// <summary>
        /// Called by Xamarin.Forms CollectionView when the user changes the selections in the DisplayedMaterials collection. We use it to either add a new StepMaterial or remove an unwanted one.
        /// </summary>
        /// <param name="obj">I think this contains all the selected objects from the the CollectionView. Not entirely sure becuase we don't use it anyways. It just has to be here to satisfy Xamarin.Forms.</param>
        private async void MaterialSelectionChanged( object obj )
        {
            if( !locked )
            {
                // Figure out if we are adding a selection or removing one by comparing the respective collection counts.
                if( DisplayedMaterialSelections.Count >= SelectedMaterialIds.Count )
                {
                    // If we are adding one we need to make sure that the backing data is all properly created, but we don't know which one is new, so we just update all of them.
                    foreach( Material material in DisplayedMaterialSelections.ToList() )
                    {
                        await UpdateStepMaterials( material ).ConfigureAwait( false );
                    }
                }
                else if( SelectedMaterialIds.Count > DisplayedMaterialSelections.Count )
                {
                    // If we are removing one we need to find out which one so we create a temporary list of material ids from the DisplayedMaterialSelections collection and use the IEnumerable.Except() method to get only the ids that exists in the SelectedMaterialIds HashSet and don't exists in the DisplayedMaterialSelections collection and remove them all.
                    List<int> temp = new List<int>();

                    foreach( Material mat in DisplayedMaterialSelections.ToList() )
                    {
                        temp.Add( mat.Id );
                    }

                    IEnumerable<int> diff = SelectedMaterialIds.Except(temp);

                    foreach( int id in diff.ToList() )
                    {
                        RemoveMaterialFromSelectedMaterials( id );
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to remove the supplied Id from the SelectedMaterialIds <see cref="HashSet{T}"/>. If it successfully removes the Id it ensures that all other references to that material are reset to the unselected state. Otherwise it just returns.
        /// </summary>
        /// <param name="id">The id of the material to remove.</param>
        internal void RemoveMaterialFromSelectedMaterials( int id )
        {
            if( SelectedMaterialIds.Remove( id ) )
            {
                foreach( StepMaterialViewModel smvm in SelectedMaterials.ToList() )
                {
                    if( smvm.MaterialId == id )
                    {
                        _ = SelectedMaterials.Remove( smvm );
                    }
                }

                foreach( Material mat in DisplayedMaterialSelections.ToList() )
                {
                    if( mat.Id == id )
                    {
                        _ = DisplayedMaterialSelections.Remove( mat );
                    }
                }
            }
        }

        /// <summary>
        /// Converts the search term and the material name/tag to lower case and returns true if the name or tag starts with the search term
        /// </summary>
        /// <param name="material">The material to check</param>
        /// <returns>True if search term matches the start of the name or tag, false otherwise.</returns>
        private bool FilterMaterials( Material material )
        {
            return ( material.Name != null && material.Name.ToLowerInvariant().StartsWith( MaterialNameSearch.ToLowerInvariant() ) ) ||
                   ( material.Tag != null && material.Tag.ToLowerInvariant().StartsWith( MaterialNameSearch.ToLowerInvariant() ) );
        }

        private async Task AddSelectedStepMaterials()
        {
            StringBuilder queryStringBuilder = new StringBuilder();
            int count = 0;
            foreach( StepMaterialViewModel smvm in SelectedMaterials )
            {
                if( SelectedMaterialIds.Contains( smvm.MaterialId ) )
                {
                    await smvm.Update();
                    _ = count < 1 ? queryStringBuilder.Append( $"{smvm.StepMaterial.Id}" ) : queryStringBuilder.Append( $",{smvm.StepMaterial.Id}" );

                    Console.WriteLine( $"Step Material {smvm.StepMaterial.Name} id = {smvm.StepMaterial.Id} | Material {smvm.StepMaterial.Material.Name} id = {smvm.StepMaterial.Material.Id} | Registered Material ID = {smvm.MaterialId}" );

                    count++;
                }
                else
                {
                    throw new Exception( "Something went wrong, you have SelectedMaterials that are not showing up in the SelectedMaterialIds HashSet" );
                }

            }

            string encodedQuery = HttpUtility.UrlEncode( queryStringBuilder.ToString() );

            await Shell.Current.GoToAsync( $"..?stepMaterialIds={encodedQuery}" );
        }

        private async Task AddNewStepMaterials()
        {
            string ids = string.Join( ',', SelectedMaterialIds );
            string encodedIds = HttpUtility.UrlEncode( ids );
            await Shell.Current.GoToAsync( $"..?stepMaterialIds={encodedIds}" ); // This goes to StepViewModel
        }

        private async Task UpdateStepMaterials( Material material )
        {
            if( SelectedMaterialIds.Add( material.Id ) )
            {
                int stepMaterialId = await CreateNewStepMaterial( material ).ConfigureAwait( false );
                _stepMaterials.Add( await DbServiceLocator.GetItemAsync<StepMaterial>( stepMaterialId ).ConfigureAwait( false ) );
            }
        }

        private async Task<int> CreateNewStepMaterial( Material mat )
        {
            StepMaterialViewModel viewModel = new StepMaterialViewModel( mat )
            {
                Quantity = 1,
                AddStepMaterialToStepViewModel = this
            };

            SelectedMaterials.Add( viewModel );

            int id = await DbServiceLocator.AddItemAndReturnIdAsync(viewModel.StepMaterial);
            return id;
        }

        private async Task CreateNewMaterial()
        {
            string encodedName = HttpUtility.UrlEncode( MaterialNameSearch );
            await Shell.Current.GoToAsync( $"/{nameof( CreateNewMaterialView )}?materialName={encodedName}" );
        }

        #region Query Handling

        private protected async override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                // Used when coming from CreateNewMaterialViewModel to add the Material the user just created and save them having to find it in the list of Materials.
                case nameof( addMaterialId ):
                    MaterialNameSearch = string.Empty;
                    await Refresh();

                    string decodedValue = HttpUtility.UrlDecode(kvp.Value);

                    if( int.TryParse( decodedValue, out addMaterialId ) )
                    {
                        await UpdateStepMaterials( _materials.Where( x => x.Id == addMaterialId ).SingleOrDefault() ).ConfigureAwait( false );
                    }

                    await Refresh();
                    break;

                // Used when coming from StepViewModel when some StepMaterials have already been added to the step, such as when a user wants to edit the materials used in a step they have already created.
                case nameof( preselectedStepMaterialIds ):

                    await Refresh();

                    string decodedIds = HttpUtility.UrlDecode(kvp.Value);
                    string[] ids = decodedIds.Split(',');

                    preselectedStepMaterialIds = new int[ids.Length];

                    for( int i = 0; i < ids.Length; i++ )
                    {
                        if( int.TryParse( ids[i], out int n ) )
                        {
                            preselectedStepMaterialIds[i] = n;
                        }
                    }

                    await PopulatePreselectedStepMaterials();
                    break;

                case "refresh":
                    await Refresh();
                    break;
            }
        }

        /// <summary>
        /// Used when the page recieves query parameters with the key 'preselectedStepMaterialIds' to prepopulate the <see cref="ObservableRangeCollection{}"/> <see href="SelectedMaterials"/>  correctly.
        /// </summary>
        private async Task PopulatePreselectedStepMaterials()
        {
            foreach( int id in preselectedStepMaterialIds )
            {
                StepMaterialViewModel stepMatVM = new StepMaterialViewModel();
                await stepMatVM.AsyncInit( id, this );

                if( SelectedMaterialIds.Add( stepMatVM.MaterialId ) )
                {
                    SelectedMaterials.Add( stepMatVM );
                }
            }

            await Refresh();
        }

        #endregion

        #endregion
    }
}
