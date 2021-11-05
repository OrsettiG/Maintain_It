using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;

using SQLite;

namespace Maintain_it.ViewModels
{
    public class TestItem : BaseViewModel
    {
        public TestItem()
        {
            Items = new ObservableCollection<TestItemModel>();

            db = new TestDB();

            RefreshCommand = new AsyncCommand( Refresh );
            AddCommand = new AsyncCommand( Add );
            DeleteCommand = new AsyncCommand( Delete );

            _ = Task.Run( async () => await Refresh() );
        }

        private TestDB db;
        private int count = 1;
        public ObservableCollection<TestItemModel> Items { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand AddCommand { get; }
        public AsyncCommand DeleteCommand { get; }
        public bool IsBusy { get; private set; }

        private async Task Delete()
        {
            TestItemModel item = Items[0];
            if( item != null )
            {
                await db.DeleteItemAsync( item.id );
            }

            await Refresh();
        }

        private async Task Add()
        {
            TestItemModel testItem = new TestItemModel() { Name = $"Test Item {count}" };
            await db.AddItemAsync( testItem );
            count++;

            await Refresh();
        }

        private async Task Refresh()
        {
            if( !IsBusy )
            {
                IsBusy = true;

                await Task.Delay( 1000 );

                Items.Clear();

                List<TestItemModel> items = await db.GetAllItemsAsync() as List<TestItemModel>;

                foreach(TestItemModel item in items )
                {
                    Items.Add( item );
                }

                IsBusy = false;
            }
        }

        private protected override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            throw new NotImplementedException();
        }
    }
}
