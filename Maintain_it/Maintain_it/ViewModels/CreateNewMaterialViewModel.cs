using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Essentials;
using Xamarin.Forms;

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public class CreateNewMaterialViewModel : BaseViewModel
    {
        #region Properties
        private string materialName;
        public string MaterialName
        {
            get => materialName;
            set => SetProperty( ref materialName, value, validateValue: ValidateString );
        }

        private int quantityOwned;
        public int QuantityOwned
        {
            get => quantityOwned;
            set => SetProperty( ref quantityOwned, value <= maxValue ? value : QuantityOwned );
        }

        private int preferredRetailerId;
        public int PreferredRetailerId
        {
            get => preferredRetailerId;
            set => SetProperty( ref preferredRetailerId, value );
        }

        private PreferredRetailer preferredRetailer;
        public PreferredRetailer PreferredRetailer
        {
            get => preferredRetailer;
            set => SetProperty( ref preferredRetailer, value );
        }

        private ObservableRangeCollection<TagViewModel> allTags;
        public ObservableRangeCollection<TagViewModel> AllTags
        {
            get => allTags ??= new ObservableRangeCollection<TagViewModel>();
            set
            {
                SetProperty( ref allTags, value );

                AvailableTags.Clear();
                AvailableTags.AddRange( new ObservableRangeCollection<TagViewModel>( AllTags.OrderByDescending( x => x.Selected ).ThenBy( x => x.Name ) ) );

                SetProperty( ref selectedTags, value.Where( x => x.Selected ) as ObservableRangeCollection<TagViewModel> );
            }
        }

        private ObservableRangeCollection<TagViewModel> selectedTags;
        public ObservableRangeCollection<TagViewModel> SelectedTags
        {
            get => selectedTags ??= new ObservableRangeCollection<TagViewModel>();
            private set => SetProperty( ref selectedTags, value );
        }

        private ObservableRangeCollection<TagViewModel> availableTags;
        public ObservableRangeCollection<TagViewModel> AvailableTags
        {
            get => availableTags ??= new ObservableRangeCollection<TagViewModel>();
            private set => SetProperty( ref availableTags, value );
        }

        private string tagSearch;
        public string TagSearch
        {
            get => tagSearch;
            set
            {
                if( SetProperty( ref tagSearch, value, validateValue: ValidateString ) )
                {
                    if( !string.IsNullOrEmpty( value ) )
                    {
                        AvailableTags.Clear();
                        AvailableTags.AddRange( AllTags.Where( x => FilterTags( x ) ) );
                    }
                    else
                    {
                        AvailableTags.Clear();
                        AvailableTags.AddRange( AllTags.OrderByDescending( x => x.Selected ) );
                    }
                }
            }
        }

        private int lifeExpectency;
        public int LifeExpectency
        {
            get => lifeExpectency;
            set => SetProperty( ref lifeExpectency, value );
        }

        private int lifeExpectencyTimeframe;
        public Timeframe LifeExpectencyTimeframe
        {
            get => (Timeframe)lifeExpectencyTimeframe;
            set => SetProperty( ref lifeExpectencyTimeframe, (int)value );
        }

        private byte[] imageData;
        private Bitmap imageBitmap;
        public Bitmap ImageBitmap
        {
            get => imageBitmap;
            private set => SetProperty( ref imageBitmap, value );
        }

        private ImageSource image;
        public ImageSource Image
        {
            get => image ??= imageData != null ? ImageSource.FromStream( () => new MemoryStream( imageData ) ) : default;
            set => SetProperty( ref image, value );
        }

        private string specialIdentifier;
        public string SpecialIdentifier
        {
            get => specialIdentifier;
            set => SetProperty( ref specialIdentifier, value );
        }

#nullable enable
        private string? materialDescription;
        public string? MaterialDescription { get => materialDescription; set => SetProperty( ref materialDescription, value ); }

        private string? materialUnits;
        public string? MaterialUnits { get => materialUnits; set => SetProperty( ref materialUnits, value ); }

        private double? size;
        public double? Size { get => size; set => SetProperty( ref size, value ); }
#nullable disable

        private DateTime createdOn = DateTime.UtcNow;

        private Material material;

        #region View Controllers
        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty( ref isVisible, value );
        }
        #endregion

        #region Query Parameters
#nullable enable
        private int? editMaterialId = null;
#nullable disable
        #endregion

        // Not in use
        // Use to allow users to pick alternative looseMaterials
        private ObservableRangeCollection<Material> materials;
        public ObservableRangeCollection<Material> Materials { get => materials ??= new ObservableRangeCollection<Material>(); set => SetProperty( ref materials, value ); }

        // Not in use
        // Use for providing a dropdown list of available allTags from all the allTags the user has added in the past.
        private HashSet<Tag> existingTags;
        #endregion

        #region Commands

        private AsyncCommand saveMaterialCommand;
        public ICommand SaveMaterialCommand => saveMaterialCommand ??= new AsyncCommand( SaveMaterial );

        private Command incrementCommand;
        public ICommand IncrementCommand => incrementCommand ??= new Command( Increment );

        private Command decrementCommand;
        private readonly int maxValue = 10000000;

        public ICommand DecrementCommand => decrementCommand ??= new Command( Decrement );

        private AsyncCommand selectTagsCommand;
        public ICommand SelectTagsCommand
        {
            get => selectTagsCommand ??= new AsyncCommand( SelectTags );
        }
        private async Task SelectTags()
        {
            IsVisible = true;
            List<Tag> tags = await DbServiceLocator.GetAllItemsAsync<Tag>() as List<Tag>;

            foreach( Tag tag in tags )
            {
                if( !AllTags.Select( x => x.Name ).Contains( tag.Name ) )
                {
                    TagViewModel vm = new TagViewModel( tag );
                    vm.SelectionChanged += RefreshTagSelections;
                    AllTags.Add( vm );
                }
            }

            AvailableTags.Clear();

            ObservableRangeCollection<TagViewModel> i = new ObservableRangeCollection<TagViewModel>(AllTags.OrderByDescending( x => x.Selected ).ThenBy(x => x.Name));

            AvailableTags.AddRange( i );
        }

        private AsyncCommand addSelectedTagsCommand;
        public ICommand AddSelectedTagsCommand
        {
            get => addSelectedTagsCommand ??= new AsyncCommand( AddTags );
        }
        private async Task AddTags()
        {
            IsVisible = false;
            TagSearch = string.Empty;
            SelectedTags.Clear();
            SelectedTags.AddRange( AllTags.Where( x => x.Selected ) );
        }

        private AsyncCommand createNewTagCommand;
        public ICommand CreateNewTagCommand
        {
            get => createNewTagCommand ??= new AsyncCommand( CreateNewTag );
        }
        private async Task CreateNewTag()
        {
            if( !string.IsNullOrEmpty( TagSearch ) )
            {
                try
                {
                    TagViewModel newTagVM = new TagViewModel( await TagManager.GetNewTag( TagSearch ) )
                    {
                        Selected = true
                    };

                    AllTags.Add( newTagVM );

                }
                catch( Exception ex )
                {
                    switch( ex.Message )
                    {
                        case "UNIQUE constraint failed: Tag.Name":
                            await Shell.Current.DisplayAlert( Alerts.Error, Alerts.UniqueTagNameErrorMessage, Alerts.Confirmation );
                            break;
                    }

                    Console.WriteLine( $"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Insert Failed Due to Unique Constraint: {ex.Message} !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
                }
            }

            TagSearch = string.Empty;
        }

        private AsyncCommand takePhotoCommand;
        public ICommand TakePhotoCommand
        {
            get => takePhotoCommand ??= new AsyncCommand( TakePhoto );
        }
        private async Task TakePhoto()
        {
            if( !MediaPicker.IsCaptureSupported )
            {
                await Shell.Current.DisplayAlert( Alerts.Error, Alerts.CameraErrorMessage, Alerts.Confirmation );
                return;
            }
            if( Image != null && Image != default )
            {
                bool intent = await Shell.Current.DisplayAlert(Alerts.ReplaceImageTitle, Alerts.ReplaceImageMessage, Alerts.Yes, Alerts.Cancel);

                switch( intent )
                {
                    case true:
                        break;
                    case false:
                        return;
                }
            }

            FileResult photo = await Xamarin.Essentials.MediaPicker.CapturePhotoAsync();

            if( photo == null || photo == default )
            {
                return;
            }

            using( Stream stream = await photo.OpenReadAsync() )
            using( MemoryStream mStream = new MemoryStream() )
            {
                stream.CopyTo( mStream );
                imageBitmap = new Bitmap( stream );
                imageData = mStream.ToArray();
                Image = ImageSource.FromStream( () => new MemoryStream( imageData ) );
            }
        }
        #endregion

        #region Methods

        private async Task SaveMaterial()
        {
            if( editMaterialId != null )
            {
                await UpdateMaterial();
            }
            else
            {
                await AddMaterial();
            }
        }

        private async Task AddMaterial()
        {
            material = new Material()
            {
                Name = materialName,
                Size = size,
                Description = materialDescription,
                Units = materialUnits,
                CreatedOn = createdOn,
                QuantityOwned = QuantityOwned,
                StepMaterials = new List<StepMaterial>(),
                RetailerMaterials = new List<RetailerMaterial>(),
                ShoppingListMaterials = new List<ShoppingListMaterial>()
            };

            int id = await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( material );

            string encodedId = HttpUtility.UrlEncode( id.ToString() );
            await Shell.Current.GoToAsync( $"..?{QueryParameters.MaterialID}={encodedId}" ); // This goes to AddStepMaterialViewModel OR AddShoppingListMaterialViewModel
        }

        private async Task UpdateMaterial()
        {
            material.Name = materialName;
            material.Size = size;
            material.Description = materialDescription;
            material.Units = materialUnits;
            material.QuantityOwned = QuantityOwned;
            material.ImageBytes = imageData;
            material.PartNumber = SpecialIdentifier;
            material.LifeExpectancy = LifeExpectency;
            material.LifeExpectancyTimeframe = (int)LifeExpectencyTimeframe;

            List<Tag> tags = new List<Tag>( SelectedTags.Select( x => x.Tag ));
            material.Tags = tags;

            _ = await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( material );
            await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" ); // This goes to AddStepMaterialViewModel
        }

        private void Increment()
        {
            QuantityOwned = QuantityOwned < maxValue ? QuantityOwned + 1 : QuantityOwned;
        }

        private void Decrement()
        {
            QuantityOwned = QuantityOwned > 0 ? QuantityOwned - 1 : QuantityOwned;
        }

        private async Task AsyncInit( int id )
        {
            Console.WriteLine( $"AsycnInit Material id: {id} and editMaterialId of: {editMaterialId}" );

            material = await DbServiceLocator.GetItemAsync<Material>( id );

            MaterialName = material.Name;
            MaterialDescription = material.Description;
            MaterialUnits = material.Units;
            Size = material.Size;
            QuantityOwned = material.QuantityOwned;

            editMaterialId = id;
        }

        private bool FilterTags( TagViewModel vm )
        {
            return !string.IsNullOrEmpty( vm.Name ) && vm.Name.ToLowerInvariant().StartsWith( TagSearch.ToLowerInvariant() );
        }

        private void RefreshTagSelections()
        {
            SelectedTags = new ObservableRangeCollection<TagViewModel>( AllTags.Where( x => x.Selected ).OrderBy( x => x.Name ) );

            //AvailableTags = new ObservableRangeCollection<TagViewModel>( AllTags.OrderByDescending( x => x.Selected ).ThenBy( x => x.Name ) );
        }

        #endregion

        #region Query Handling

        public async override void ApplyQueryAttributes( IDictionary<string, string> query )
        {
            _ = Parallel.ForEach( query, kvp => EvaluateQueryParams( kvp.Key, kvp.Value ) );
        }

        private protected override async void EvaluateQueryParams( string key, string value )
        {
            switch( key )
            {
                case nameof( materialName ):
                    MaterialName = HttpUtility.UrlDecode( value );
                    break;
                case nameof( editMaterialId ):
                    if( int.TryParse( HttpUtility.UrlDecode( value ), out int _materialId ) )
                    {
                        editMaterialId = _materialId;
                        await AsyncInit( _materialId );
                    }
                    break;
            }
        }

        #endregion
    }
}
