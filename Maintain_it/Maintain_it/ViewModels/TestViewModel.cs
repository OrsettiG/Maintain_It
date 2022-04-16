using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
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
        }

        private ObservableRangeCollection<StepMaterial> testData;
        public ObservableRangeCollection<StepMaterial> TestData { get => testData ??= new ObservableRangeCollection<StepMaterial>(); set => SetProperty( ref testData, value ); }

        private int testNum;
        public int TestNum { get => testNum; set => SetProperty( ref testNum, value ); }

        private AsyncCommand loadDataCommand;
        public ICommand LoadDataCommand { get => loadDataCommand ??= new AsyncCommand( LoadData ); }
        private async Task LoadData()
        {
            List<StepMaterial> data = await DbServiceLocator.GetAllItemsAsync<StepMaterial>() as List<StepMaterial>;

            TestData.Clear();
            TestData.AddRange( data );
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
