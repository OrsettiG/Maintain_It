using System;
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

        private HashSet<int> selectedMaterialIds;
        public HashSet<int> SelectedMaterialIds { get => selectedMaterialIds ??= new HashSet<int>( 100 ); set => SetProperty( ref selectedMaterialIds, value ); }

        private ObservableRangeCollection<StepMaterialViewModel> selectedMaterials;
        public ObservableRangeCollection<StepMaterialViewModel> SelectedMaterials { get => selectedMaterials ??= new ObservableRangeCollection<StepMaterialViewModel>(); set => SetProperty( ref selectedMaterials, value ); }

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

        private async Task Refresh()
        {
            Console.WriteLine( "Refreshed" );
            if( !locked )
            {
                locked = true;

                List<Material> mats = await DbServiceLocator.GetAllItemsAsync<Material>().ConfigureAwait(false) as List<Material>;
                _materials.Clear();
                DisplayedMaterials.Clear();

                _materials.AddRange( mats );
                DisplayedMaterials.AddRange( mats );

                locked = false;
            }
        }

        internal void RemoveStepMaterialFromSelectedMaterials( int id )
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

        private bool FilterMaterials( Material material )
        {
            return ( material.Name != null && material.Name.StartsWith( MaterialNameSearch ) ) ||
                   ( material.Tag != null && material.Tag.StartsWith( MaterialNameSearch ) );
        }

        private async Task AddSelectedStepMaterials()
        {
            throw new NotImplementedException();
        }

        private async Task AddNewStepMaterials()
        {
            string ids = string.Join( ',', SelectedMaterialIds );
            string encodedIds = HttpUtility.UrlEncode( ids );
            await Shell.Current.GoToAsync( $"..?stepMaterialIds={encodedIds}" ); // This goes to StepViewModel
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

        private async void MaterialSelectionChanged( object obj )
        {
            if( DisplayedMaterialSelections.Count >= SelectedMaterialIds.Count )
            {
                foreach( Material material in DisplayedMaterialSelections.ToList() )
                {
                    if( SelectedMaterialIds.Add( material.Id ) )
                    {
                        int stepMaterialId = await CreateNewStepMaterial( material ).ConfigureAwait( false );
                        _stepMaterials.Add( await DbServiceLocator.GetItemAsync<StepMaterial>( stepMaterialId ).ConfigureAwait( false ) );
                    }
                }
            }
            else if( SelectedMaterialIds.Count > DisplayedMaterialSelections.Count )
            {
                List<int> temp = new List<int>();

                foreach( Material mat in DisplayedMaterialSelections.ToList() )
                {
                    temp.Add( mat.Id );
                }

                IEnumerable<int> diff = SelectedMaterialIds.Except(temp);

                foreach( int id in diff.ToList() )
                {
                    RemoveStepMaterialFromSelectedMaterials( id );
                }
            }
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

                    string decodedValue = HttpUtility.UrlDecode(kvp.Value);

                    if( int.TryParse( decodedValue, out addMaterialId ) )
                    {
                        await Refresh();
                        if( SelectedMaterialIds.Add( addMaterialId ) )
                        {
                            SelectedMaterials.Add( new StepMaterialViewModel( _materials.Where( x => x.Id == addMaterialId ).FirstOrDefault() )
                            {
                                Quantity = 1
                            } );
                        }
                    }

                    break;

                // Used when coming from StepViewModel when some StepMaterials have already been added to the step, such as when a user wants to edit the materials used in a step they have already created.
                case nameof( preselectedStepMaterialIds ):

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
                await stepMatVM.AsyncInit( id );
                SelectedMaterials.Add( stepMatVM );
            }
        }

        #endregion

        #endregion
    }
}
