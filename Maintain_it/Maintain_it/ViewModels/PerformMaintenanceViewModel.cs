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
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class PerformMaintenanceViewModel : BaseViewModel
    {
        public PerformMaintenanceViewModel() { }

        #region Properties
        private Step step;
        public Step Step { get => step; private set => SetProperty( ref step, value ); }

        private string title;
        public string Title { get => title; set => SetProperty( ref title, value ); }

        private string description;
        public string Description { get => description; set => SetProperty( ref description, value ); }

        private ObservableRangeCollection<NoteViewModel> notes;
        public ObservableRangeCollection<NoteViewModel> Notes
        {
            get => notes ??= new ObservableRangeCollection<NoteViewModel>();
            set => SetProperty( ref notes, value );
        }

        private ObservableRangeCollection<StepMaterial> stepMaterials;
        public ObservableRangeCollection<StepMaterial> StepMaterials
        {
            get => stepMaterials ??= new ObservableRangeCollection<StepMaterial>();
            set => SetProperty( ref stepMaterials, value );
        }

        private bool isCompleted;
        public bool IsCompleted { get => isCompleted; set => SetProperty( ref isCompleted, value ); }

        private double timeRequired;
        public double TimeRequired { get => timeRequired; set => SetProperty( ref timeRequired, value ); }

        private double timeTaken;
        public double TimeTaken
        {
            get => timeTaken;
            set => SetProperty( ref timeTaken, value );
        }

        private int timeframe;
        public string Timeframe
        {
            get => ( (Timeframe)timeframe ).ToString();
            set => SetProperty( ref timeframe, int.Parse( value ) );
        }

        private DateTime createdOn;
        public DateTime CreatedOn { get => createdOn; set => SetProperty( ref createdOn, value ); }

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
        private async Task NextStep()
        {
            if( Step != null && Step.NextNodeId != 0 )
            {
                Step step = await StepManager.GetItemRecursiveAsync( (int)Step.NextNodeId );

                if( step != null )
                {
                    Step = step;

                    await Refresh();
                }
            }
        }

        private AsyncCommand previousStepCommand;
        public ICommand PreviousStepCommand => previousStepCommand ??= new AsyncCommand( PreviousStep );
        private async Task PreviousStep()
        {
            if( Step != null && Step.PreviousNodeId != 0 )
            {
                Step step = await StepManager.GetItemRecursiveAsync( (int)Step.PreviousNodeId );

                if( step != null )
                {
                    Step = step;

                    await Refresh();
                }
            }

        }

        private AsyncCommand completeStepCommand;
        public ICommand CompleteStepCommand => completeStepCommand ??= new AsyncCommand( CompleteStep );
        private async Task CompleteStep()
        {
            await StepManager.CompleteStep( Step.Id );

            if( Step.NextNodeId != 0 )
            {
                await NextStep();
            }
            else
            {
                await MaintenanceItemManager.CompleteMaintenance( Step.MaintenanceItemId, TimeTaken );
                await Shell.Current.GoToAsync( $"../?{RoutingPath.Refresh}=true" );
            }
        }

        private AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand
        {
            get => addNoteCommand ??= new AsyncCommand( AddNote );
        }

        private async Task AddNote()
        {

        }

        #endregion

        #region Methods


        private async Task Refresh()
        {
            if( Step == null )
                return;

            Step = await StepManager.GetItemRecursiveAsync( Step.Id );
            StepViewModel = new StepViewModel( Step );

            StepNumber = Step.Index.ToString();
            // !!! PICK UP HERE !!! Get this viewmodel properly populated and then sort out the addition and subtraction of materials. After that figure out how/what to do when a service is completed.
            IsCompleted = !Step.IsCompleted;
            Description = Step.Description;
            TimeRequired = Step.TimeRequired;
            Timeframe = Step.Timeframe.ToString();

            StepMaterials.Clear();
            StepMaterials.AddRange( Step.StepMaterials );

            List<int> noteIds = new List<int>( Step.Notes.GetIds() );
            Notes.Clear();
            Notes.AddRange( await NoteManager.GetItemRangeAsViewModelsAsync( noteIds ) );

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

                        if( maintenanceItem.Steps.Count > 0 )
                        {
                            int index = maintenanceItem.ServiceRecords.Last().CurrentStepIndex;
                            
                            int? stepId = maintenanceItem.Steps.Where( x => x.Index == index ).FirstOrDefault()?.Id;

                            if( stepId == null)
                            {
                                stepId = maintenanceItem.Steps.OrderBy( x => x.Index ).First().Id;
                            }

                            Step = await StepManager.GetItemRecursiveAsync( stepId.Value );
                        }
                    }

                    await Refresh();
                    break;
            }
        }

        #endregion
    }
}
