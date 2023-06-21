using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;
using Maintain_it.Services;

namespace Maintain_it.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Task task = Task.Run( async () => await Refresh() );
        }

        #region PROPERTIES

        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if( SetProperty( ref searchTerm, value, validateValue: ValidateString ) )
                {
                    DisplayedMaintenanceItems.Clear();
                    DisplayedMaintenanceItems.AddRange( allServiceItemViewModels.Where( x => PassesSearchTerm( x ) && PassesFilters( x ) ) );
                }
            }
        }

        #region Filters
        private bool showFilters;
        public bool ShowFilters
        {
            get => showFilters;
            set => SetProperty( ref showFilters, value );
        }

        // Notifications
        private bool showActiveFilterFlag = true;
        public bool ShowActiveFilterFlag
        {
            get => showActiveFilterFlag;
            set => SetProperty( ref showActiveFilterFlag, value );
        }

        private bool showInactiveFilterFlag = false;
        public bool ShowInactiveFilterFlag
        {
            get => showInactiveFilterFlag;
            set => SetProperty( ref showInactiveFilterFlag, value );
        }

        private bool showSuspendedFilterFlag = true;
        public bool ShowSuspendedFilterFlag
        {
            get => showSuspendedFilterFlag;
            set => SetProperty( ref showSuspendedFilterFlag, value );
        }

        // --

        // IStorableObject

        // --

        // Service Record Summary
        private DateTime lastServiceDateFilterRangeStart;
        public DateTime LastServiceDateFilterRangeStart
        {
            get => lastServiceDateFilterRangeStart;
            set => SetProperty( ref lastServiceDateFilterRangeStart, value );
        }

        private DateTime lastServiceDateFilterRangeEnd;
        public DateTime LastServiceDateFilterRangeEnd
        {
            get => lastServiceDateFilterRangeEnd;
            set => SetProperty( ref lastServiceDateFilterRangeEnd, value );
        }
        // --

        // Life Expectancy
        private int lifeExpectancyFilter;
        public int LifeExpectancyFilter
        {
            get => lifeExpectancyFilter;
            set => SetProperty( ref lifeExpectancyFilter, value );
        }

        private Timeframe lifeExpectancyTimeframeFilter;
        public Timeframe LifeExpectancyTimeframeFilter
        {
            get => lifeExpectancyTimeframeFilter;
            set => SetProperty( ref lifeExpectancyTimeframeFilter, value );
        }

        private int remainingLifeExpectancyFilter;
        public int RemainingLifeExpectancyFilter
        {
            get => remainingLifeExpectancyFilter;
            set => SetProperty( ref remainingLifeExpectancyFilter, value );
        }

        private Timeframe remainingLifeExpectancyTimeframeFilter;
        public Timeframe RemainingLifeExpectancyTimeframeFilter
        {
            get => remainingLifeExpectancyTimeframeFilter;
            set => SetProperty( ref remainingLifeExpectancyTimeframeFilter, value );
        }
        // --

        // Service Schedule

        // Next Service Date Filters
        private bool useNextServiceDateFilters = true;
        public bool UseNextServiceDateFilters
        {
            get => useNextServiceDateFilters;
            set => SetProperty( ref useNextServiceDateFilters, value );
        }

        private bool showOverdue = true;
        public bool ShowOverdue_NextServiceDateFilters
        {
            get => showOverdue;
            set => SetProperty( ref showOverdue, value );
        }

        private DateTime nextServiceDateFilterRangeStart = DateTime.UtcNow.ToLocalTime();
        public DateTime NextServiceDateFilterRangeStart
        {
            get => nextServiceDateFilterRangeStart;
            set => SetProperty( ref nextServiceDateFilterRangeStart, value );
        }

        private DateTime nextServiceDateFilterRangeEnd = DateTime.UtcNow.ToLocalTime().AddDays( 7 );
        public DateTime NextServiceDateFilterRangeEnd
        {
            get => nextServiceDateFilterRangeEnd;
            set => SetProperty( ref nextServiceDateFilterRangeEnd, value );
        }

        private BoolianFilterFlag hasServiceLimitFilter;
        public BoolianFilterFlag HasServiceLimitFilter
        {
            get => hasServiceLimitFilter;
            set => SetProperty( ref hasServiceLimitFilter, value );
        }

        // Step Summary Data
        private int estimatedServiceCompletionTimeFilterRangeStart;
        public int EstimatedServiceCompletionTimeFilterRangeStart
        {
            get => estimatedServiceCompletionTimeFilterRangeStart;
            set => SetProperty( ref estimatedServiceCompletionTimeFilterRangeStart, value );
        }

        private int estimatedServiceCompletionTimeFilterRangeEnd;
        public int EstimatedServiceCompletionTimeFilterRangeEnd
        {
            get => estimatedServiceCompletionTimeFilterRangeEnd;
            set => SetProperty( ref estimatedServiceCompletionTimeFilterRangeEnd, value );
        }

        private Timeframe estimatedServiceCompletionTimeFilterRangeStartTimeframe;
        public Timeframe EstimatedServiceCompletionTimeFilterRangeStartTimeframe
        {
            get => estimatedServiceCompletionTimeFilterRangeStartTimeframe;
            set => SetProperty( ref estimatedServiceCompletionTimeFilterRangeStartTimeframe, value );
        }

        private Timeframe estimatedServiceCompletionTimeFilterRangeEndTimeframe;
        public Timeframe EstimatedServiceCompletionTimeFilterRangeEndTimeframe
        {
            get => estimatedServiceCompletionTimeFilterRangeEndTimeframe;
            set => SetProperty( ref estimatedServiceCompletionTimeFilterRangeEndTimeframe, value );
        }

        // Creation Date Filters
        private bool useCreationDateFilters = false;
        public bool UseCreationDateFilters
        {
            get => useCreationDateFilters;
            set => SetProperty( ref useCreationDateFilters, value );
        }

        private bool showCompleted_CreationDateFilters;
        public bool ShowCompleted_CreationDateFilters
        {
            get => showCompleted_CreationDateFilters;
            set => SetProperty( ref showCompleted_CreationDateFilters, value );
        }

        private bool showOverdue_CreationDateFilters;
        public bool ShowOverdue_CreationDateFilters
        {
            get => showOverdue_CreationDateFilters;
            set => SetProperty( ref showOverdue_CreationDateFilters, value );
        }

        private DateTime creationDateFilterRangeStart = DateTime.UtcNow.ToLocalTime();
        public DateTime CreationDateFilterRangeStart
        {
            get => creationDateFilterRangeStart;
            set => SetProperty( ref creationDateFilterRangeStart, value );
        }

        private DateTime creationDateFilterRangeEnd = DateTime.UtcNow.ToLocalTime().AddDays( 7 );
        public DateTime CreationDateFilterRangeEnd
        {
            get => creationDateFilterRangeEnd;
            set => SetProperty( ref creationDateFilterRangeEnd, value );
        }

        #endregion Filters

        private ObservableRangeCollection<ServiceItem> maintenanceItems = new ObservableRangeCollection<ServiceItem>();

        private List<MaintenanceItemViewModel> allServiceItemViewModels = new List<MaintenanceItemViewModel>();

        private ObservableRangeCollection<MaintenanceItemViewModel> displayedMaintenanceItems;
        public ObservableRangeCollection<MaintenanceItemViewModel> DisplayedMaintenanceItems
        {
            get => displayedMaintenanceItems ??= new ObservableRangeCollection<MaintenanceItemViewModel>();
            set => SetProperty( ref displayedMaintenanceItems, value );
        }

        private bool Locked { get; set; } = false;
        public bool IsRefreshing => Locked;


        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );


        private Command toggleFiltersCommand;
        public ICommand ToggleFiltersCommand
        {
            get => toggleFiltersCommand ??= new Command( ToggleFilters );
        }

        private void ToggleFilters()
        {
            ShowFilters = !ShowFilters;
        }

        private Command applyFiltersCommand;
        public ICommand ApplyFiltersCommand
        {
            get => applyFiltersCommand ??= new Command( ApplyFilters );
        }
        private void ApplyFilters()
        {
            //await Refresh();

            // Create new List from allServiceViewModels with only the Active/Inactive/Suspended projects desired

            List<MaintenanceItemViewModel> filteredItems = allServiceItemViewModels.Where( x => ( x.ActiveState == ActiveStateFlag.Active && ShowActiveFilterFlag == true ) || ( x.ActiveState == ActiveStateFlag.Inactive && ShowInactiveFilterFlag == true ) || ( x.ActiveState == ActiveStateFlag.Suspended && ShowSuspendedFilterFlag == true ) ).ToList();
            // Iterate over the list and remove any items that do not match one or more filters
            // Next Service Date
            if( UseNextServiceDateFilters )
            {
                DateTime dateRangeStart = NextServiceDateFilterRangeStart.ToUniversalTime();
                DateTime dateRangeEnd = NextServiceDateFilterRangeEnd.ToUniversalTime();

                filteredItems = filteredItems.Where( x => ( DateTime.Compare( x.NextServiceDate.ToUniversalTime(), dateRangeStart ) >= 0 && DateTime.Compare( x.NextServiceDate.ToUniversalTime(), dateRangeEnd ) <= 0 ) || ( DateTime.Compare( x.NextServiceDate.ToUniversalTime(), dateRangeStart ) < 0 && x.Item.ServiceRecords.Last().ServiceCompleted == false ) ).ToList();

                //TODO: Add an option to filter by only completed items.

                // If the user doesn't want to see overdue items we need to remove them from the results
                if( !ShowOverdue_NextServiceDateFilters )
                {
                    filteredItems = filteredItems.Where( x => x.NextServiceDate.ToLocalTime() >= DateTime.UtcNow.ToLocalTime() ).ToList();
                }
            }

            // Date Created
            if( UseCreationDateFilters )
            {
                DateTime dateRangeStart = CreationDateFilterRangeStart.ToUniversalTime();
                DateTime dateRangeEnd = CreationDateFilterRangeEnd.ToUniversalTime();

                filteredItems = filteredItems.Where( x => x.CreatedOn.ToUniversalTime() >= dateRangeStart && x.CreatedOn.ToUniversalTime() <= dateRangeEnd ).ToList();
            }
            // Date Last Completed
            // etc)

            // Clear DisplayedItems ObservableRangeCollection
            DisplayedMaintenanceItems.Clear();
            DisplayedMaintenanceItems.AddRange( filteredItems );
            // Add new List to maintenanceItems Collection

            if( ShowFilters )
            {
                ToggleFilters();
            }
        }
        #endregion

        #region Methods

        private async Task Add()
        {
            await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}?{QueryParameters.NewItem}=true" );
        }


        private async Task Refresh()
        {
            if( !Locked )
            {
                Locked = true;
                List<ServiceItem> items = await ServiceItemManager.GetAllItemsRecursiveAsync();

                //List<MaintenanceItemViewModel> vms = CreateRange( items );
                Task<List<MaintenanceItemViewModel>> vmsTask = ServiceItemManager.GetItemRangeAsViewModelAsync( items.GetIds() );

                List<MaintenanceItemViewModel> vms = await vmsTask;

                maintenanceItems.Clear();
                maintenanceItems.AddRange( items );

                // Needed this to avoid loading race condidtion, might be able to remove it but don't want to open that can of worms right now.
                vmsTask.Wait();

                allServiceItemViewModels.Clear();
                allServiceItemViewModels.AddRange( vms );


                ApplyFilters();
                //DisplayedMaintenanceItems.Clear();
                //DisplayedMaintenanceItems.AddRange( vms );
            }

            Locked = false;
        }

        private bool PassesSearchTerm( MaintenanceItemViewModel vm )
        {
            return string.IsNullOrEmpty( SearchTerm ) || vm.Name.ToLowerInvariant().Contains( SearchTerm.ToLowerInvariant() );
        }

        private bool PassesFilters( MaintenanceItemViewModel vm )
        {
            return true;
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

        #endregion
    }
}
