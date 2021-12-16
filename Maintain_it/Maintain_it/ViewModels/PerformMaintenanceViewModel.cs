using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class PerformMaintenanceViewModel : BaseViewModel
    {
        public PerformMaintenanceViewModel() { }

        #region Properties
        private Step step;
        public Step Step { get => step; private set => SetProperty( ref step, value ); }

        private StepViewModel stepViewModel;
        public StepViewModel StepViewModel { get => stepViewModel; private set => SetProperty( ref stepViewModel, value ); }

        private bool previousStepExists;
        public bool PreviousStepExists { get => previousStepExists; set => SetProperty( ref previousStepExists, value ); }

        private bool nextStepExists;
        public bool NextStepExists { get => nextStepExists; set => SetProperty( ref nextStepExists, value ); }

        private List<Step> steps = new List<Step>();
        private MaintenanceItem maintenanceItem { get; set; }
        #endregion

        #region Commands
        private AsyncCommand nextStepCommand;
        public ICommand NextStepCommand => nextStepCommand ??= new AsyncCommand( NextStep );

        private AsyncCommand previousStepCommand;
        public ICommand PreviousStepCommand => previousStepCommand ??= new AsyncCommand( PreviousStep );

        #endregion

        #region Methods
        private async Task NextStep()
        {
            if( Step.NextStep != null )
            {
                Step = Step.NextStep;

                await Refresh();
            }
        }

        private async Task PreviousStep()
        {
            if( Step.PreviousStep != null )
            {
                Step = Step.PreviousStep;

                await Refresh();
            }

        }

        private async Task Refresh()
        {
            StepViewModel.SaveStepCommand?.Execute( null );

            StepViewModel = new StepViewModel()
            {
                Step = Step
            };

            await StepViewModel.InitAsync().ConfigureAwait( false );

            PreviousStepExists = Step.PreviousStep != null;
            NextStepExists = Step.NextStep != null;


        }
        #endregion

        #region Query Handling

        #region Query Parameters
        private int maintenanceItemId;
        #endregion

        public override async void ApplyQueryAttributes( IDictionary<string, string> query )
        {
            foreach( KeyValuePair<string, string> kvp in query )
            {
                await EvaluateQueryParams( kvp );
            }
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case nameof( maintenanceItemId ):
                    if( int.TryParse( kvp.Value, out maintenanceItemId ) )
                    {
                        maintenanceItem = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( maintenanceItemId ).ConfigureAwait( false );

                        List<Step> steps = maintenanceItem.Steps.AsParallel().OrderBy( x => x.StepNumber ).ToList();
                        Step = steps.FirstOrDefault();
                    }

                    await Refresh().ConfigureAwait( false );
                    break;
            }
        }

        #endregion
    }
}
