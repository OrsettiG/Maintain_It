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

        public MaintenanceItemViewModel( int itemId )
        {
            ItemId = itemId;
        }

        //public MaintenanceItemViewModel( ServiceItem item ) => Item = item;
        #endregion

        #region PROPERTIES
        private ServiceItem item;
        public ServiceItem Item
        {
            get => item;
            private set => SetProperty( ref item, value );
        }

        private int itemId;
        public int ItemId
        {
            get => Item?.Id ?? itemId;
            set => SetProperty( ref itemId, Item?.Id ?? value );
        }

        public List<Timeframe> Timeframes => Options.timeframes;

        public bool Locked { get; private set; }

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

        private DateTime nextServiceDate = DateTime.MinValue;
        public DateTime NextServiceDate
        {
            get => nextServiceDate;
            set => SetProperty( ref nextServiceDate, value );
        }

        public string NextServiceDateUIString
        {
            get
            {
                return NextServiceDate != DateTime.MinValue ? $"{NextServiceDate.DayOfWeek} {NextServiceDate.Day}/{NextServiceDate.Month}/{NextServiceDate.Year}" : "No Service Scheduled";
            }
        }

        private DateTime lastServiceDate = DateTime.MinValue;
        public DateTime LastServiceDate
        {
            get => lastServiceDate;
            set => SetProperty( ref lastServiceDate, value );
        }

        public string LastServiceDateUIString
        {
            get
            {
                return LastServiceDate != DateTime.MinValue ? $"{LastServiceDate.DayOfWeek} {LastServiceDate.Day}/{LastServiceDate.Month}/{LastServiceDate.Year}" : "No Service Completed";
            }
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
            set
            {
                _ = SetProperty( ref hasServiceLimit, value );
                TimesToRepeatService = value ? 1 : 0;
            }
        }

        private int timesToRepeatService = 0;
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

        private ActiveStateFlag activeState;
        public ActiveStateFlag ActiveState
        {
            get => activeState;
            set => SetProperty( ref activeState, value );
        }

        public Color ActiveStateColor
        {
            get
            {
                return ActiveState switch
                {
                    ActiveStateFlag.Inactive => (Color)App.Current.Resources["Secondary"],
                    ActiveStateFlag.Suspended => (Color)App.Current.Resources["Warning"],
                    ActiveStateFlag.Active => (Color)App.Current.Resources["Cue"],
                    _ => (Color)App.Current.Resources["Cue"],
                };
            }
        }

        private DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn.ToLocalTime();
            set => SetProperty( ref createdOn, value.ToUniversalTime() );
        }

        //TODO: Update all the query params here to use the QueryParameters constants instead.
        #region QUERY PARAMS
        private enum EditState { NewItem, Editing }
        private EditState editState = EditState.NewItem;
        private readonly List<int> stepIds = new List<int>();
        #endregion

        private ObservableRangeCollection<StepViewModel> stepViewModels;
        public ObservableRangeCollection<StepViewModel> StepViewModels
        {
            get => stepViewModels ??= new ObservableRangeCollection<StepViewModel>();
            set => SetProperty( ref stepViewModels, value.OrderBy( x => x.StepNum ) as ObservableRangeCollection<StepViewModel> );
        }

        public string CompletionTimeEstimate
        {
            get
            {
                //if( completionTimeEstimate == null )
                //{
                //    CalculateServiceCompletionTimeEstimate();
                //}

                return Item.ServiceCompletionTimeEst > 0 ? $"{Item.ServiceCompletionTimeEst} {(TimeInMinutes)Item.ServiceCompletionTimeEstTimeframe}" : "Unavailable";
            }
        }

        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public AsyncCommand SaveChangesAsyncCommand => addCommand ??= new AsyncCommand( SaveChanges );

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
                    await SaveChanges();
                    break;
                case Alerts.Discard:
                    // Delete sends the user back to their previous page, so we only need to call GoToAsync if they are discarding changes to an existing item
                    if( editState == EditState.NewItem )
                        await Delete();
                    else
                        await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
                    break;
            }

        }

        private AsyncCommand editCommand;
        public AsyncCommand EditCommand => editCommand ??= new AsyncCommand( Edit );

        private AsyncCommand refreshStepsCommand;
        public AsyncCommand RefreshStepsCommand
        {
            get => refreshStepsCommand ??= new AsyncCommand( RefreshStepsFromDb );
        }

        private async Task RefreshStepsFromDb()
        {
            ServiceItem tempItem = await ServiceItemManager.GetItemRecursiveAsync( ItemId );

            int[] ids = tempItem.Steps.GetIds().ToArray();

            if( ids.Length > 0 )
            {
                stepIds.Clear();
                stepIds.AddRange( ids );

                List<StepViewModel> stepVMs = await StepManager.GetItemRangeAsComplexViewModel( ids, this );

                StepViewModels.Clear();
                StepViewModels.AddRange( stepVMs.OrderBy( x => x.StepNum ) );
            }
            else
            {
                stepIds.Clear();
                StepViewModels.Clear();
            }
            //else if( StepViewModels.RemainingItemsCount > 0 )
            //{
            //    foreach( StepViewModel svm in StepViewModels )
            //    {
            //        svm.RefreshAllCommand.Execute(null);
            //    }
            //}
        }

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
            if( Item.ServiceRecords.Count == 0 || Item.ServiceRecords.Last().ServiceCompleted )
            {
                _ = await ServiceItemManager.InsertServiceRecord( ItemId );
            }

            string encodedId = HttpUtility.UrlEncode($"{ItemId}");

            await Shell.Current.GoToAsync( $"{nameof( PerformMaintenanceView )}?{QueryParameters.ServiceItemId}={encodedId}" );
        }

        private AsyncCommand initCommand;
        public AsyncCommand InitCommand
        {
            get => initCommand ??= new AsyncCommand( Init );
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
            if( Item != null )
            {
                bool hasMissingMats = await HasMissingMaterials();
                //// Parallel loop through all the looseMaterials to get the amounts needed.
                //ConcurrentDictionary<int, int> materialIdsAndQuantitysRequired = await GetMaterialIdsAndQuantitysRequired();

                //StringBuilder sb = new StringBuilder();

                //// Add those looseMaterials into the query string using = and ; as separators
                //BuildQueryString( materialIdsAndQuantitysRequired, sb );


                //string encodedQuery = HttpUtility.UrlEncode( sb.ToString() );

                if( hasMissingMats )
                {
                    await CreateOrAddServiceItemToShoppingList();

                }
                else
                {
                    // If the string is empty then there are no looseMaterials that we do not own. This doesn't check any projects other than the one the user is looking at.
                    bool choice = await Shell.Current.DisplayAlert( Alerts.Information, Alerts.MaterialsAlreadyOwned, Alerts.Yes, Alerts.Cancel );

                    switch( choice )
                    {
                        case true:
                            await CreateOrAddServiceItemToShoppingList();
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private async Task CreateOrAddServiceItemToShoppingList()
        {
            Dictionary<string, int> ActiveShoppingListNamesAndIds = await ShoppingListManager.GetActiveShoppingListNamesAndIds();

            string value = await Shell.Current.DisplayActionSheet( Alerts.AddToShoppingListTitle, Alerts.Cancel, Alerts.CreateNew, ActiveShoppingListNamesAndIds.Keys.ToArray() );

            if( value != string.Empty && value != null )
            {
                switch( value )
                {
                    case Alerts.Cancel:
                        break;
                    case Alerts.CreateNew:
                        string encodedName = HttpUtility.UrlEncode( $"{Item.Name} Shopping List" );
                        string encodedId = HttpUtility.UrlEncode($"{Item.Id}");
                        await Shell.Current.GoToAsync( $"{nameof( CreateNewShoppingListView )}?{QueryParameters.NewItem}={true}&{QueryParameters.ServiceItemId}={encodedId}&{QueryParameters.ItemName}={encodedName}" );
                        break;
                    default:
                        if( ActiveShoppingListNamesAndIds.ContainsKey( value ) )
                            //await ShoppingListManager.AddShoppingListItemsToListAsync( ActiveShoppingListNamesAndIds[value], materialIdsAndQuantitysRequired );
                            await ShoppingListManager.AddServiceItemToShoppingList( ActiveShoppingListNamesAndIds[value], Item.Id );
                        break;
                }
            }
        }

        private static void BuildQueryString( ConcurrentDictionary<int, int> materialIdsAndQuantitysRequired, StringBuilder sb )
        {
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
        }

        private async Task<ConcurrentDictionary<int, int>> GetMaterialIdsAndQuantitysRequired()
        {
            ConcurrentDictionary<int, int> materialIdsAndQuantitysRequired = new ConcurrentDictionary<int, int>();

            foreach( Step step in Item.Steps )
            {
                Step s = await StepManager.GetItemRecursiveAsync(step.Id);

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

            return materialIdsAndQuantitysRequired;
        }

        private async Task<bool> HasMissingMaterials()
        {
            bool hasMissingMaterials = false;

            foreach( Step step in Item.Steps )
            {
                if( !hasMissingMaterials )
                {
                    Step s = await StepManager.GetItemRecursiveAsync( step.Id );

                    ParallelLoopResult loop = Parallel.ForEach( s.StepMaterials, (x, state) =>
                    {
                        if( !state.ShouldExitCurrentIteration )
                        {
                            if( x.Quantity > x.Material.QuantityOwned )
                            {
                                hasMissingMaterials = true;
                                state.Break();
                            }
                        }

                        return;
                    } );

                    if( !loop.IsCompleted )
                    {
                        Console.WriteLine( $"Loop did not run to completion, hasMissingMaterials value: {hasMissingMaterials}" );

                        if( hasMissingMaterials )
                            break;
                    }

                    Console.WriteLine( $"Loop Ran to completion. hasMissingMaterials value: {hasMissingMaterials}" );
                }
            }

            return hasMissingMaterials;
        }

        private async Task SaveChanges()
        {
            if( ActiveState != ActiveStateFlag.Active )
            {
                ActiveState = ActiveState switch
                {
                    ActiveStateFlag.Inactive => await Shell.Current.DisplayAlert( Alerts.SetProjectActive, Alerts.UpdateProjectActiveState_InactiveMessage, accept: Alerts.Yes, cancel: Alerts.No ) ? ActiveStateFlag.Active : ActiveStateFlag.Inactive,

                    ActiveStateFlag.Suspended => await Shell.Current.DisplayAlert( Alerts.SetProjectActive, Alerts.UpdateProjectActiveState_SuspendedMessage, accept: Alerts.Yes, cancel: Alerts.No ) ? ActiveStateFlag.Active : ActiveStateFlag.Suspended
                };
            }

            if( Item != null )
            {
                NextServiceDate = NextServiceDate.AddHours( ServiceTime.Hours ).AddMinutes( ServiceTime.Minutes );

                await ServiceItemManager.UpdateProperties( Item.Id,
                                                                name: Name,
                                                                comment: Comment,
                                                                serviceDate: NextServiceDate,
                                                                recursEvery: RecursEvery,
                                                                timeframe: (int)ServiceTimeframe,
                                                                hasServiceLimit: HasServiceLimit,
                                                                timesToRepeatService: TimesToRepeatService,
                                                                notifyOfNextServiceDate: NotifyOfNextServiceDate,
                                                                advanceNotice: AdvanceNotice,
                                                                advanceNoticeTimeframe: (int)NoticeTimeframe,
                                                                reminders: TimesToRemind,
                                                                steps: stepIds,
                                                                isActive: (int)ActiveState );

            }
            else
            {
                ItemId = await ServiceItemManager.NewMaintenanceItem( name: Name, firstServiceDate: NextServiceDate, comment: Comment, recursEvery: RecursEvery, hasServiceLimit: HasServiceLimit, timesToRepeatService: TimesToRepeatService, serviceTimeframe: (int)ServiceTimeframe, notifyOfNextServiceDate: NotifyOfNextServiceDate, timesToRemind: TimesToRemind, advanceNotice: AdvanceNotice, noticeTimeframe: (int)NoticeTimeframe, stepIds: stepIds );
            }

            ClearData();

            await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
        }

        private async Task Init()
        {
            if( ItemId == 0 )
                return;

            Item = await ServiceItemManager.GetItemRecursiveAsync( ItemId );
            await LoadStepsAsync();

            // IStorableObject
            Name = Item.Name;
            CreatedOn = Item.CreatedOn;

            // Information
            Comment = Item.Comment;

            // Service Schedule
            NextServiceDate = Item.NextServiceDate;
            IsRecurring = RecursEvery > 0;
            RecursEvery = Item.RecursEvery;
            ServiceTimeframe = (Timeframe)Item.ServiceTimeframe;
            HasServiceLimit = Item.HasServiceLimit;
            TimesToRepeatService = Item.TimesToRepeatService;

            // Notifications
            ActiveState = (ActiveStateFlag)Item.ActiveState;
            NotifyOfNextServiceDate = Item.NotifyOfNextServiceDate;
            AdvanceNotice = Item.AdvanceNotice;
            NoticeTimeframe = (Timeframe)Item.NoticeTimeframe;

            // Life Expectancy

            // Step Data Summary

            // Service Record Summary
            PreviousServiceCompleted = TimesServiced > 0 && Item.ServiceRecords[^1].ServiceCompleted;
            TimesServiced = Item.ServiceRecords.Count;
            LastServiceDate = Item.ServiceRecords.Count > 0 ? Item.ServiceRecords.Where( x => x.ServiceCompleted == true ).OrderByDescending( x => x.ActualServiceCompletionDate ).First().ActualServiceCompletionDate : DateTime.MinValue;

            // Steps

            // Service Records



        }

        private async Task LoadStepsAsync()
        {
            stepIds.AddRange( Item.Steps.GetIds() );
            List<StepViewModel> stepVMs = await StepManager.GetItemRangeAsComplexViewModel( stepIds, this );

            StepViewModels.Clear();
            StepViewModels.AddRange( stepVMs );
        }

        private void InitData( ServiceItem maintenanceItem, bool _update = false )
        {
            Item = maintenanceItem;

            Name = Item.Name;
            Comment = Item.Comment;
            NextServiceDate = Item.NextServiceDate;
            RecursEvery = Item.RecursEvery;
            IsRecurring = Item.RecursEvery > 0;
            ServiceTimeframe = (Timeframe)Item.ServiceTimeframe;
            TimesToRepeatService = Item.TimesToRepeatService;
            NotifyOfNextServiceDate = Item.NotifyOfNextServiceDate;
            AdvanceNotice = Item.AdvanceNotice;
            NoticeTimeframe = (Timeframe)Item.NoticeTimeframe;
            TimesServiced = Item.ServiceRecords.Count;
            PreviousServiceCompleted = TimesServiced > 0 && Item.ServiceRecords[^1].ServiceCompleted;
            NotifyOfNextServiceDate = Item.NotifyOfNextServiceDate;
            ActiveState = (ActiveStateFlag)Item.ActiveState;
        }

        private void ClearData()
        {
            if( !Locked )
            {
                Locked = true;

                Name = string.Empty;
                Comment = string.Empty;
                NextServiceDate = DateTime.UtcNow.ToLocalTime();
                IsRecurring = false;
                RecursEvery = 0;
                ServiceTimeframe = Timeframe.Months;
                TimesServiced = 0;
                PreviousServiceCompleted = false;
                NotifyOfNextServiceDate = false;
                StepViewModels.Clear();
                item = null;

                Locked = false;
            }
        }

        private async Task LoadSteps()
        {
            if( !Locked )
            {
                Locked = true;

                List<StepViewModel> sVMs = await StepManager.GetItemRangeAsComplexViewModel( stepIds, this );

                StepViewModels.Clear();
                StepViewModels.AddRange( sVMs.OrderBy( x => x.StepNum ) );
                //CalculateServiceCompletionTimeEstimate();

                Locked = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await ServiceItemManager.DeleteItem( Item.Id );
                await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
            }
        }

        private async Task Edit()
        {
            if( ItemId != 0 )
            {
                string encodedQuery = HttpUtility.UrlEncode( ItemId.ToString() );

                await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}?{QueryParameters.ServiceItemId}={encodedQuery}" );
            }
        }

        // TODO: Make it so you can add a new step to a completed project. Got a null reference error when I tried.
        private async Task NewStep()
        {
            if( Item == null )
            {
                ItemId = await ServiceItemManager.NewMaintenanceItem( Name, NextServiceDate, Comment, RecursEvery, true, TimesServiced, notifyOfNextServiceDate: NotifyOfNextServiceDate );
                Item = await ServiceItemManager.GetItemAsync( ItemId );
            }
            string encodedMaintenanceItemId = HttpUtility.UrlEncode(ItemId.ToString());

            if( StepViewModels.Count > 0 )
            {
                string encodedFinalStepId = HttpUtility.UrlEncode(StepViewModels[^1].Step.Id.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?previousStepId={encodedFinalStepId}&{QueryParameters.ServiceItemId}={encodedMaintenanceItemId}" );
            }

            if( StepViewModels.Count < 1 )
            {
                string encodedQuery = HttpUtility.UrlEncode(true.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?isFirstStep={encodedQuery}&{QueryParameters.ServiceItemId}={encodedMaintenanceItemId}" );
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
                case QueryParameters.ServiceItemId:
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out itemId ) )
                    {
                        ItemId = itemId;
                        editState = EditState.Editing;

                        await Init();

                        //InitData( item );
                    }
                    break;
                case QueryParameters.NewItem:
                    ItemId = await ServiceItemManager.NewMaintenanceItem( string.Empty, DateTime.UtcNow, string.Empty, notifyOfNextServiceDate: true );
                    await Init();
                    //item = await ServiceItemManager.GetItemAsync( ItemId );
                    //InitData( item );
                    editState = EditState.NewItem;
                    break;
                case QueryParameters.RefreshSteps:
                    await RefreshStepsFromDb();
                    break;
            }
        }

        #endregion
    }
}

