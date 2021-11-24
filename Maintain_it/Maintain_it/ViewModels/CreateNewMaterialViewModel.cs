using System;
using System.Collections.Generic;
using System.Text;
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

        // Not in use
        // Use to allow users to pick alternative materials
        private ObservableRangeCollection<Material> materials;
        public ObservableRangeCollection<Material> Materials { get => materials ??= new ObservableRangeCollection<Material>(); set => SetProperty( ref materials, value ); }

        // Not in use
        // Use for providing a dropdown list of available tags from all the tags the user has added in the past.
        private HashSet<string> tags;
        #endregion

        #region Commands

        private AsyncCommand addMaterialCommand;
        public ICommand AddMaterialCommand => addMaterialCommand ??= new AsyncCommand( AddMaterial );

        private Command incrementCommand;
        public ICommand IncrementCommand => incrementCommand ??= new Command( Increment );

        private Command decrementCommand;
        public ICommand DecrementCommand => decrementCommand ??= new Command( Decrement );

        #endregion

        #region Methods

        private async Task AddMaterial()
        {
            material = new Material()
            {
                Name = materialName,
                Size = size,
                Description = materialDescription,
                Tag = materialTag,
                CreatedOn = createdOn,
                StepMaterials = new List<StepMaterial>(),
                RetailerMaterials = new List<RetailerMaterial>(),
                ShoppingListMaterials = new List<ShoppingListMaterial>()
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync( material );

            string encodedId = HttpUtility.UrlEncode( id.ToString() );
            await Shell.Current.GoToAsync( $"..?addMaterialId={encodedId}" ); // This goes to AddStepMaterialViewModel
        }

        private void Increment()
        {
            QuantityOwned++;
        }

        private void Decrement()
        {
            QuantityOwned--;
        }

        #endregion

        #region Query Handling

        public override void ApplyQueryAttributes( IDictionary<string, string> query )
        {
            foreach( KeyValuePair<string, string> kvp in query )
            {
                EvaluateQueryParams( kvp.Key, kvp.Value );
            }
        }

        private protected override void EvaluateQueryParams( string key, string value )
        {
            switch( key )
            {
                case nameof( materialName ):
                    MaterialName = value;
                    break;
            }
        }

        #endregion
    }
}
