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

        private AsyncCommand newStepCommand;
        public ICommand NewStepCommand
        {
            get => newStepCommand ??= new AsyncCommand( NewStep );
        }

        private async Task NewStep()
        {
            int id = await StepManager.NewStep(true);

            Step = await StepManager.GetItemRecursiveAsync( id );
            await Refresh();
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



    //public class TestItemViewModel : BaseViewModel
    //{
    //    public TestItemViewModel( string name, TestViewModel vm )
    //    {
    //        Name = name;
    //        viewModel = vm;
    //    }
    //    public string Name { get; set; }
    //    public bool Dragging = false;

    //    private TestViewModel viewModel { get; }


    //    #region Commands

    //    private ICommand dropCompleteCommand;
    //    public ICommand DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<TestItemViewModel>( x => DropComplete( x ) );

    //    private ICommand dragOverCommand;
    //    public ICommand DragOverCommand => dragOverCommand ??= new AsyncCommand<TestItemViewModel>( x => DragOver( x ) );

    //    private ICommand dragLeaveCommand;
    //    public ICommand DragLeaveCommand => dragLeaveCommand ??= new AsyncCommand<TestItemViewModel>( x => DragLeave( x ) );

    //    private ICommand dropCommand;
    //    public ICommand DropCommand => dropCommand ??= new AsyncCommand<TestItemViewModel>( x => Drop( x ) );

    //    private ICommand dragStartingCommand;
    //    public ICommand DragStartingCommand => dragStartingCommand ??= new AsyncCommand<TestItemViewModel>( x => DragStarting( x ) );

    //    #endregion

    //    private async Task DragStarting( TestItemViewModel item )
    //    {
    //        Dragging = true;
    //        Console.WriteLine( $"TestViewModel DragStarting on {item.Name}" );
    //    }

    //    private async Task Drop( TestItemViewModel itemDroppedOn )
    //    {
    //        TestItemViewModel itemDropping = viewModel.TestData.First(i => i.Dragging);
    //        int index1 = viewModel.TestData.IndexOf(itemDropping);
    //        int index2 = viewModel.TestData.IndexOf(itemDroppedOn);

    //        viewModel.TestData.Move( index1, index2 );

    //        Console.WriteLine( $"TestViewModel Dropping {itemDroppedOn.Name}" );
    //    }

    //    private async Task DragLeave( TestItemViewModel item )
    //    {
    //        Console.WriteLine( $"TestViewModel DragLeave {item.Name}" );
    //    }

    //    private async Task DragOver( TestItemViewModel item )
    //    {
    //        Console.WriteLine( $"TestViewModel DragOver {item.Name}" );
    //    }

    //    private async Task DropComplete( TestItemViewModel item )
    //    {
    //        Dragging = false;
    //        Console.WriteLine( $"TestViewModel DropComplete {item.Name}" );
    //    }
    //}
}
