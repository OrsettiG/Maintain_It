﻿using System;
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
using NUnit.Framework;
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

        public int ItemId
        {
            get => item.Id;
        }

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

        private DateTime previousServiceDate;
        public DateTime PreviousServiceDate
        {
            get => previousServiceDate;
            set => SetProperty( ref previousServiceDate, value );
        }

        private DateTime nextServiceDate = DateTime.Now;
        public DateTime NextServiceDate
        {
            get => nextServiceDate;
            set => SetProperty( ref nextServiceDate, value );
        }

        private bool isRecurring = false;
        public bool IsRecurring
        {
            get => isRecurring;
            set => SetProperty( ref isRecurring, value );
        }

        private int recursEvery;
        public int RecursEvery
        {
            get => recursEvery;
            set => SetProperty( ref recursEvery, ( value! > 0 && value! < 1000 ) ? value : 1 );
        }

        private Timeframe frequency = Timeframe.Months;
        public Timeframe Frequency
        {
            get => frequency;
            set => SetProperty( ref frequency, value );
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

        #region QUERY PARAMS
        private readonly List<int> stepIds = new List<int>();
        private int maintenanceItemId;
        private bool update = false;
        #endregion

        private ObservableRangeCollection<StepViewModel> _stepViewModels;
        public ObservableRangeCollection<StepViewModel> StepViewModels
        {
            get => _stepViewModels ??= new ObservableRangeCollection<StepViewModel>();
            set => SetProperty( ref _stepViewModels, value.OrderBy( x => x.StepNum ) as ObservableRangeCollection<StepViewModel> );
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
                string encodedQuery = HttpUtility.UrlEncode( maintenanceItemId.ToString() );

                //await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListFromMaintenanceItemView )}?{nameof( maintenanceItemId )}={encodedFinalStepId}" );
            }
        }

        private async Task Add()
        {

            if( update )
            {
                await MaintenanceItemManager.UpdateProperties( item.Id, name: name, comment: comment, firstServiceDate: firstServiceDate, recursEvery: recursEvery, timeframe: (int)frequency, notifyOfNextServiceDate: notifyOfNextServiceDate );

                await MaintenanceItemManager.UpdateSteps( item.Id, stepIds );
            }
            else
            {

                int id = await MaintenanceItemManager.NewMaintenanceItem( Name, FirstServiceDate, Comment, RecursEvery, (int)Frequency, NotifyOfNextServiceDate );

                await MaintenanceItemManager.AddSteps( id, stepIds );

            }

            ClearData();
            await Shell.Current.GoToAsync( $"//{nameof( HomeView )}?Refresh=true" );
        }

        private async Task<List<Step>> CreateStepList()
        {
            List<Step> list = new List<Step>();

            foreach( StepViewModel model in StepViewModels )
            {
                //await MainThread.InvokeOnMainThreadAsync( () =>
                //  model.SaveStep() );

                list.Add( model.Step );
            };

            return list;
        }

        private List<StepViewModel> CreateStepViewModelList( List<Step> stepList )
        {
            StepViewModel[] vms = new StepViewModel[stepList.Count];

            for( int i = 0; i < stepList.Count; i++ )
            {
                StepViewModel vm = new StepViewModel( this )
                {
                    Step = stepList[i]
                };

                vm.Init();

                vms[i] = vm;
            };

            return vms.OrderBy( x => x.StepNum ).ToList();
        }

        private async Task<StepViewModel> CreateNewStepViewModel( Step step )
        {
            StepViewModel vm = await MainThread.InvokeOnMainThreadAsync(() =>
                new StepViewModel(this)
                {
                    Step = step
                }
            );

            vm.Init();

            return vm;
        }

        private void InitData( MaintenanceItem maintenanceItem, bool _update = false )
        {
            item = maintenanceItem;

            if( _update )
            {
                update = true;
            }

            Name = item.Name;
            Comment = item.Comment;
            FirstServiceDate = item.FirstServiceDate;
            RecursEvery = item.RecursEvery;

            Frequency = (Timeframe)item.Timeframe;
            TimesServiced = item.ServiceRecords.Count;
            PreviousServiceCompleted = TimesServiced > 0 && item.ServiceRecords[^1].ServiceCompleted;
            NotifyOfNextServiceDate = item.NotifyOfNextServiceDate;

            stepIds.AddRange( GetStepIds() );

            _ = Task.Run( async () => await RefreshSteps() );
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

                List<Step> newStepList = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>( stepIds ) as List<Step>;

                List<StepViewModel> data = new List<StepViewModel>();
                foreach( Step step in newStepList )
                {

                    try
                    {
                        Task<StepViewModel> task = CreateNewStepViewModel( step );
                        data.Add( task.Result );
                    }
                    catch( AggregateException ex )
                    {
                        Console.WriteLine( $"EXEPTION: {ex.InnerExceptions}" );
                    }

                }

                StepViewModels.AddRange( data.OrderBy( x => x.StepNum ) );

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
                    data.Add( await CreateNewStepViewModel( step ) );
                }

                StepViewModels.AddRange( data.OrderBy( x => x.StepNum ) );

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
            if( item == null )
            {
                maintenanceItemId = await MaintenanceItemManager.NewMaintenanceItem( Name, FirstServiceDate, Comment, RecursEvery, TimesServiced, NotifyOfNextServiceDate );
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
            }
        }

        #endregion
    }
}

