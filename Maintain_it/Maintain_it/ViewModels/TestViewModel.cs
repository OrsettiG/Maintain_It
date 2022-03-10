using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using System.Windows.Input;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public TestViewModel()
        {
            testData = new ObservableRangeCollection<TestItemViewModel>()
            {
                new TestItemViewModel( "Item 1", 1 ),
                new TestItemViewModel( "Item 2", 2 ),
                new TestItemViewModel( "Item 3", 3 ),
                new TestItemViewModel( "Item 4", 4 ),
                new TestItemViewModel( "Item 5", 5 ),
                new TestItemViewModel( "Item 6", 6 ),
                new TestItemViewModel( "Item 7", 7 ),
                new TestItemViewModel( "Item 8", 8 ),
                new TestItemViewModel( "Item 9", 9 ),
                new TestItemViewModel( "Item 10", 10 )
            };
        }

        private ObservableRangeCollection<TestItemViewModel> testData;
        public ObservableRangeCollection<TestItemViewModel> TestData { get => testData ??= new ObservableRangeCollection<TestItemViewModel>(); set => SetProperty( ref testData, value ); }

        //#region Commands
        //private AsyncCommand<string> dragStartingCommand;
        //public AsyncCommand<string> DragStartingCommand => dragStartingCommand ??= new AsyncCommand<string>( x => DragStarting( x ) );

        //private AsyncCommand<string> dropCompleteCommand;
        //public AsyncCommand<string> DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<string>( x => DropComplete( x ) );

        //private AsyncCommand<string> dragOverCommand;
        //public AsyncCommand<string> DragOverCommand => dragOverCommand ??= new AsyncCommand<string>( x => DragOver( x ) );

        //private AsyncCommand<string> dragLeaveCommand;
        //public AsyncCommand<string> DragLeaveCommand => dragLeaveCommand ??= new AsyncCommand<string>( x => DragLeave( x ) );

        //private AsyncCommand<string> dropCommand;
        //public AsyncCommand<string> DropCommand => dropCommand ??= new AsyncCommand<string>( x => Drop( x ) );

        //#endregion

        //private Task Drop( string x )
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DragLeave( string x )
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DragOver( string x )
        //{
        //    throw new NotImplementedException();
        //}


        //private Task DragStarting( string x )
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DropComplete( string x )
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class TestItemViewModel : BaseViewModel
    {
        public TestItemViewModel( string name, int index )
        {
            Name = name;
            Index = index;
        }
        public string Name { get; set; }
        public int Index { get; set; }

        #region Commands
        private ICommand dragStartingCommand;
        public ICommand DragStartingCommand => dragStartingCommand ??= new AsyncCommand<string>( x => DragStarting( x ) ) ;

        private ICommand dropCompleteCommand;
        public ICommand DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<string>( x => DropComplete( x ) );

        private ICommand dragOverCommand;
        public ICommand DragOverCommand => dragOverCommand ??= new AsyncCommand<string>( x => DragOver( x ) );

        private ICommand dragLeaveCommand;
        public ICommand DragLeaveCommand => dragLeaveCommand ??= new AsyncCommand<string>( x => DragLeave( x ) );

        private ICommand dropCommand;
        public ICommand DropCommand => dropCommand ??= new AsyncCommand<string>( x => Drop( x ) );

        #endregion

        private async Task Drop( string x )
        {
            Console.WriteLine( $"{x} Drop" );
        }

        private async Task DragLeave( string x )
        {
            Console.WriteLine( $"{x} Drag Leave" );
        }

        private async Task DragOver( string x )
        {
            Console.WriteLine( $"{x} Drag Over" );
        }

        private async Task DragStarting( string x )
        {
            Console.WriteLine( $"{x} Drag Starting" );
        }

        private async Task DropComplete( string x )
        {
            Console.WriteLine( $"{x} Drag Complete" );
        }
    }
}
