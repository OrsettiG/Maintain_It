using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
//using System.Windows.Input;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public TestViewModel()
        {
            Console.WriteLine( $"PACKAGE NAME = {AppInfo.PackageName}" );
        }

        private string name;
        public string Name 
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private int nextNodeId;
        public int NextNodeId
        {
            get => nextNodeId;
            set => SetProperty(ref nextNodeId, value);
        }
        
        private int previousNodeId;
        public int PreviousNodeId
        {
            get => previousNodeId;
            set => SetProperty(ref previousNodeId, value);
        }
        
        
        private string dbName;
        public string DbName 
        {
            get => dbName;
            set => SetProperty(ref dbName, value);
        }

        private int dbNextNodeId;
        public int DbNextNodeId
        {
            get => dbNextNodeId;
            set => SetProperty(ref dbNextNodeId, value);
        }
        
        private int dbPreviousNodeId;
        public int DbPreviousNodeId
        {
            get => dbPreviousNodeId;
            set => SetProperty(ref dbPreviousNodeId, value);
        }


        private Step step;
        public Step Step
        {
            get => step;
            set => SetProperty( ref step, value );
        }

        private AsyncCommand nofityCommand;
        public ICommand NotifyCommand
        {
            get => nofityCommand ??= new AsyncCommand( Notify );
        }

        private async Task Notify()
        {
            LocalNotificationManager.ShowNotification( "SHOW TEST", "TESTING SHOW NOTIFICATION STUFFS" );
            NextNodeId = await LocalNotificationManager.GetNewScheduledNotification( "TEST", DateTime.Now.AddSeconds( 120 ), 1, (int)Timeframe.Minutes );
        }

        private AsyncCommand updateStepCommand;
        public ICommand UpdateStepCommand
        {
            get => updateStepCommand ??= new AsyncCommand( UpdateStep );
        }

        private async Task UpdateStep()
        {
            Step.Name = Name;
            Step.NextNodeId = NextNodeId ;
            Step.PreviousNodeId = PreviousNodeId;

            await DbServiceLocator.UpdateItemAsync( Step );
            await Refresh();
        }

        private async Task Refresh()
        {
            Step = await DbServiceLocator.GetItemRecursiveAsync<Step>( Step.Id );

            DbName = Step.Name;
            DbNextNodeId = Step.NextNodeId;
            DbPreviousNodeId = Step.PreviousNodeId;
        }
    }
}
