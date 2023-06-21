using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers.Commands;
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;


namespace Maintain_it.ViewModels
{
    internal class StepMaterialViewModel : BaseViewModel
    {
        public StepMaterialViewModel()
        {

        }

        public StepMaterialViewModel( StepMaterial stepMaterial )
        {
            StepMaterial = stepMaterial;
            MaterialId = stepMaterial.MaterialId;
            Quantity = stepMaterial.Quantity;
        }

        public StepMaterialViewModel( Material material )
        {
            stepMaterial = new StepMaterial()
            {
                Name = material.Name,
                CreatedOn = DateTime.UtcNow,
                MaterialId = material.Id,
                Material = material
            };

            MaterialId = material.Id;
        }

        #region PROPERTIES
        private const int maxQuantity = 1000001;

        public AddStepMaterialToStepViewModel AddStepMaterialToStepViewModel { get; set; }

        private StepMaterial stepMaterial;
        public StepMaterial StepMaterial { get => stepMaterial; set => SetProperty( ref stepMaterial, value ); }

        public int MaterialId { get; private set; }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if( value > 0 && value < maxQuantity )
                {
                    _ = SetProperty( ref quantity, value );
                }
                else
                {
                    //TODO: Inform user of minimum and maximum quantity limitations.
                }
            }
        }

        #endregion

        #region COMMANDS

        private Command decrementQuantityCommand;
        public ICommand DecrementQuantityCommand => decrementQuantityCommand ??= new Command( DecrementQuantity );

        private Command incrementQuantityCommand;
        public ICommand IncrementQuantityCommand => incrementQuantityCommand ??= new Command( IncrementQuantity );

        private Command deleteCommand;
        public ICommand DeleteCommand => deleteCommand ??= new Command( Delete );

        private AsyncCommand editMaterialCommand;
        public ICommand EditMaterialCommand => editMaterialCommand ??= new AsyncCommand( EditMaterial );

        #endregion

        #region METHODS

        public async Task AsyncInit( int stepMatId )
        {
            StepMaterial = await DbServiceLocator.GetItemAsync<StepMaterial>( stepMatId ).ConfigureAwait( false );
            if( StepMaterial != null )
            {
                StepMaterial.Name = StepMaterial.Material.Name;
                Quantity = StepMaterial.Quantity;
                MaterialId = StepMaterial.MaterialId;
            }
        }
        
        public async Task AsyncInit( int stepMatId, AddStepMaterialToStepViewModel viewModel )
        {
            StepMaterial = await DbServiceLocator.GetItemRecursiveAsync<StepMaterial>( stepMatId ).ConfigureAwait( false );
            if( StepMaterial != null )
            {
                StepMaterial.Name = StepMaterial.Material.Name;
                Quantity = StepMaterial.Quantity;
                MaterialId = StepMaterial.MaterialId;
            }

            AddStepMaterialToStepViewModel = viewModel;
        }

        public async Task Update()
        {
            stepMaterial.Quantity = quantity;

            await DbServiceLocator.UpdateItemAsync( stepMaterial ).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the user to the CreateNewMaterialView and pre-loads the view with the details of the Material that this StepMaterial is based on.
        /// </summary>
        internal async Task EditMaterial()
        {
            string encodedId = HttpUtility.UrlEncode( MaterialId.ToString() );
            await Shell.Current.GoToAsync( $"/{nameof( CreateNewMaterialView )}?editMaterialId={encodedId}" );
        }

        private void Delete()
        {
            AddStepMaterialToStepViewModel.RemoveMaterialFromSelectedMaterials( MaterialId );
        }

        private void DecrementQuantity()
        {
            Quantity--;
        }

        private void IncrementQuantity()
        {
            Quantity++;
        }
        #endregion

    }
}
