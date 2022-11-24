using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models.Interfaces;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class ShoppingListItemGroupViewModel : ObservableRangeCollection<IShoppingListItemViewModel>, INotifyPropertyChanged
    {

        #region Constructors
        public ShoppingListItemGroupViewModel( string name, IEnumerable<IShoppingListItemViewModel> items ) : base( items )
        {
            foreach( IShoppingListItemViewModel item in Items )
            {
                item.OnItemPurchased += UpdateCount;
                item.OnItemDeleted += ItemDeleted;
                item.OnItemSaved += x => Console.WriteLine( $"Item with id {x} Saved!" );
                TotalCount += item.Quantity;
            }

            GroupName = name;
            //PurchasedCount = items.Where(x => x.Purchased).Select( x => x.Quantity ).Sum();
            //if( PurchasedCount == TotalCount )
            //{
            //    Complete = true;
            //}
            PurchasedCount = 0;
            UpdateCount( items.Where( x => x.Purchased ).Select( x => x.Quantity ).Sum() );
            DisplayName = $"{GroupName} {PurchasedCount}/{TotalCount}";
        }
        #endregion Constructors

        #region Events
        public event Action<int> OnItemPurchased;
        public event Action<int> OnAllItemsPurchased;
        #endregion Events

        #region Properties
        private string displayName;
        public string DisplayName
        {
            get => displayName; 
            set => SetProperty( ref displayName, value );
        }

        private string groupName;
        public string GroupName
        {
            get => groupName;
            set => SetProperty( ref groupName, value );
        }

        // The sum of all item Quantities
        private int totalCount;
        public int TotalCount
        {
            get => totalCount;
            private set => SetProperty( ref totalCount, value );
        }

        // The sum of all purchased item Quantities
        private int purchasedCount;
        public int PurchasedCount
        {
            get => purchasedCount;
            private set => SetProperty( ref purchasedCount, value );
        }

        private bool complete = false;
        public bool Complete
        {
            get => complete;
            set => SetProperty( ref complete, value );
        }
        #endregion Properties

        #region Commands
        private Command<int> itemPrePurchased;
        public ICommand ItemPrePurchased
        {
            get => itemPrePurchased ??= new Command<int>( x => UpdateCount( x ) );
        }

        private AsyncCommand purchaseAllItemsCommand;
        public ICommand PurchaseAllItemsCommand
        {
            get => purchaseAllItemsCommand ??= new AsyncCommand( PurchaseAllItems );
        }

        private async Task PurchaseAllItems()
        {
            foreach(IShoppingListItemViewModel item in Items)
            {
                item.TogglePurchasedCommand.Execute(null);
            }
        }
        #endregion Commands

        #region Methods
        private void UpdateCount( int change )
        {
            PurchasedCount += change;

            if( change > 0 )
            {
                if( PurchasedCount == TotalCount )
                {
                    OnAllItemsPurchased?.Invoke( 1 );
                    Complete = true;
                }
            }
            else if( change < 0 )
            {

                if( Complete && PurchasedCount < TotalCount )
                {
                    OnAllItemsPurchased?.Invoke( -1 );
                    Complete = false;
                }
            }

            OnItemPurchased?.Invoke( change );
            DisplayName = $"{GroupName} {PurchasedCount}/{TotalCount}";
        }

        private void ItemDeleted( int materialId )
        {
            IShoppingListItemViewModel delete = Items.Where( x => x.MaterialId == materialId ).FirstOrDefault();

            if( delete != null )
            {
                _ = Items.Remove( delete );
            }
        }

        //public void AddItemToGroup( ShoppingListMaterialViewModel item )
        //{
        //    if( !Items.Contains( item ) )
        //    {
        //        item.OnPurchasedChanged += UpdateCount;
        //        item.OnItemDeletedAsyncCommand = new AsyncCommand<int>( x => ItemDeleted( x ) );
        //        Items.Add( item );
        //    }
        //}

        //private async Task ItemDeleted( int id )
        //{

        //    ShoppingListMaterialViewModel deletedVM = Items.Where( x => x.ShoppingListMaterial.Id == id).SingleOrDefault();
        //    if( deletedVM != default )
        //    {
        //        _ = Items.Remove( deletedVM );
        //    }
        //}


        #endregion Methods

        #region INotifyPropertyChanged Implementation
        //
        // Summary:
        //     Occurs when property changed.
        public event PropertyChangedEventHandler? PropertyChanged;

        //
        // Summary:
        //     Sets the property.
        //
        // Parameters:
        //   backingStore:
        //     Backing store.
        //
        //   value:
        //     Value.
        //
        //   validateValue:
        //     Validates value.
        //
        //   propertyName:
        //     Property name.
        //
        //   onChanged:
        //     On changed.
        //
        // Type parameters:
        //   T:
        //     The 1st type parameter.
        //
        // Returns:
        //     true, if property was set, false otherwise.
        protected virtual bool SetProperty<T>( ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null, Func<T, T, bool>? validateValue = null )
        {
            if( EqualityComparer<T>.Default.Equals( backingStore, value ) )
            {
                return false;
            }

            if( validateValue != null && !validateValue!( backingStore, value ) )
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged( propertyName );
            return true;
        }

        //
        // Summary:
        //     Raises the property changed event.
        //
        // Parameters:
        //   propertyName:
        //     Property name.
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion INotifyPropertyChanged Implementation
    }
}
