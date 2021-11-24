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

namespace Maintain_it.ViewModels
{
    public class MaintenanceItemViewModel : BaseViewModel
    {
        #region Constructors
        public MaintenanceItemViewModel()
        {
            _steps = new ObservableRangeCollection<Step>();
            newStepId = new List<int>();
        }

        //public MaintenanceItemViewModel( MaintenanceItem item ) => Item = item;
        #endregion

        #region PROPERTIES
        private MaintenanceItem item;

        public List<Timeframe> Timeframes => Options.timeframes;

        public bool IsBusy { get; private set; }

        private string name = "New Maintenance Item";
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
        private readonly List<int> newStepId;
        private readonly int maintenanceItemId;
        #endregion

        private ObservableRangeCollection<Step> _steps;
        public ObservableRangeCollection<Step> Steps { get => _steps; set => SetProperty( ref _steps, value ); }

        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public AsyncCommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand deleteCommand;
        public AsyncCommand DeleteCommand => deleteCommand ??= new AsyncCommand( Delete );

        private AsyncCommand refreshCommand;
        public AsyncCommand RefreshCommand => refreshCommand ??= new AsyncCommand( RefreshSteps );

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
                case nameof( newStepId ):
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out int stepId ) )
                    {
                        newStepId.Add( stepId );
                        await RefreshSteps();
                    }
                    break;
                case nameof( maintenanceItemId ):
                    if( int.TryParse( HttpUtility.UrlDecode( kvp.Value ), out int miId ) )
                    {
                        //Fetch MaintenanceItem from db
                        //Initialize page w/ MI data
                    }
                    break;
            }
        }

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
                Steps = await ConvertToListAsync( Steps )
            };

            await DbServiceLocator.AddItemAsync( item );
            ClearData();
            await Shell.Current.GoToAsync( $"//{nameof( HomeView )}?Refresh=true" );
        }

        private void ClearData()
        {
            if( !IsBusy )
            {
                IsBusy = true;

                Name = "New Maintenance Item";
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
                Steps.Clear();
                item = null;

                IsBusy = false;
            }
        }

        private async Task RefreshSteps()
        {
            if( !IsBusy )
            {
                IsBusy = true;

                Steps.Clear();

                List<Step> s = await DbServiceLocator.GetItemRangeAsync<Step>( newStepId ) as List<Step>;

                Steps.AddRange( s );

                IsBusy = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await DbServiceLocator.DeleteItemAsync<MaintenanceItem>( item.Id );
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
            await Shell.Current.GoToAsync( $"/{nameof( AddNewStepView )}" );
        }

        private protected override void EvaluateQueryParams( string key, string value )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

