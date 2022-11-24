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

namespace Maintain_it.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            _ = Task.Run( async () => await Refresh() );
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
            set => SetProperty(ref showSuspendedFilterFlag, value );
        }

        // --

        // IStorableObject
        private DateTime creationDateFilterRangeStart;
        public DateTime CreationDateFilterRangeStart
        {
            get => creationDateFilterRangeStart;
            set => SetProperty( ref creationDateFilterRangeStart, value );
        }

        private DateTime creationDateFilterRangeEnd;
        public DateTime CreationDateFilterRangeEnd
        {
            get => creationDateFilterRangeEnd;
            set => SetProperty( ref creationDateFilterRangeEnd, value );
        }
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
        private DateTime nextServiceDateFilterRangeStart;
        public DateTime NextServiceDateFilterRangeStart
        {
            get => nextServiceDateFilterRangeStart;
            set => SetProperty( ref nextServiceDateFilterRangeStart, value );
        }

        private DateTime nextServiceDateFilterRangeEnd;
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

        private bool showFilters;
        public bool ShowFilters
        {
            get => showFilters;
            set => SetProperty( ref showFilters, value );
        }

        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );


        private Command toggleFilterCommand;
        public ICommand ToggleFilterCommand
        {
            get => toggleFilterCommand ??= new Command( ToggleFilter );
        }

        private void ToggleFilter()
        {
            if( ShowFilters )
            {
                Console.WriteLine( "Close Filters" );
            }
            else
            { 
                Console.WriteLine( "Open Filters" ); 
            }

            ShowFilters = !ShowFilters;
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

                DisplayedMaintenanceItems.Clear();
                DisplayedMaintenanceItems.AddRange( vms );
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
