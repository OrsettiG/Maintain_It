using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using System.Windows.Input;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public TestViewModel()
        {
            testData = new ObservableRangeCollection<TestItemViewModel>()
            {
                new TestItemViewModel( "Item 1", this ),
                new TestItemViewModel( "Item 2", this ),
                new TestItemViewModel( "Item 3", this ),
                new TestItemViewModel( "Item 4", this ),
                new TestItemViewModel( "Item 5", this ),
                new TestItemViewModel( "Item 6", this ),
                new TestItemViewModel( "Item 7", this ),
                new TestItemViewModel( "Item 8", this ),
                new TestItemViewModel( "Item 9", this ),
                new TestItemViewModel( "Item 10", this )
            };

            TestNum = 1;
        }

        private ObservableRangeCollection<TestItemViewModel> testData;
        public ObservableRangeCollection<TestItemViewModel> TestData { get => testData ??= new ObservableRangeCollection<TestItemViewModel>(); set => SetProperty( ref testData, value ); }

        private int testNum;
        public int TestNum { get => testNum; set => SetProperty( ref testNum, value ); }

        
    }

    public class TestItemViewModel : BaseViewModel
    {
        public TestItemViewModel( string name, TestViewModel vm )
        {
            Name = name;
            viewModel = vm;
        }
        public string Name { get; set; }
        public bool Dragging = false;

        private TestViewModel viewModel { get; }


        #region Commands

        private ICommand dropCompleteCommand;
        public ICommand DropCompleteCommand => dropCompleteCommand ??= new AsyncCommand<TestItemViewModel>( x => DropComplete( x ) );

        private ICommand dragOverCommand;
        public ICommand DragOverCommand => dragOverCommand ??= new AsyncCommand<TestItemViewModel>( x => DragOver( x ) );

        private ICommand dragLeaveCommand;
        public ICommand DragLeaveCommand => dragLeaveCommand ??= new AsyncCommand<TestItemViewModel>( x => DragLeave( x ) );

        private ICommand dropCommand;
        public ICommand DropCommand => dropCommand ??= new AsyncCommand<TestItemViewModel>( x => Drop( x ) );

        private ICommand dragStartingCommand;
        public ICommand DragStartingCommand => dragStartingCommand ??= new AsyncCommand<TestItemViewModel>( x => DragStarting( x ) );

        #endregion

        private async Task DragStarting( TestItemViewModel item )
        {
            Dragging = true;
            Console.WriteLine( $"TestViewModel DragStarting on {item.Name}" );
        }

        private async Task Drop( TestItemViewModel itemDroppedOn )
        {
            TestItemViewModel itemDropping = viewModel.TestData.First(i => i.Dragging);
            int index1 = viewModel.TestData.IndexOf(itemDropping);
            int index2 = viewModel.TestData.IndexOf(itemDroppedOn);

            viewModel.TestData.Move( index1, index2 );
            
            Console.WriteLine( $"TestViewModel Dropping {itemDroppedOn.Name}" );
        }

        private async Task DragLeave( TestItemViewModel item )
        {
            Console.WriteLine( $"TestViewModel DragLeave {item.Name}" );
        }

        private async Task DragOver( TestItemViewModel item )
        {
            Console.WriteLine( $"TestViewModel DragOver {item.Name}" );
        }

        private async Task DropComplete( TestItemViewModel item )
        {
            Dragging = false;
            Console.WriteLine( $"TestViewModel DropComplete {item.Name}" );
        }
    }
}
