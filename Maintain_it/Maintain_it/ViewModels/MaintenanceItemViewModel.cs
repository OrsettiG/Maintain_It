using System;
using System.Web;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Maintain_it.Views;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;
using MvvmHelpers;


using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;
using System.Collections.Concurrent;
using System.Linq;
using NUnit.Framework;

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

        private string name = string.Empty;
        public string Name { get => name; set => SetProperty( ref name, value ); }

        private string comment;
        public string Comment { get => comment; set => SetProperty( ref comment, value ); }

        private DateTime firstServiceDate = DateTime.Now;
        public DateTime FirstServiceDate { get => firstServiceDate; set => SetProperty( ref firstServiceDate, value ); }

        private DateTime previousServiceDate;
        public DateTime PreviousServiceDate { get => previousServiceDate; set => SetProperty( ref previousServiceDate, value ); }

        private DateTime nextServiceDate = DateTime.Now;
        public DateTime NextServiceDate { get => nextServiceDate; set => SetProperty( ref nextServiceDate, value ); }

        private bool isRecurring = false;
        public bool IsRecurring { get => isRecurring; set => SetProperty( ref isRecurring, value ); }

        private int recursEvery = 1;
        public int RecursEvery { get => recursEvery; set => SetProperty( ref recursEvery, ( value! < 0 && value! > 1000 ) ? value : 1 ); }

        private Timeframe frequency = Timeframe.Months;
        public Timeframe Frequency { get => frequency; set => SetProperty( ref frequency, value ); }

        private int timesServiced;
        public int TimesServiced { get => timesServiced; set => SetProperty( ref timesServiced, value ); }

        private bool previousServiceCompleted;
        public bool PreviousServiceCompleted { get => previousServiceCompleted; set => SetProperty( ref previousServiceCompleted, value ); }

        private bool notifyOfNextServiceDate = true;
        public bool NotifyOfNextServiceDate { get => notifyOfNextServiceDate; set => SetProperty( ref notifyOfNextServiceDate, value ); }

        #region QUERY PARAMS
        private readonly List<int> stepIds = new List<int>();
        private int maintenanceItemId;
        private bool update = false;
        #endregion

        private ObservableRangeCollection<StepViewModel> _stepViewModels;
        public ObservableRangeCollection<StepViewModel> StepViewModels { get => _stepViewModels ??= new ObservableRangeCollection<StepViewModel>(); set => SetProperty( ref _stepViewModels, value ); }


        private HomeViewModel _homeViewModel;
        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public AsyncCommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand deleteCommand;
        public AsyncCommand DeleteCommand => deleteCommand ??= new AsyncCommand( Delete );

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

        private async Task Add()
        {

            if( update )
            {

                item.Name = name;
                item.Comment = comment;
                item.FirstServiceDate = firstServiceDate;
                item.PreviousServiceDate = previousServiceDate;
                item.NextServiceDate = nextServiceDate;
                item.IsRecurring = isRecurring;
                item.RecursEvery = recursEvery;
                item.Frequency = (int)frequency;
                item.TimesServiced = timesServiced;
                item.PreviousServiceCompleted = previousServiceCompleted;
                item.NotifyOfNextServiceDate = notifyOfNextServiceDate;
                item.Steps = CreateStepList();

                await DbServiceLocator.UpdateItemAsync( item );
            }
            else
            {
                item = new MaintenanceItem()
                {
                    Name = name,
                    Comment = comment,
                    FirstServiceDate = firstServiceDate,
                    PreviousServiceDate = previousServiceDate,
                    NextServiceDate = nextServiceDate,
                    IsRecurring = isRecurring,
                    RecursEvery = recursEvery,
                    Frequency = (int)frequency,
                    TimesServiced = timesServiced,
                    PreviousServiceCompleted = previousServiceCompleted,
                    NotifyOfNextServiceDate = notifyOfNextServiceDate,
                    Steps = CreateStepList()
                };

                await DbServiceLocator.AddItemAsync( item );
            }

            ClearData();
            await Shell.Current.GoToAsync( $"//{nameof( HomeView )}?Refresh=true" );
        }

        private List<Step> CreateStepList()
        {
            ConcurrentBag<Step> bag = new ConcurrentBag<Step>();

            _ = Parallel.ForEach( StepViewModels, step =>
             {
                 bag.Add( step.Step );
             } );

            return bag.ToList();
        }

        private List<StepViewModel> CreateStepViewModelList( List<Step> stepList )
        {
            ConcurrentBag<StepViewModel> bag = new ConcurrentBag<StepViewModel>();
            StepViewModel[] vms = new StepViewModel[stepList.Count];

            for( int i = 0; i < stepList.Count; i++ )
            {
                vms[i] = new StepViewModel()
                {
                    Step = stepList[i] 
                };
            }

            _ = Parallel.ForEach( vms, vm =>
             {
                 vm.Init();

                 bag.Add( vm );
             } );

            return bag.OrderBy(x => x.StepNum).ToList();
        }

        private void InitData( MaintenanceItem maintenanceItem, bool _update = false )
        {
            item = maintenanceItem;

            if( _update )
            {
                update = true;
            }

            Name = maintenanceItem.Name;
            Comment = maintenanceItem.Comment;
            FirstServiceDate = maintenanceItem.FirstServiceDate;
            PreviousServiceDate = maintenanceItem.PreviousServiceDate;
            NextServiceDate = maintenanceItem.NextServiceDate;
            IsRecurring = maintenanceItem.IsRecurring;
            RecursEvery = maintenanceItem.RecursEvery;
            Frequency = (Timeframe)maintenanceItem.Frequency;
            TimesServiced = maintenanceItem.TimesServiced;
            PreviousServiceCompleted = maintenanceItem.PreviousServiceCompleted;
            NotifyOfNextServiceDate = maintenanceItem.NotifyOfNextServiceDate;
            
            RefreshSteps();
        }

        private void ClearData()
        {
            if( !locked )
            {
                locked = true;

                Name = string.Empty;
                Comment = string.Empty;
                FirstServiceDate = DateTime.Now;
                PreviousServiceDate = DateTime.Now;
                NextServiceDate = DateTime.Now.AddDays( 1 );
                IsRecurring = false;
                RecursEvery = 0;
                Frequency = Timeframe.Months;
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

                List<Step> stepList = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>( stepIds ) as List<Step>;

                List<StepViewModel> data = CreateStepViewModelList( stepList );
                
                StepViewModels.AddRange( data );

                locked = false;
            }
        }

        private void RefreshSteps()
        {
            if( !locked )
            {
                locked = true;

                StepViewModels.Clear();

                List<StepViewModel> svm = CreateStepViewModelList( item.Steps );

                StepViewModels.AddRange( svm );

                locked = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await DbServiceLocator.DeleteItemAsync<MaintenanceItem>( item.Id );
                await _homeViewModel.ItemDeleted( maintenanceItemId );
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
            if( StepViewModels.Count > 0 )
            {
                string encodedQuery = HttpUtility.UrlEncode(StepViewModels[^1].Step.Id.ToString());
                await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}?previousStepId={encodedQuery}" );
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
            }
        }

        #endregion
    }
}

