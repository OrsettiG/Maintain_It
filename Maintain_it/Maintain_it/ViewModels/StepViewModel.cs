using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

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
        public double TimeRequired { get; set; }
        public Timeframe Timeframe { get; set; }

        public ObservableRangeCollection<StepMaterial> StepMaterials;
        public ObservableRangeCollection<Note> Notes;

        private Step step;


        #region COMMANDS

        AsyncCommand addCommand;
        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );

        #endregion

        #region METHODS

        private async Task Add()
        {
            Console.WriteLine( "Add Step Command Fired" );

            step = new Step()
            {
                Name = Name,
                Description = Description,
                TimeRequired = TimeRequired,
                Timeframe = Timeframe.MINUTES,
                IsCompleted = false,

                StepMaterials = await ConvertToListAsync( StepMaterials ),
                Notes = await ConvertToListAsync( Notes )
            };

            int stepId = await DbServiceLocator.AddItemAndReturnIdAsync( step );
            await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}/?newStepId={stepId}" );
        }

        private protected override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
