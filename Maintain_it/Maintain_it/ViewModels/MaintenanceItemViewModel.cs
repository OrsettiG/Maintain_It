﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;
using MvvmHelpers;
using Maintain_it.Views;

namespace Maintain_it.ViewModels
{
    public class MaintenanceItemViewModel : BaseViewModel
    {
        #region Constructors
        public MaintenanceItemViewModel()
        {
            //_ = Task.Run( async () => await InitializeServices() );

            Steps = new ObservableRangeCollection<Step>();

            Steps.Add( new Step()
            {
                Name = "Default Step"
            } );
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
        public int RecursEvery { get => recursEvery; set => SetProperty( ref recursEvery, value ); }

        private Timeframe frequency = Timeframe.MONTHS;
        public Timeframe Frequency { get => frequency; set => SetProperty( ref frequency, value ); }

        private int timesServiced;
        public int TimesServiced { get => timesServiced; set => SetProperty( ref timesServiced, value ); }

        private bool previousServiceCompleted;
        public bool PreviousServiceCompleted { get => previousServiceCompleted; set => SetProperty( ref previousServiceCompleted, value ); }

        private bool notifyOfNextServiceDate = true;
        public bool NotifyOfNextServiceDate { get => notifyOfNextServiceDate; set => SetProperty( ref notifyOfNextServiceDate, value ); }


        public ObservableRangeCollection<Step> Steps { get; set; }

        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public AsyncCommand AddCommand => addCommand ??= new AsyncCommand( Add );

        private AsyncCommand deleteCommand;
        public AsyncCommand DeleteCommand => deleteCommand ??= new AsyncCommand( Delete );

        private AsyncCommand refreshCommand;
        public AsyncCommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );

        private AsyncCommand updateCommand;
        public AsyncCommand UpdateCommand => updateCommand ??= new AsyncCommand( Update );

        private Command onIncrementCommand;
        public ICommand OnIncrementCommand => onIncrementCommand ??= new Command( Increment );

        private Command onDecrementCommand;
        public ICommand OnDecrementCommand => onDecrementCommand ??= new Command( Decrement );

        private AsyncCommand newStepCommand;
        public ICommand NewStepCommand => newStepCommand ??= new AsyncCommand( NewStep );
        #endregion

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
                Frequency = frequency,
                TimesServiced = timesServiced,
                PreviousServiceCompleted = previousServiceCompleted,
                NotifyOfNextServiceDate = notifyOfNextServiceDate,
                Steps = ConvertToList( Steps )
            };

            await DbServiceLocator.AddItemAsync( item );
            await Refresh();
        }

        private List<T> ConvertToList<T>( IEnumerable<T> target )
        {
            List<T> list = new List<T>();
            foreach( T item in target )
            {
                list.Add( item );
            }

            return list;
        }

        private async Task Refresh()
        {
            if( !IsBusy && item != null )
            {
                IsBusy = true;

                //await Task.Delay( 0 );

                item = await DbServiceLocator.GetItemAsync<MaintenanceItem>( item.Id );

                ////This might cause a bug... Just check it if the list isn't displaying when you think it should be
                //StepMaterials = new ObservableRangeCollection<Material>( await DbServiceLocator.GetAllItemsAsync<Material>() );

                IsBusy = false;
            }
        }

        private async Task Delete()
        {
            if( item != null )
            {
                await DbServiceLocator.DeleteItemAsync<MaintenanceItem>( item.Id );
            }

            await Refresh();
        }

        private async Task Update()
        {
            if( item != null )
            {
                if( await DbServiceLocator.GetItemAsync<MaintenanceItem>( item.Id ) != item )
                {
                    await DbServiceLocator.UpdateItemAsync( item );
                }

                await Refresh();
            }
        }


        private async Task NewStep()
        {
            await AppShell.Current.GoToAsync( nameof( AddNewStepView ) );
        }
    }
}

