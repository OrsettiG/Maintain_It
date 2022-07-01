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
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;
using System.Linq;
using System.Web;
using Xamarin.Essentials;
using Maintain_it.Helpers;
using Xamarin.Forms.PlatformConfiguration;

namespace Maintain_it.ViewModels
{
    public class StepViewModel : BaseViewModel
    {
        public StepViewModel() { }

        public StepViewModel( MaintenanceItemViewModel maintenanceItemViewModel )
        {
            maintenanceItemVM = maintenanceItemViewModel;
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
        private string _name;
        public string Name { get => _name; set => SetProperty( ref _name, value ); }

        private int stepNum;
        public int StepNum { get => stepNum; set => SetProperty( ref stepNum, value ); }

        private string _description;
        public string Description { get => _description; set => SetProperty( ref _description, value ); }

        private bool _isCompleted;
        public bool IsCompleted { get => _isCompleted; set => SetProperty( ref _isCompleted, value ); }

        private double _timeRequired;
        public double TimeRequired { get => _timeRequired; set => SetProperty( ref _timeRequired, value ); }

        private Timeframe _timeframe = Timeframe.Minutes;
        public Timeframe Timeframe { get => _timeframe; set => SetProperty( ref _timeframe, value ); }

        private string _noteText;
        public string NoteText { get => _noteText; set => SetProperty( ref _noteText, value ); }

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

        private int _stepMatCounter;
        public int StepMatCounter { get => _stepMatCounter; set => SetProperty( ref _stepMatCounter, value >= 0 ? value : 0 ); }

        private ObservableRangeCollection<StepMaterial> _stepMaterials;
        public ObservableRangeCollection<StepMaterial> StepMaterials { get => _stepMaterials ??= new ObservableRangeCollection<StepMaterial>(); set => SetProperty( ref _stepMaterials, value ); }

        //TODO: Update this (and all others) to use NoteViewModel instead of the Model directly
        private ObservableRangeCollection<NoteViewModel> notes;
        public ObservableRangeCollection<NoteViewModel> Notes => notes ??= new ObservableRangeCollection<NoteViewModel>();

        public List<Timeframe> timeframes => Options.timeframes;

        private Step step;
        public Step Step { get => step; set => step = value; }

        private Step nextStep;
        public Step NextStep { get => nextStep; set => SetProperty( ref nextStep, value ); }

        private Step previousStep;
        public Step PreviousStep { get => previousStep; set => SetProperty( ref previousStep, value ); }

        public MaintenanceItemViewModel maintenanceItemVM { get; }
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
        public ICommand AddStepCommand => addStepCommand ??= new AsyncCommand( AddStep );

        private AsyncCommand selectMaterialsCommand;
        public ICommand SelectMaterialsCommand => selectMaterialsCommand ??= new AsyncCommand( SelectMaterials );

        private AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand => addNoteCommand ??= new AsyncCommand( AddNote );

        private ICommand showHideAddNoteCommand;
        public ICommand ShowHideAddNoteCommand => showHideAddNoteCommand ??= new Command( ShowHideAddNote );

        private Command decrementStepMatQuantityCommand;
        public ICommand DecrementStepMatQuantityCommand => decrementStepMatQuantityCommand ??= new Command( DecrementStepMatCounter );

        private Command incrementStepMatQuantityCommand;
        public ICommand IncrementStepMatQuantityCommand => incrementStepMatQuantityCommand ??= new Command( IncrementStepMatCounter );

        private ICommand toggleCanDragCommand;
        public ICommand ToggleCanDragCommand
        {
            get => toggleCanDragCommand ??= new Command( ToggleCanDrag );
        }

        private void ToggleCanDrag()
        {
            CanDrag = !CanDrag;
        }

        #endregion

        #region Drag and Drop

        #region Drag and Drop Commands

        private ICommand dropCompleteCommand;
        public ICommand DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<StepViewModel>( x => DropComplete( x ) );

        private ICommand dragOverCommand;
        public ICommand DragOverCommand => dragOverCommand ??= new AsyncCommand<StepViewModel>( x => DragOver( x ) );

        private ICommand dragLeaveCommand;
        public ICommand DragLeaveCommand => dragLeaveCommand ??= new AsyncCommand<StepViewModel>( x => DragLeave( x ) );

        private ICommand dropCommand;
        public ICommand DropCommand => dropCommand ??= new AsyncCommand<StepViewModel>( x => Drop( x ) );

        private ICommand dragStartingCommand;
        public ICommand DragStartingCommand => dragStartingCommand ??= new AsyncCommand<StepViewModel>( x => DragStarting( x ) );

        #endregion

        #region Drag and Drop Methods

        private async Task DragStarting( StepViewModel item )
        {
            Dragging = true;
        }

        private async Task Drop( StepViewModel itemDroppedOn )
        {
            StepViewModel itemDropping = maintenanceItemVM.StepViewModels.First(i => i.Dragging);
            if( itemDropping != null )
            {
                int index1 = maintenanceItemVM.StepViewModels.IndexOf(itemDropping);
                int index2 = maintenanceItemVM.StepViewModels.IndexOf(itemDroppedOn);

                maintenanceItemVM.StepViewModels.Move( index1, index2 );

                int droppedOnStepNum = itemDroppedOn.StepNum;

                if( droppedOnStepNum > itemDropping.StepNum )
                {
                    foreach( StepViewModel item in maintenanceItemVM.StepViewModels.Where( x => x.StepNum <= droppedOnStepNum && x.StepNum >= itemDropping.StepNum && x.Step.Id != itemDropping.Step.Id ) )
                    {
                        item.Step.Index--;
                        item.StepNum--;
                    }
                }
                else if( droppedOnStepNum < itemDropping.StepNum )
                {
                    foreach( StepViewModel item in maintenanceItemVM.StepViewModels.Where( x => x.StepNum >= droppedOnStepNum && x.StepNum <= itemDropping.StepNum && x.Step.Id != itemDropping.Step.Id ) )
                    {
                        item.Step.Index++;
                        item.StepNum++;
                    }
                }

                itemDropping.StepNum = droppedOnStepNum;
                itemDropping.Step.Index = droppedOnStepNum;
            }

        }

        private async Task DragLeave( StepViewModel item )
        {
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

        public async Task Init( int id )
        {
            Step = await StepManager.GetItemRecursiveAsync( id );

            Name = Step.Name;
            Description = Step.Description;
            TimeRequired = Step.TimeRequired;
            Timeframe = (Timeframe)Step.Timeframe;
            IsCompleted = Step.IsCompleted;
            StepNum = Step.Index;
            StepMaterials.AddRange( Step.StepMaterials );
            // TODO: Make getting all these note view models a bit more elegant.
            Notes.AddRange( await NoteManager.GetItemRangeAsViewModelsAsync( Step.Notes.GetIds() ) );
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

        private async Task AddStep()
        {

            int stepId = await StepManager.NewStep( isFirstStep, Name, Description, IsCompleted, TimeRequired, (int)Timeframe );

            await StepManager.UpdateItemIndexAsync( stepId, StepNum );
            Step = await StepManager.GetItemRecursiveAsync( stepId );
            //TODO: Add Notes to Step
            if( Notes.Count > 0 )
            {
                List<int> ids = new List<int>();

                foreach( NoteViewModel nvm in Notes )
                {
                    ids.Add( nvm.NoteId );
                }

                await StepManager.AddNotes( ids, Step.Id );
            }

            //TODO: Add StepMaterials to Step
            if( StepMaterials.Count > 0 )
            {
                await StepManager.AddStepMaterials( stepMaterialIds, Step.Id );
            }

            if( maintenanceItemId != null )
            {
                _ = await StepManager.UpdateMaintenanceItem( (int)maintenanceItemId, stepId );
            }

            string encodedId = HttpUtility.UrlEncode( stepId.ToString());

            await Shell.Current.GoToAsync( $"..?stepIds={encodedId}" );
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
                await Shell.Current.GoToAsync( $"/{nameof( AddStepMaterialsToStepView )}?{RoutingPath.Refresh}=true" );
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
                case RoutingPath.MaintenanceItemId:
                    string id = HttpUtility.UrlDecode(kvp.Value);
                    if( int.TryParse( id, out int itemId ) )
                    {
                        maintenanceItemId = itemId;
                    }
                    break;
                case RoutingPath.RefreshNote:
                    if( int.TryParse( kvp.Value, out int noteId ) )
                    {
                        await RefreshNote( noteId );
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
            StepMaterials.AddRange( stepMats );
        }
        #endregion
    }
}
