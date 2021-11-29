using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public class CreateNewMaterialViewModel : BaseViewModel
    {
        #region Properties
        private string materialName;
        public string MaterialName { get => materialName; set => SetProperty( ref materialName, value, validateValue: ValidateString ); }

        private int quantityOwned;
        public int QuantityOwned { get => quantityOwned; set => SetProperty( ref quantityOwned, value ); }

#nullable enable
        private string? materialDescription;
        public string? MaterialDescription { get => materialDescription; set => SetProperty( ref materialDescription, value ); }

        private string? materialTag;
        public string? MaterialTag { get => materialTag; set => SetProperty( ref materialTag, value ); }

        private string? materialUnits;
        public string? MaterialUnits { get => materialUnits; set => SetProperty( ref materialUnits, value ); }

        private double? size;
        public double? Size { get => size; set => SetProperty( ref size, value ); }
#nullable disable

        private DateTime createdOn = DateTime.Now;

        private Material material;

        #region Query Parameters
#nullable enable
        private int? editMaterialId = null;
#nullable disable
        #endregion

        // Not in use
        // Use to allow users to pick alternative materials
        private ObservableRangeCollection<Material> materials;
        public ObservableRangeCollection<Material> Materials { get => materials ??= new ObservableRangeCollection<Material>(); set => SetProperty( ref materials, value ); }

        // Not in use
        // Use for providing a dropdown list of available tags from all the tags the user has added in the past.
        private HashSet<string> tags;
        #endregion

        #region Commands

        private AsyncCommand saveMaterialCommand;
        public ICommand SaveMaterialCommand => saveMaterialCommand ??= new AsyncCommand( SaveMaterial );

        private Command incrementCommand;
        public ICommand IncrementCommand => incrementCommand ??= new Command( Increment );

        private Command decrementCommand;
        public ICommand DecrementCommand => decrementCommand ??= new Command( Decrement );


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
                Console.WriteLine( "Adding New Material" );
                await AddMaterial();
            }
        }

        private async Task AddMaterial()
        {
            if( material != null )
            {
                Console.WriteLine( $"Adding New Material: {material.Name}" );
            }

            material = new Material()
            {
                Name = materialName,
                Size = size,
                Description = materialDescription,
                Tag = materialTag,
                Units = materialUnits,
                CreatedOn = createdOn,
                StepMaterials = new List<StepMaterial>(),
                RetailerMaterials = new List<RetailerMaterial>(),
                ShoppingListMaterials = new List<ShoppingListMaterial>()
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync( material );

            string encodedId = HttpUtility.UrlEncode( id.ToString() );
            Console.WriteLine( "Sending Page back to AddStepMaterialView" );
            await Shell.Current.GoToAsync( $"..?addMaterialId={encodedId}" ); // This goes to AddStepMaterialViewModel
        }

        private async Task UpdateMaterial()
        {
            Console.WriteLine( $"Updating Material with id: {material.Id}" );

            material.Name = materialName;
            material.Size = size;
            material.Description = materialDescription;
            material.Tag = materialTag;
            material.Units = materialUnits;

            await DbServiceLocator.UpdateItemAsync( material );

            await Shell.Current.GoToAsync( $"..?refresh=true" ); // This goes to AddStepMaterialViewModel
        }

        private void Increment()
        {
            QuantityOwned++;
        }

        private void Decrement()
        {
            QuantityOwned--;
        }

        private async Task AsyncInit( int id )
        {
            Console.WriteLine( $"AsycnInit Material id: {id} and editMaterialId of: {editMaterialId}" );

            material = await DbServiceLocator.GetItemAsync<Material>( id );

            MaterialName = material.Name;
            MaterialDescription = material.Description;
            MaterialTag = material.Tag;
            MaterialUnits = material.Units;
            Size = material.Size;
            QuantityOwned = material.QuantityOwned;

            editMaterialId = id;
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
