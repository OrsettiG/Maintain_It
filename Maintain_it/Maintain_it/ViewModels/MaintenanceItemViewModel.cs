using System;
using System.Web;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using Maintain_it.Views;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;
using MvvmHelpers;


using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;
using Xamarin.Essentials;
using Maintain_it.Helpers;

namespace Maintain_it.ViewModels
{
    public class MaintenanceItemViewModel : BaseViewModel
    {
        #region Constructors
        public MaintenanceItemViewModel()
        {
        }

        public MaintenanceItemViewModel( MaintenanceItem maintenanceItem, HomeViewModel homeViewModel )
        {
            InitData( maintenanceItem );
            maintenanceItemId = maintenanceItem.Id;
            _homeViewModel = homeViewModel;
        }

        //public MaintenanceItemViewModel( MaintenanceItem item ) => Item = item;
        #endregion

        #region PROPERTIES
        private MaintenanceItem item;

        public List<Timeframe> Timeframes => Options.timeframes;

        public bool locked { get; private set; }

        private string name = "";
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        private string comment = "";
        public string Comment
        {
            get => comment;
            set => SetProperty( ref comment, value );
        }

        private DateTime firstServiceDate = DateTime.Now;
        public DateTime FirstServiceDate
        {
            get => firstServiceDate;
            set => SetProperty( ref firstServiceDate, value );
        }

        private TimeSpan serviceTime = Config.DefaultReminderTime;
        public TimeSpan ServiceTime
        {
            get => serviceTime;
            set => SetProperty( ref serviceTime, value );
        }

        private bool isRecurring = false;
        public bool IsRecurring
        {
            get => isRecurring;
            set
            {
                _ = SetProperty( ref isRecurring, value );
                HasServiceLimit = false;
            }
        }

        private int recursEvery;
        public int RecursEvery
        {
            get => recursEvery;
            set => SetProperty( ref recursEvery, ( value! > 0 && value! < 1000 ) ? value : 1 );
        }

        private Timeframe serviceTimeframe = Timeframe.Months;
        public Timeframe ServiceTimeframe
        {
            get => serviceTimeframe;
            set => SetProperty( ref serviceTimeframe, value );
        }

        private bool hasServiceLimit;
        public bool HasServiceLimit
        {
            get => hasServiceLimit;
            set => SetProperty( ref hasServiceLimit, value );
        }

        private int timesToRepeatService;
        public int TimesToRepeatService
        {
            get => timesToRepeatService;
            set => SetProperty( ref timesToRepeatService, value );
        }

        private int timesServiced;
        public int TimesServiced
        {
            get => timesServiced;
            set => SetProperty( ref timesServiced, value );
        }

        private bool previousServiceCompleted;
        public bool PreviousServiceCompleted
        {
            get => previousServiceCompleted;
            set => SetProperty( ref previousServiceCompleted, value );
        }

        private bool notifyOfNextServiceDate = true;
        public bool NotifyOfNextServiceDate
        {
            get => notifyOfNextServiceDate;
            set => SetProperty( ref notifyOfNextServiceDate, value );
        }

        private Timeframe noticeTimeframe = Timeframe.Days;
        public Timeframe NoticeTimeframe
        {
            get => noticeTimeframe;
            set => SetProperty( ref noticeTimeframe, value );
        }

        private int advanceNotice = 3;
        public int AdvanceNotice
        {
            get => advanceNotice;
            set => SetProperty( ref advanceNotice, value );
        }

        private int maxReminders = Config.DefaultReminders;
        public int TimesToRemind
        {
            get => maxReminders;
            set => SetProperty( ref maxReminders, value );
        }

        //TODO: Update all the query params here to use the RoutingPath constants instead.
        #region QUERY PARAMS
        private enum EditState { NewItem, Editing }
        private EditState editState = EditState.NewItem;
        private readonly List<int> stepIds = new List<int>();
        private int maintenanceItemId;
        //TODO: Get Rid of this and use the EditState Flag instead
        private bool update = false;
        #endregion

        private ObservableRangeCollection<StepViewModel> _stepViewModels;
        public ObservableRangeCollection<StepViewModel> StepViewModels
        {
            get => _stepViewModels ??= new ObservableRangeCollection<StepViewModel>();
            set => SetProperty( ref _stepViewModels, value.OrderBy( x => x.StepNum ) as ObservableRangeCollection<StepViewModel> );
        }

        private string completionTimeEstimate;
        public string CompletionTimeEstimate
        {
            get
            {
                if( completionTimeEstimate == null )
                {
                    CalculateServiceCompletionTimeEstimate();
                }

                return completionTimeEstimate;
            }

            set => SetProperty( ref completionTimeEstimate, value );
        }



        private bool canDrag = false;
        public bool CanDrag
        {
            get => canDrag;
            set => SetProperty( ref canDrag, value );
        }


        private HomeViewModel _homeViewModel;
        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public AsyncCommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand addMaterialsToShoppingCartCommand;
        public ICommand AddMaterialsToShoppingCartCommand => addMaterialsToShoppingCartCommand ??= new AsyncCommand( GoToAddMaterialsToShoppingList );

        private AsyncCommand deleteCommand;
        public AsyncCommand DeleteCommand => deleteCommand ??= new AsyncCommand( Delete );

        private AsyncCommand backCommand;
        public ICommand BackCommand => backCommand ??= new AsyncCommand( Back );

        private async Task Back()
        {
            string choice = await Shell.Current.DisplayActionSheet( Alerts.DiscardChangesTitle, Alerts.Cancel, null, Alerts.Discard, Alerts.Save );

            switch( choice )
            {
                case Alerts.Save:
                    await Add();
                    break;
                case Alerts.Discard:
                    // Delete sends the user back to their previous page, so we only need to call GoToAsync if they are discarding changes to an existing item
                    if( editState == EditState.NewItem )
                        await Delete();
                    else
                        await Shell.Current.GoToAsync( $"..?{RoutingPath.Refresh}=true" );
                    break;
                default:
                    break;
            }

        }

        private AsyncCommand editCommand;
        public AsyncCommand EditCommand => editCommand ??= new AsyncCommand( Edit );

        private AsyncCommand refreshCommand;
        public AsyncCommand RefreshCommand => refreshCommand ??= new AsyncCommand( LoadSteps );

        private AsyncCommand updateCommand;
        public AsyncCommand UpdateCommand => updateCommand ??= new AsyncCommand( Update );

        private Command onIncrementCommand;
        public ICommand OnIncrementCommand => onIncrementCommand ??= new Command( Increment );

        private Command onDecrementCommand;
        public ICommand OnDecrementCommand => onDecrementCommand ??= new Command( Decrement );

        private AsyncCommand newStepCommand;
        public ICommand NewStepCommand => newStepCommand ??= new AsyncCommand( NewStep );

        private ICommand toggleCanDragCommand;
        public ICommand ToggleCanDragCommand
        {
            get => toggleCanDragCommand ??= new Command( ToggleCanDrag );
        }

        private void ToggleCanDrag()
        {
            foreach( StepViewModel vm in StepViewModels )
            {
                vm.ToggleCanDragCommand?.Execute( null );
            }
        }

        private AsyncCommand startMaintenanceCommand;
        public ICommand StartMaintenanceCommand
        {
            get => startMaintenanceCommand ??= new AsyncCommand( StartMaintenance );
        }

        private async Task StartMaintenance()
        {
            _ = await MaintenanceItemManager.InsertServiceRecord( maintenanceItemId );

            string encodedId = HttpUtility.UrlEncode($"{maintenanceItemId}");

            await Shell.Current.GoToAsync( $"{nameof( PerformMaintenanceView )}?{RoutingPath.MaintenanceItemId}={encodedId}" );
        }
        #endregion

        #region METHODS
        private void Increment()
        {
            if( RecursEvery <= 999 )
                RecursEvery += 1;
        }

        private void Decrement()
        {
            if( RecursEvery > 0 )
                RecursEvery -= 1;
        }

        private async Task GoToAddMaterialsToShoppingList()
        {
            if( item != null )
            {
                ConcurrentDictionary<int, int> materialIdsAndQuantitysRequired = new ConcurrentDictionary<int, int>();

                foreach( Step step in item.Steps )
                {
                    Step s = await StepManager.GetItemRecursiveAsync( step.Id );

                    _ = Parallel.ForEach( s.StepMaterials, x =>
                    {
                        int quantityRequired = x.Quantity;
                        int quantityOwned = x.Material.QuantityOwned;

                        if( quantityRequired > quantityOwned )
                        {
                            int diff = quantityRequired - quantityOwned;
                            _ = materialIdsAndQuantitysRequired.AddOrUpdate( x.MaterialId, diff, ( key, oldValue ) => oldValue + diff );
                        }
                    } );
                }


                StringBuilder sb = new StringBuilder();

                if( materialIdsAndQuantitysRequired.Count > 0 )
                {
                    foreach( KeyValuePair<int, int> kvp in materialIdsAndQuantitysRequired )
                    {
                        _ = sb.Append( kvp.Key );
                        _ = sb.Append( "=" );
                        _ = sb.Append( kvp.Value );
                        _ = sb.Append( ";" );
                    }
                }

                string encodedQuery = HttpUtility.UrlEncode( sb.ToString() );

                if( encodedQuery != string.Empty )
                {
                    string encodedName = HttpUtility.UrlEncode( $"{item.Name} Shopping List" );

                    await Shell.Current.GoToAsync( $"{nameof( CreateNewShoppingListView )}?{RoutingPath.PreSelectedMaterialIds}={encodedQuery}&{RoutingPath.ItemName}={encodedName}" );
                }
                else
                {
                    await Shell.Current.DisplayAlert( Alerts.Information, Alerts.MaterialsAlreadyOwned, Alerts.Confirmation );
                }
            }
        }

        private async Task Add()
        {
            bool isActive = true;

            if( item.IsActive != true )
            {
                isActive = Alerts.Yes == await Shell.Current.DisplayPromptAsync( Alerts.SetProjectActive, Alerts.ProjectActiveStateMessage, accept: Alerts.Yes, cancel: Alerts.No );
            }

            if( item != null )
            {
                FirstServiceDate = FirstServiceDate.AddHours( ServiceTime.Hours ).AddMinutes( ServiceTime.Minutes );

                await MaintenanceItemManager.UpdateProperties( item.Id, name: Name, comment: Comment, firstServiceDate: FirstServiceDate, recursEvery: RecursEvery, timeframe: (int)ServiceTimeframe, hasServiceLimit: HasServiceLimit, timesToRepeatService: TimesToRepeatService, notifyOfNextServiceDate: NotifyOfNextServiceDate, advanceNotice: AdvanceNotice, advanceNoticeTimeframe: (int)NoticeTimeframe, reminders: TimesToRemind, steps: stepIds, isActive: isActive );
            }
            else
            {
                int id = await MaintenanceItemManager.NewMaintenanceItem( Name, FirstServiceDate, Comment, RecursEvery, true, (int)ServiceTimeframe, notifyOfNextServiceDate: NotifyOfNextServiceDate, timesToRemind: TimesToRemind, advanceNotice: AdvanceNotice, noticeTimeframe: (int)NoticeTimeframe, stepIds: stepIds );
            }

            ClearData();
            await Shell.Current.GoToAsync( $"..?{RoutingPath.Refresh}=true" );
        }

        private async Task<List<Step>> CreateStepList()
        {
            List<Step> list = new List<Step>();

            foreach( StepViewModel model in StepViewModels )
            {
                list.Add( model.Step );
            };

            return list;
        }

        //private async Task<StepViewModel> CreateNewStepViewModel( Step step )
        //{
        //    StepViewModel vm = new StepViewModel(this);

        //    await vm.Init( step.Id );

        //    return vm;
        //}

        private void CalculateServiceCompletionTimeEstimate()
        {
            double minutes = 0;
            TimeInMinutes largestTimeframe = TimeInMinutes.None;

            foreach( Step step in item.Steps )
            {
                switch( (Timeframe)step.Timeframe )
                {
                    case Timeframe.Minutes:
                        minutes += step.TimeRequired;

                        if( largestTimeframe < TimeInMinutes.Minutes )
                            largestTimeframe = TimeInMinutes.Minutes;
                        break;

                    case Timeframe.Hours:
                        minutes += step.TimeRequired * 60;

                        if( largestTimeframe < TimeInMinutes.Hours )
                            largestTimeframe = TimeInMinutes.Hours;
                        break;

                    case Timeframe.Days:
                        minutes += step.TimeRequired * 1440;

                        if( largestTimeframe < TimeInMinutes.Days )
                            largestTimeframe = TimeInMinutes.Days;
                        break;

                    case Timeframe.Weeks:
                        minutes += step.TimeRequired * 10080;

                        if( largestTimeframe < TimeInMinutes.Weeks )
                            largestTimeframe = TimeInMinutes.Weeks;
                        break;

                    case Timeframe.Months:
                        minutes += step.TimeRequired * 43800;
                        if( largestTimeframe < TimeInMinutes.Months )
                            largestTimeframe = TimeInMinutes.Months;
                        break;

                    case Timeframe.Years:
                        minutes += step.TimeRequired * 525600;
                        if( largestTimeframe < TimeInMinutes.Years )
                            largestTimeframe = TimeInMinutes.Years;
                        break;
                }
            }

            CompletionTimeEstimate = largestTimeframe != TimeInMinutes.None ? $"{Math.Round( minutes / (int)largestTimeframe, 1 )} {largestTimeframe}" : "Unavailable";
        }

        private async Task<StepViewModel> CreateNewStepViewModel( int stepId )
        {
            StepViewModel vm = new StepViewModel(this);

            await vm.Init( stepId );

            return vm;
        }

        private void InitData( MaintenanceItem maintenanceItem, bool _update = false )
        {
            item = maintenanceItem;

            if( _update )
            {
                editState = EditState.Editing;
                update = true;
            }

            Name = item.Name;
            Comment = item.Comment;
            FirstServiceDate = item.NextServiceDate ?? item.FirstServiceDate;
            RecursEvery = item.RecursEvery;
            ServiceTimeframe = (Timeframe)item.ServiceTimeframe;
            TimesToRepeatService = item.TimesToRepeatService;
            NotifyOfNextServiceDate = item.NotifyOfNextServiceDate;
            AdvanceNotice = item.AdvanceNotice;
            NoticeTimeframe = (Timeframe)item.NoticeTimeframe;
            TimesServiced = item.ServiceRecords.Count;
            PreviousServiceCompleted = TimesServiced > 0 && item.ServiceRecords[^1].ServiceCompleted;
            NotifyOfNextServiceDate = item.NotifyOfNextServiceDate;

            if( item.Steps != null || item.Steps.Count > 0 )
            {
                stepIds.AddRange( GetStepIds() );
                _ = Task.Run( async () => await RefreshSteps() );
            }

        }

        private IEnumerable<int> GetStepIds()
        {
            List<int> ids = new List<int>();
            foreach( Step step in item.Steps )
            {
                ids.Add( step.Id );
            }
            return ids;
        }

        private void ClearData()
        {
            if( !locked )
            {
                locked = true;

                Name = string.Empty;
                Comment = string.Empty;
                FirstServiceDate = DateTime.UtcNow.ToLocalTime();
                IsRecurring = false;
                RecursEvery = 0;
                ServiceTimeframe = Timeframe.Months;
                TimesServiced = 0;
                PreviousServiceCompleted = false;
                NotifyOfNextServiceDate = false;
                StepViewModels.Clear();
                item = null;

                locked = false;
            }
        }

        private async Task LoadSteps()
        {
            if( !locked )
            {
                locked = true;

                StepViewModels.Clear();

                foreach( int id in stepIds )
                {
                    StepViewModels.Add( await CreateNewStepViewModel( id ) );
                }

                CalculateServiceCompletionTimeEstimate();

                locked = false;
            }
        }

        private async Task RefreshSteps()
        {
            if( !locked )
            {
                locked = true;

                StepViewModels.Clear();

                List<StepViewModel> data = new List<StepViewModel>();

                foreach( Step step in item.Steps )
                {
                    data.Add( await CreateNewStepViewModel( step.Id ) );
                }

                StepViewModels.AddRange( data.OrderBy( x => x.StepNum ) );

                locked = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await MaintenanceItemManager.DeleteItem( item.Id );
                await Shell.Current.GoToAsync( $"..?{RoutingPath.Refresh}=true" );
            }
        }

        private async Task Edit()
        {
            if( item != null )
            {
                string encodedQuery = HttpUtility.UrlEncode( maintenanceItemId.ToString() );

                await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}?{nameof( maintenanceItemId )}={encodedQuery}" );
            }
        }

        private async Task Update()
        {
            if( item != null )
            {
                if( await DbServiceLocator.GetItemAsync<MaintenanceItem>( item.Id ) != item )
                {
                    await DbServiceLocator.UpdateItemAsync( item );
                }
            }
        }


        private async Task NewStep()
        {
            if( item == null )
            {
                maintenanceItemId = await MaintenanceItemManager.NewMaintenanceItem( Name, FirstServiceDate, Comment, RecursEvery, true, TimesServiced, notifyOfNextServiceDate: NotifyOfNextServiceDate );
                item = await MaintenanceItemManager.GetItemAsync( maintenanceItemId );
            }

            if( StepViewModels.Count > 0 )
            {
                string encodedFinalStepId = HttpUtility.UrlEncode(StepViewModels[^1].Step.Id.ToString());
                string encodedMaintenanceItemId = HttpUtility.UrlEncode(maintenanceItemId.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?previousStepId={encodedFinalStepId}&{RoutingPath.MaintenanceItemId}={encodedMaintenanceItemId}" );
            }

            if( StepViewModels.Count < 1 )
            {
                string encodedQuery = HttpUtility.UrlEncode(true.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?isFirstStep={encodedQuery}" );
            }

        }

        #endregion

        #region Query Handling

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
                case nameof( stepIds ):
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out int stepId ) )
                    {
                        stepIds.Add( stepId );
                        await LoadSteps();
                    }
                    break;
                case nameof( maintenanceItemId ):
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out maintenanceItemId ) )
                    {
                        item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( maintenanceItemId ).ConfigureAwait( false );

                        InitData( item, true );
                    }
                    break;
                case RoutingPath.NewItem:
                    maintenanceItemId = await MaintenanceItemManager.NewMaintenanceItem( string.Empty, DateTime.Now, string.Empty, notifyOfNextServiceDate: true );
                    item = await MaintenanceItemManager.GetItemAsync( maintenanceItemId );
                    InitData( item );
                    editState = EditState.NewItem;
                    break;
            }
        }

        #endregion
    }
}

