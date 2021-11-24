using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    internal class StepMaterialViewModel : BaseViewModel
    {
        public StepMaterialViewModel()
        {

        }

        public StepMaterialViewModel( Material material )
        {
            stepMaterial = new StepMaterial()
            {
                Name = material.Name,
                CreatedOn = DateTime.Now,
                MaterialId = material.Id,
                Material = material
            };

            MaterialId = material.Id;
        }

        #region PROPERTIES
        private const int maxQuantity = 1000001;

        private StepMaterial stepMaterial;
        public StepMaterial StepMaterial { get => stepMaterial; set => SetProperty( ref stepMaterial, value ); }

        public int MaterialId { get; private set; }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if(value > 0 && value < maxQuantity )
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

        #endregion

        #region METHODS

        public async Task AsyncInit( int stepMatId )
        {
            StepMaterial = await DbServiceLocator.GetItemAsync<StepMaterial>( stepMatId ).ConfigureAwait( false );

            Quantity = StepMaterial.Quantity;
            MaterialId = StepMaterial.MaterialId;

        }


        private void DecrementQuantity()
        {
            Quantity = Quantity > 1 ? Quantity-- : 0;
        }

        private void IncrementQuantity()
        {
            Quantity = Quantity < maxQuantity ? Quantity++ : Quantity;
        }
        #endregion

    }
}
