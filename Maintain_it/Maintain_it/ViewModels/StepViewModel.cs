using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class StepViewModel : BaseViewModel
    {
        public StepViewModel()
        {
            StepMaterials = new ObservableRangeCollection<StepMaterial>();
            Notes = new ObservableRangeCollection<Note>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public float TimeRequired { get; set; }
        public Timeframe Timeframe { get; set; }

        public ObservableRangeCollection<StepMaterial> StepMaterials;
        public ObservableRangeCollection<Note> Notes;

        private Step step;


        #region COMMANDS

        AsyncCommand addCommand;

        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );

        #endregion

        #region METHODS

        private async Task<int> Add()
        {
            return await DbServiceLocator.AddItemAndReturnIdAsync( step );
        }

        #endregion
    }
}
