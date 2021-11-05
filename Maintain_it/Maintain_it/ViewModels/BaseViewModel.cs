using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Services;

using MvvmHelpers;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public abstract class BaseViewModel : ObservableObject, IQueryAttributable
    {
        public INavigation Navigation { get; set; }

        public virtual async void ApplyQueryAttributes( IDictionary<string, string> query )
        {
            foreach( KeyValuePair<string, string> kvp in query )
            {
                await EvaluateQueryParams( kvp );
            }
        }

        private protected abstract Task EvaluateQueryParams( KeyValuePair<string, string> kvp );


        public int ScreenWidth => (int)DeviceDisplay.MainDisplayInfo.Width;
        public int ScreenHeight => (int)DeviceDisplay.MainDisplayInfo.Height;

        private protected List<T> ConvertToList<T>( IEnumerable<T> target )
        {
            List<T> list = new List<T>();
            foreach( T item in target )
            {
                list.Add( item );
            }

            return list;
        }

        private protected async Task<List<T>> ConvertToListAsync<T>(IEnumerable<T> target )
        {
            return await Task.Run( () => ConvertToList( target ) );
        }

    }
}
