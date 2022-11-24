using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Helpers;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;
using System.Linq;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;

namespace Maintain_it.ViewModels
{
    public class StepViewModel : BaseViewModel
    {
        public StepViewModel() { }

        public StepViewModel( MaintenanceItemViewModel maintenanceItemViewModel )
        {
            MaintenanceItemVm = maintenanceItemViewModel;
        }

        public StepViewModel( Step step )
        {
            Step = step;
            Name = step.Name;
            Description = step.Description;
            StepNum = step.Index;
            TimeRequired = step.TimeRequired;
            MaintenanceItemName = step.MaintenanceItem.Name;
        }

        #region Parameters
        // TODO: Create Custom Entry Renderer for Android and IOS
        private string name;
        public string Name { get => name; set => SetProperty( ref name, value ); }

        private int stepNum;
        public int StepNum { get => stepNum; set => SetProperty( ref stepNum, value ); }

        private string description;
        public string Description { get => description; set => SetProperty( ref description, value ); }

        private bool isCompleted;
        public bool IsCompleted { get => isCompleted; set => SetProperty( ref isCompleted, value ); }

        private double timeRequired;
        public double TimeRequired { get => timeRequired; set => SetProperty( ref timeRequired, value ); }

        private Timeframe timeframe = Timeframe.Minutes;
        public Timeframe Timeframe { get => timeframe; set => SetProperty( ref timeframe, value ); }

        private string noteText;
        public string NoteText { get => noteText; set => SetProperty( ref noteText, value ); }

        private ImageSource noteImage;
        public ImageSource NoteImage { get => noteImage; set => SetProperty( ref noteImage, value ); }

        private NoteViewModel newNoteViewModel;
        public NoteViewModel NewNoteViewModel
        {
            get => newNoteViewModel ??= new NoteViewModel();
            set => SetProperty( ref newNoteViewModel, value );
        }

        private bool showAddNote = false;
        public bool ShowAddNote
        {
            get => showAddNote;
            set => SetProperty( ref showAddNote, value );
        }

        private bool showNotes = true;
        public bool ShowNotes
        {
            get => showNotes;
            set => SetProperty( ref showNotes, value );
        }

        private int stepMatCounter;
        public int StepMatCounter { get => stepMatCounter; set => SetProperty( ref stepMatCounter, value >= 0 ? value : 0 ); }

        private ObservableRangeCollection<StepMaterial> stepMaterials;
        public ObservableRangeCollection<StepMaterial> StepMaterials { get => stepMaterials ??= new ObservableRangeCollection<StepMaterial>(); set => SetProperty( ref stepMaterials, value ); }

        //TODO: Update this (and all others) to use NoteViewModel instead of the Model directly
        private ObservableRangeCollection<NoteViewModel> notes;
        public ObservableRangeCollection<NoteViewModel> Notes => notes ??= new ObservableRangeCollection<NoteViewModel>();

        public List<Timeframe> Timeframes => Options.timeframes;

        private Step step;
        public Step Step { get => step; set => SetProperty( ref step, value ); }

        private Step nextStep;
        public Step NextStep { get => nextStep; set => SetProperty( ref nextStep, value ); }

        private Step previousStep;
        public Step PreviousStep { get => previousStep; set => SetProperty( ref previousStep, value ); }

        public MaintenanceItemViewModel MaintenanceItemVm { get; }
        public string MaintenanceItemName { get; }
        private bool Dragging = false;

        private bool canDrag = false;
        public bool CanDrag
        {
            get => canDrag;
            set => SetProperty( ref canDrag, value );
        }

        #region Query Parameters
        private HashSet<int> stepMaterialIds = new HashSet<int>();
        private int? previousStepId = null;
        private bool isFirstStep = false;
        private int? maintenanceItemId = null;
        #endregion

        #endregion

        #region COMMANDS

        private AsyncCommand addStepCommand;
        public ICommand AddStepCommand => addStepCommand ??= new AsyncCommand( AddOrUpdate );

        private AsyncCommand selectMaterialsCommand;
        public ICommand SelectMaterialsCommand => selectMaterialsCommand ??= new AsyncCommand( SelectMaterials );

        private AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand => addNoteCommand ??= new AsyncCommand( AddNote );

        private ICommand showHideAddNoteCommand;
        public ICommand ShowHideAddNoteCommand => showHideAddNoteCommand ??= new Command( ShowHideAddNote );

        private ICommand toggleCanDragCommand;
        public ICommand ToggleCanDragCommand => toggleCanDragCommand ??= new Command( ToggleCanDrag );

        private void ToggleCanDrag()
        {
            CanDrag = !CanDrag;
        }

        private AsyncCommand openCommand;
        public ICommand OpenCommand => openCommand ??= new AsyncCommand( Open );

        private async Task Open()
        {
            if( Step.Id == 0 )
            {
                return;
            }

            string encodedId = HttpUtility.UrlEncode($"{Step.Id}");

            await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}?{QueryParameters.StepId}={encodedId}" );
        }

        private AsyncCommand deleteCommand;
        public ICommand DeleteCommand => deleteCommand ??= new AsyncCommand( Delete );

        private async Task Delete()
        {
            bool choice = await Shell.Current.DisplayAlert( Alerts.DeleteStepTitle, Alerts.DeleteStepMessage, Alerts.ConfirmDelete, Alerts.CancelDelete );

            switch( choice )
            {
                case true:
                    _ = await ServiceItemManager.RemoveStep( Step.MaintenanceItemId, Step.Id );
                    await StepManager.DeleteItem( Step.Id );
                    await MaintenanceItemVm.RefreshStepsCommand.ExecuteAsync();
                    break;

                case false:
                    break;
            }
        }

        private AsyncCommand refreshAllCommand;
        public ICommand RefreshAllCommand
        {
            get => refreshAllCommand ??= new AsyncCommand( RefreshAll );
        }

        #endregion

        #region Drag and Drop

        #region Drag and Drop Commands

        private ICommand dropCompleteCommand;
        public ICommand DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<StepViewModel>( x => DropComplete( x ) );

        private ICommand dragOverCommand;
        public ICommand DragOverCommand => dragOverCommand ??= new AsyncCommand<StepViewModel>( x => DragOver( x ) );

        private ICommand dropCommand;
        public ICommand DropCommand => dropCommand ??= new AsyncCommand<StepViewModel>( x => Drop( x ) );

        private ICommand dragStartingCommand;
        public ICommand DragStartingCommand => dragStartingCommand ??= new MvvmHelpers.Commands.Command<StepViewModel>( x => DragStarting( x ) );

        #endregion

        #region Drag and Drop Methods

        private void DragStarting( StepViewModel item ) => Dragging = true;

        private Task Drop( StepViewModel itemDroppedOn )
        {
            StepViewModel itemDropping = MaintenanceItemVm.StepViewModels.First(i => i.Dragging);
            {
                int index1 = MaintenanceItemVm.StepViewModels.IndexOf(itemDropping);
                int index2 = MaintenanceItemVm.StepViewModels.IndexOf(itemDroppedOn);

                MaintenanceItemVm.StepViewModels.Move( index1, index2 );

                int droppedOnStepNum = itemDroppedOn.StepNum;

                if( droppedOnStepNum > itemDropping.StepNum )
                {
                    foreach( StepViewModel item in MaintenanceItemVm.StepViewModels.Where( x => x.StepNum <= droppedOnStepNum && x.StepNum >= itemDropping.StepNum && x.Step.Id != itemDropping.Step.Id ) )
                    {
                        item.Step.Index--;
                        item.StepNum--;
                    }
                }
                else if( droppedOnStepNum < itemDropping.StepNum )
                {
                    foreach( StepViewModel item in MaintenanceItemVm.StepViewModels.Where( x => x.StepNum >= droppedOnStepNum && x.StepNum <= itemDropping.StepNum && x.Step.Id != itemDropping.Step.Id ) )
                    {
                        item.Step.Index++;
                        item.StepNum++;
                    }
                }

                itemDropping.StepNum = droppedOnStepNum;
                itemDropping.Step.Index = droppedOnStepNum;
            }

            return Task.CompletedTask;
        }

        private async Task DragOver( StepViewModel item )
        {
        }

        private async Task DropComplete( StepViewModel item )
        {
            Dragging = false;
        }
        #endregion
        #endregion


        #region METHODS

        public async Task DeepInitAsync()
        {
            if( Step == null )
                return;

            if( Step.PreviousNodeId != 0 )
            {
                PreviousStep = await StepManager.GetItemAsync( Step.PreviousNodeId );
            }

            if( Step.NextNodeId != 0 )
            {
                NextStep = await StepManager.GetItemAsync( Step.NextNodeId );
            }

            if( Step.StepMaterials.Count > 0 )
            {
                // TODO: Make convert this to using ViewModels instead of the Model directly.
                StepMaterials.AddRange( Step.StepMaterials );
            }

            if( Step.Notes.Count > 0 )
            {
                // TODO: Make getting all these note view models a bit more elegant.
                IEnumerable<NoteViewModel> noteVMs = await NoteManager.GetItemRangeAsViewModelsAsync( Step.Notes.GetIds() );

                Notes.AddRange( noteVMs );
            }
        }

        public async Task InitAsync()
        {
            Task[] tasks = new Task[]
            {
                new Task( () => Name = step.Name ),
                new Task( () => Description = step.Description ),
                new Task( () => TimeRequired = step.TimeRequired ),
                new Task( () => Timeframe = (Timeframe)step.Timeframe ),
                new Task( () => IsCompleted = step.IsCompleted ),
                new Task( () => StepNum = step.Index ),
                new Task( () => StepMaterials.AddRange( step.StepMaterials ) ),
                new Task( async () => Notes.AddRange(
                                                      await NoteManager.GetItemRangeAsViewModelsAsync(
                                                          step.Notes.GetIds() ) ) )
            };

            await Task.WhenAll( tasks ).ConfigureAwait( false );
        }

        private void ShowHideAddNote()
        {
            ShowAddNote = !ShowAddNote;
            ShowNotes = !ShowNotes;
        }

        private void DecrementStepMatCounter()
        {
            StepMatCounter--;
        }

        private void IncrementStepMatCounter()
        {
            StepMatCounter++;
        }

        private async Task AddOrUpdate()
        {
            if( Step == null )
            {
                await AddStep();
            }
            else
            {
                await UpdateStep();
            }
        }

        private async Task AddStep()
        {
            int stepId = await StepManager.NewStep( isFirstStep, Name, Description, IsCompleted, TimeRequired, (int)Timeframe );

            await StepManager.UpdateItemIndexAsync( stepId, StepNum );
            Step = await StepManager.GetItemRecursiveAsync( stepId );
            //TODO: AddShallow Notes to Step
            if( Notes.Count > 0 )
            {
                List<int> ids = new List<int>();

                foreach( NoteViewModel nvm in Notes )
                {
                    ids.Add( nvm.NoteId );
                }

                await StepManager.AddNotes( ids, Step.Id );
            }

            //TODO: AddShallow StepMaterials to Step
            if( StepMaterials.Count > 0 )
            {
                await StepManager.AddStepMaterials( stepMaterialIds, Step.Id );
            }

            if( maintenanceItemId != null )
            {
                if( await StepManager.UpdateServiceItem( (int)maintenanceItemId, stepId ) )
                    await ServiceItemManager.AddStep( (int)maintenanceItemId, stepId );
            }

            string encodedId = HttpUtility.UrlEncode( stepId.ToString());

            await Shell.Current.GoToAsync( $"..?stepIds={encodedId}" );
        }

        private async Task UpdateStep()
        {

            List<int> noteIds = new List<int>(), stepMaterialIds = new List<int>();

            foreach( NoteViewModel vm in Notes )
            {
                noteIds.Add( vm.NoteId );
            }

            //TODO: Update StepMaterials to use ViewModels instead of the Model directly
            stepMaterialIds.AddRange( StepMaterials.Select( sMvm => sMvm.Id ) );

            await StepManager.UpdateItemAsync( Step.Id, name: Name, description: Description, isCompleted: IsCompleted, timeRequired: TimeRequired, timeFrame: (int)Timeframe, stepMaterialIds: stepMaterialIds, noteIds: noteIds );

            await Shell.Current.GoToAsync( $"..?{QueryParameters.RefreshSteps}=true" );
        }

        private async Task SelectMaterials()
        {
            if( stepMaterialIds.Count > 0 )
            {
                string encodedIds = HttpUtility.UrlEncode( string.Join( ',', stepMaterialIds ) );
                await Shell.Current.GoToAsync( $"/{nameof( AddStepMaterialsToStepView )}?preselectedStepMaterialIds={encodedIds}" );
            }
            else
            {
                await Shell.Current.GoToAsync( $"/{nameof( AddStepMaterialsToStepView )}?{QueryParameters.Refresh}=true" );
            }
        }

        private async Task AddNote()
        {
            int id = await NewNoteViewModel.Save();

            Notes.Add( await NoteManager.GetItemAsViewModelAsync( id ) );

            NewNoteViewModel = new NoteViewModel
            {
                Text = string.Empty,
                Image = default,
                CreatedOn = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            ShowHideAddNote();
        }

        private async Task Refresh()
        {
            if( Notes.Count > 0 )
            {
                List<int> ids = new List<int>();

                foreach( NoteViewModel nvm in Notes )
                {
                    ids.Add( nvm.NoteId );
                }

                Notes.Clear();
                Notes.AddRange( await NoteManager.GetItemRangeAsViewModelsAsync( ids ) );
            }

            if( stepMaterialIds.Count > 0 )
            {
                await RetrieveStepMaterialsFromDb();
            }
        }

        private async Task RefreshNote( int noteId )
        {
            NoteViewModel nvm = Notes.Where(x => x.NoteId == noteId ).FirstOrDefault();

            if( nvm != null )
            {
                await nvm.RefreshCommand.ExecuteAsync();
            }
        }

        private async Task RefreshAll()
        {
            Name = Step.Name;
            StepNum = Step.Index;
            Description = Step.Description;
            IsCompleted = Step.IsCompleted;
            TimeRequired = Step.TimeRequired;
            Timeframe = (Timeframe)Step.Timeframe;

            StepMaterials.Clear();
            StepMaterials.AddRange( Step.StepMaterials.OrderByDescending( x => x.Quantity ) );

            IEnumerable<NoteViewModel> notes = await NoteManager.GetItemRangeAsViewModelsAsync( Step.Notes.GetIds() );

            Notes.Clear();
            Notes.AddRange( notes.OrderBy( x => x.HasImage == true ) );
        }

        private async Task RefreshAll( int StepId )
        {
            Step = await StepManager.GetItemRecursiveAsync( StepId );
            await RefreshAll();
        }

        #endregion

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case nameof( stepMaterialIds ):
                    ParseStepMaterialIds( kvp.Value );
                    await RetrieveStepMaterialsFromDb();
                    break;
                case nameof( previousStepId ):
                    await ParseStepNumber( kvp.Value );
                    break;
                case nameof( isFirstStep ):
                    isFirstStep = true;
                    StepNum = 1;
                    PreviousStep = null;
                    NextStep = null;
                    break;
                case QueryParameters.ServiceItemId:
                    string id = HttpUtility.UrlDecode(kvp.Value);
                    if( int.TryParse( id, out int itemId ) )
                    {
                        maintenanceItemId = itemId;
                    }
                    break;
                case QueryParameters.RefreshNote:
                    if( int.TryParse( kvp.Value, out int noteId ) )
                    {
                        await RefreshNote( noteId );
                    }
                    break;
                case QueryParameters.StepId:
                    string stepId = HttpUtility.UrlDecode(kvp.Value);
                    if( int.TryParse( stepId, out int parsedStepId ) && parsedStepId != 0 )
                    {
                        await RefreshAll( parsedStepId );
                    }
                    break;
                default:
                    break;
            }
        }

        private void ParseStepMaterialIds( string encodedValue )
        {
            string decodedValue = HttpUtility.UrlDecode( encodedValue );
            string[] ids = decodedValue.Split(',');
            stepMaterialIds = new HashSet<int>( ids.Length );
            foreach( string id in ids )
            {
                if( int.TryParse( id, out int result ) )
                {
                    _ = stepMaterialIds.Add( result );
                }
            }
        }

        private async Task ParseStepNumber( string value )
        {
            string num = HttpUtility.UrlDecode( value );


            if( int.TryParse( num, out int pStepId ) )
            {
                previousStepId = pStepId;
                PreviousStep = await DbServiceLocator.GetItemRecursiveAsync<Step>( pStepId ).ConfigureAwait( false );
                stepNum = PreviousStep.Index + 1;

                NextStep = null;
            }
        }

        private async Task RetrieveStepMaterialsFromDb()
        {
            List<StepMaterial> stepMats = await DbServiceLocator.GetItemRangeAsync<StepMaterial>( stepMaterialIds ) as List<StepMaterial>;
            StepMaterials.Clear();
            StepMaterials.AddRange( stepMats.OrderByDescending( x => x.Quantity ) );
        }
        #endregion
    }
}
