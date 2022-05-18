using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public class DisplayAllShoppingListsViewModel : BaseViewModel
    {
        public DisplayAllShoppingListsViewModel()
        {
            _ = Task.Run( async () => await Refresh() );
        }
        #region Properties
        private List<ShoppingList> shoppingLists { get; set; }

        private ObservableRangeCollection<ShoppingListViewModel> shoppingListViewModels;
        public ObservableRangeCollection<ShoppingListViewModel> ShoppingListViewModels 
        { 
            get => shoppingListViewModels ??= new ObservableRangeCollection<ShoppingListViewModel>(); 
            set => SetProperty( ref shoppingListViewModels, value ); 
        }

        #endregion

        #region Commands
        // Create New Shopping List
        private AsyncCommand createNewShoppingListCommand;
        public ICommand CreateNewShoppingListCommand 
        { 
            get => createNewShoppingListCommand ??= new AsyncCommand( CreateNewShoppingList ); 
        }

        private async Task CreateNewShoppingList()
        {
            await Shell.Current.GoToAsync( $"{nameof( CreateNewShoppingListView )}" );
        }

        // Open ShoppingList
        private AsyncCommand<int> openShoppingListCommand;
        private ICommand OpenShoppingListCommand 
        { 
            get => openShoppingListCommand ??= new AsyncCommand<int>( x => OpenShoppingList( x ) );
        }

        private async Task OpenShoppingList( int shoppingListId )
        {
            string encodedId = HttpUtility.HtmlEncode(shoppingListId);
            await Shell.Current.GoToAsync( $"{nameof( ShoppingListDetailView )}?id={encodedId}" );
        }
        
        // Refresh
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand 
        {  
            get => refreshCommand ??= new AsyncCommand( Refresh );
        }
        private async Task Refresh()
        {
            shoppingLists = await DbServiceLocator.GetAllItemsAsync<ShoppingList>().ConfigureAwait( false ) as List<ShoppingList>;

            ShoppingListViewModels.Clear();
            ConcurrentBag<ShoppingListViewModel> bag = new ConcurrentBag<ShoppingListViewModel>();

            _ = Parallel.ForEach( shoppingLists, sList =>
            {
                if( sList.Active || !sList.Active)
                {
                    ShoppingListViewModel item = new ShoppingListViewModel( sList );
                    item.RefreshContainer = (AsyncCommand)RefreshCommand;
                    bag.Add( item );
                    
                }
            } );

            ShoppingListViewModels.AddRange( bag );
        }

        #endregion

        #region Methods
        private async Task Init()
        {
            await Refresh();
        }


        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case RoutingPath.Refresh:
                    await Refresh();
                    break;
            }
        }

        #endregion
        #endregion
    }
}
