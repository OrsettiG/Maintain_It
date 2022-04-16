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
        protected char[] restrictedChars = new char[]
        {
            '\\',
            '/',
            '?',
            '<',
            '>',
            ';',
            '"'
        };

        public int ScreenWidth => (int)DeviceDisplay.MainDisplayInfo.Width;
        public int ScreenHeight => (int)DeviceDisplay.MainDisplayInfo.Height;
        public int ScreenDensity => (int)DeviceDisplay.MainDisplayInfo.Density;
        public int DensityCorrectedScreenHeight => ScreenHeight / ScreenDensity;
        public int DensityCorrectedScreenWidth => ScreenWidth / ScreenDensity;
        public INavigation Navigation { get; set; }

        public virtual async void ApplyQueryAttributes( IDictionary<string, string> query )
        {
            foreach( KeyValuePair<string, string> kvp in query )
            {
                await EvaluateQueryParams( kvp );
                EvaluateQueryParams(kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Asyncronously evaluate query parameters and do work when arriving at a new view. Returns null unless overridden.
        /// </summary>
        /// <param name="kvp">A <see cref="KeyValuePair"/> representing the query parameter name with the key, and the value in the value. Good for evaluating query parameters with switch statements.</param>
        /// <returns><see cref="Task"/> representing whatever work the passed in parameter triggers.</returns>
        private protected virtual Task EvaluateQueryParams( KeyValuePair<string, string> kvp ) { return null; }

        /// <summary>
        /// Synchronously evaluate query parameters when arriving at a new view. Empty unless overriden.
        /// </summary>
        /// <param name="key">The parameter name</param>
        /// <param name="value">The parameter value</param>
        private protected virtual void EvaluateQueryParams( string key, string value ) { }



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

        protected virtual bool ValidateString( string _, string target )
        {
            foreach( char c in restrictedChars )
            {
                if( target.Contains( c ) )
                {
                    return false;
                }
            }

            return true;
        }

    }
}
