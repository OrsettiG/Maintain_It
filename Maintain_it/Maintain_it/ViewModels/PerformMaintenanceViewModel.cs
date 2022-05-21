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

        private int stepNumber;
        public string StepNumber
        {
            get => $"Step {stepNumber}";
            set => SetProperty( ref stepNumber, int.Parse( value ) );
        }
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
            if( Step.NextNodeId != null )
            {
                Step step = await StepManager.GetItemRecursiveAsync( (int)Step.NextNodeId );

                if( step != null )
                {
                    Step = step;

                    await Refresh();
                }
            }
        }

        private async Task PreviousStep()
        {
            if( Step.PreviousNodeId != null )
            {
                Step step = await StepManager.GetItemRecursiveAsync( (int)Step.PreviousNodeId );

                if( step != null )
                {
                    Step = step;

                    await Refresh();
                }
            }

        }

        private async Task Refresh()
        {
            if( Step == null )
                return;

            Step = await StepManager.GetItemRecursiveAsync( Step.Id );
            StepViewModel = new StepViewModel()
            {
                Step = Step
            };

            StepNumber = Step.Index.ToString();

            await StepViewModel.InitAsync().ConfigureAwait( false );



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
                case RoutingPath.MaintenanceItemId:
                    if( int.TryParse( kvp.Value, out maintenanceItemId ) )
                    {
                        maintenanceItem = await MaintenanceItemManager.GetItemRecursiveAsync( maintenanceItemId );

                        Step = maintenanceItem.Steps.Where( x => x.Index == 1 ).FirstOrDefault();
                    }

                    await Refresh();
                    break;
            }
        }

        #endregion
    }
}
