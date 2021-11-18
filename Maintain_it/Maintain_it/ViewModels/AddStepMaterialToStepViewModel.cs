using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    internal class AddStepMaterialToStepViewModel : BaseViewModel
    {
        public AddStepMaterialToStepViewModel() { }

        #region Properties

        private bool locked = false;

        private ObservableRangeCollection<StepMaterial> _stepMaterials;
        public ObservableRangeCollection<StepMaterial> StepMaterials { get => _stepMaterials; set => SetProperty( ref _stepMaterials, value ); }

        private ObservableRangeCollection<Material> _materials;
        public ObservableRangeCollection<Material> Materials { get => _materials; set => SetProperty( ref _materials, value ); }
        #endregion

        #region Commands

        private AsyncCommand refreshCommand;
        public AsyncCommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );

        #endregion

        #region Methods

        private async Task Refresh()
        {
            if( !locked )
            {
                locked = true;

                List<Material> mats = await DbServiceLocator.GetAllItemsAsync<Material>() as List<Material>;
            
                Materials.Clear();
            
                foreach ( Material mat in mats )
                {
                    Materials.Add( mat );
                }

                locked = false;
            }
        }

        private async Task AddNewStepMaterial()
        {

        }

        private protected override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
