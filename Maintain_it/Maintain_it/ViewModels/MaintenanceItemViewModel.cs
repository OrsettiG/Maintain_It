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

        //public MaintenanceItemViewModel( MaintenanceItem item ) => Item = item;
        #endregion

        #region PROPERTIES
        private MaintenanceItem item;
        public MaintenanceItem Item
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

        private DateTime firstServiceDate = DateTime.Now;
        public DateTime ServiceDate
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
            set
            {
                _ = SetProperty( ref hasServiceLimit, value );
                TimesToRepeatService = value ? 1 : 0;
            }
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

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetProperty( ref isActive, value );
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
            MaintenanceItem tempItem = await MaintenanceItemManager.GetItemAsync( ItemId );

            int[] ids = tempItem.Steps.GetIds().ToArray();

            stepIds.Clear();
            stepIds.AddRange( ids );

            List<StepViewModel> stepVMs = await StepManager.GetItemRangeAsViewModel( ids, this );

            StepViewModels.Clear();
            StepViewModels.AddRange( stepVMs.OrderBy( x => x.StepNum ) );
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
            _ = await MaintenanceItemManager.InsertServiceRecord( ItemId );

            string encodedId = HttpUtility.UrlEncode($"{ItemId}");

            await Shell.Current.GoToAsync( $"{nameof( PerformMaintenanceView )}?{QueryParameters.MaintenanceItemId}={encodedId}" );
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
            if( item != null )
            {
                // Parallel loop through all the materials to get the amounts needed.
                ConcurrentDictionary<int, int> materialIdsAndQuantitysRequired = await GetMaterialIdsAndQuantitysRequired();

                StringBuilder sb = new StringBuilder();

                // Add those materials into the query string using = and ; as separators
                BuildQueryString( materialIdsAndQuantitysRequired, sb );


                string encodedQuery = HttpUtility.UrlEncode( sb.ToString() );
                if( encodedQuery != string.Empty )
                {
                    string encodedName = HttpUtility.UrlEncode( $"{Item.Name} Shopping List" );

                    await Shell.Current.GoToAsync( $"{nameof( CreateNewShoppingListView )}?{QueryParameters.PreSelectedMaterialIds}={encodedQuery}&{QueryParameters.ItemName}={encodedName}" );
                }
                else
                {
                    // If the string is empty then there are no materials that we do not own. This doesn't check any projects other than the one the user is looking at.
                    await Shell.Current.DisplayAlert( Alerts.Information, Alerts.MaterialsAlreadyOwned, Alerts.Confirmation );
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

        private async Task SaveChanges()
        {
            if( IsActive != true )
            {
                IsActive = await Shell.Current.DisplayAlert( Alerts.SetProjectActive, Alerts.ProjectActiveStateMessage, accept: Alerts.Yes, cancel: Alerts.No );
            }

            if( Item != null )
            {
                ServiceDate = ServiceDate.AddHours( ServiceTime.Hours ).AddMinutes( ServiceTime.Minutes );

                await MaintenanceItemManager.UpdateProperties( Item.Id, name: Name, comment: Comment, firstServiceDate: ServiceDate, recursEvery: RecursEvery, timeframe: (int)ServiceTimeframe, hasServiceLimit: HasServiceLimit, timesToRepeatService: TimesToRepeatService, notifyOfNextServiceDate: NotifyOfNextServiceDate, advanceNotice: AdvanceNotice, advanceNoticeTimeframe: (int)NoticeTimeframe, reminders: TimesToRemind, steps: stepIds, isActive: IsActive );
            }
            else
            {
                ItemId = await MaintenanceItemManager.NewMaintenanceItem( Name, ServiceDate, Comment, RecursEvery, true, (int)ServiceTimeframe, notifyOfNextServiceDate: NotifyOfNextServiceDate, timesToRemind: TimesToRemind, advanceNotice: AdvanceNotice, noticeTimeframe: (int)NoticeTimeframe, stepIds: stepIds );
            }

            ClearData();

            await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
        }

        private void CalculateServiceCompletionTimeEstimate()
        {
            double minutes = 0;
            TimeInMinutes largestTimeframe = TimeInMinutes.None;

            foreach( Step step in Item.Steps )
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

        private async Task Init()
        {
            if( ItemId == 0 )
                return;

            Item = await MaintenanceItemManager.GetItemRecursiveAsync( ItemId );
            await LoadStepsAsync();
            Name = Item.Name;
            Comment = Item.Comment;
            IsActive = Item.IsActive;
            ServiceDate = Item.NextServiceDate ?? Item.FirstServiceDate;
            RecursEvery = Item.RecursEvery;
            ServiceTimeframe = (Timeframe)Item.ServiceTimeframe;
            HasServiceLimit = Item.HasServiceLimit;
            TimesToRepeatService = Item.TimesToRepeatService;
            NotifyOfNextServiceDate = Item.NotifyOfNextServiceDate;
            AdvanceNotice = Item.AdvanceNotice;
            NoticeTimeframe = (Timeframe)Item.NoticeTimeframe;
            CreatedOn = Item.CreatedOn;

            TimesServiced = Item.ServiceRecords.Count;
            PreviousServiceCompleted = TimesServiced > 0 && Item.ServiceRecords[^1].ServiceCompleted;

        }

        private async Task LoadStepsAsync()
        {
            stepIds.AddRange( Item.Steps.GetIds() );
            List<StepViewModel> stepVMs = await StepManager.GetItemRangeAsViewModel( stepIds, this );

            StepViewModels.Clear();
            StepViewModels.AddRange( stepVMs );
        }

        private void InitData( MaintenanceItem maintenanceItem, bool _update = false )
        {
            Item = maintenanceItem;

            Name = Item.Name;
            Comment = Item.Comment;
            ServiceDate = Item.NextServiceDate ?? Item.FirstServiceDate;
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
        }

        private void ClearData()
        {
            if( !Locked )
            {
                Locked = true;

                Name = string.Empty;
                Comment = string.Empty;
                ServiceDate = DateTime.UtcNow.ToLocalTime();
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

                List<StepViewModel> sVMs = await StepManager.GetItemRangeAsViewModel( stepIds, this );

                StepViewModels.Clear();
                StepViewModels.AddRange( sVMs.OrderBy( x => x.StepNum ) );
                CalculateServiceCompletionTimeEstimate();

                Locked = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await MaintenanceItemManager.DeleteItem( Item.Id );
                await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
            }
        }

        private async Task Edit()
        {
            if( ItemId != 0 )
            {
                string encodedQuery = HttpUtility.UrlEncode( ItemId.ToString() );

                await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}?{QueryParameters.MaintenanceItemId}={encodedQuery}" );
            }
        }

        // TODO: Make it so you can add a new step to a completed project. Got a null reference error when I tried.
        private async Task NewStep()
        {
            if( Item == null )
            {
                ItemId = await MaintenanceItemManager.NewMaintenanceItem( Name, ServiceDate, Comment, RecursEvery, true, TimesServiced, notifyOfNextServiceDate: NotifyOfNextServiceDate );
                Item = await MaintenanceItemManager.GetItemAsync( ItemId );
            }

            if( StepViewModels.Count > 0 )
            {
                string encodedFinalStepId = HttpUtility.UrlEncode(StepViewModels[^1].Step.Id.ToString());
                string encodedMaintenanceItemId = HttpUtility.UrlEncode(ItemId.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?previousStepId={encodedFinalStepId}&{QueryParameters.MaintenanceItemId}={encodedMaintenanceItemId}" );
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
                case QueryParameters.MaintenanceItemId:
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out itemId ) )
                    {
                        //item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( ItemId ).ConfigureAwait( false );
                        ItemId = itemId;
                        editState = EditState.Editing;

                        await Init();

                        //InitData( item );
                    }
                    break;
                case QueryParameters.NewItem:
                    ItemId = await MaintenanceItemManager.NewMaintenanceItem( string.Empty, DateTime.Now, string.Empty, notifyOfNextServiceDate: true );
                    item = await MaintenanceItemManager.GetItemAsync( ItemId );
                    InitData( item );
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

