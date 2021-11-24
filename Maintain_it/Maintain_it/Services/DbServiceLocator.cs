﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public static class DbServiceLocator
    {

        // Private singleton class for each Service type. Async Task to add an instance is just so that all the other methods can run async without complaining. Not sure that is actually does anything, but you know, what the hell.
        private static class LocatorEntry<T, U> where T : Service<U> where U : IStorableObject, new()
        {
            public static Service<U> Instance { get; set; }
        }

        // Registers new services
        private static void Register<T>( Service<T> instance ) where T : IStorableObject, new()
        {
            LocatorEntry<Service<T>, T>.Instance = instance;
        }

        // Gets a service if it already exists and creates + inits a new one based on the supplied type if not. Allows for lazy loading of services.
        private static async Task<Service<T>> GetService<T>() where T : IStorableObject, new()
        {
            if( LocatorEntry<Service<T>, T>.Instance == null )
            {
                Register( new Service<T>() );
                await LocatorEntry<Service<T>, T>.Instance.Init();
            }

            return LocatorEntry<Service<T>, T>.Instance;
        }

        /// <summary>
        /// Init a new Service for the specified type if it has not already been initted.
        /// </summary>
        /// <typeparam name="T">Service type to init</typeparam>
        public static async Task Init<T>() where T : IStorableObject, new()
        {
            if( LocatorEntry<Service<T>, T>.Instance == null )
            {
                Register( new Service<T>() );
                await LocatorEntry<Service<T>, T>.Instance.Init();
                return;
            }

            if( !LocatorEntry<Service<T>, T>.Instance.IsInitialized() )
            {
                await LocatorEntry<Service<T>, T>.Instance.Init();
            }
        }

        /// <summary>
        /// Adds an item to the db and finishes. Doesn't give you back an id, so only use this when you don't need to use the new item after it has been added.
        /// </summary>
        /// <typeparam name="T"> The item type you are adding </typeparam>
        /// <param name="Item"> The item you are adding </param>
        public static async Task AddItemAsync<T>( T item ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            await instance.AddItemAsync( item );
        }

        /// <summary>
        /// Adds a new item to the db and gives back the id. Use this when building up a list of new objects within an object you're building (new Steps() within a new MaintenanceItem())
        /// </summary>
        /// <typeparam name="T"> The item type you are adding </typeparam>
        /// <param name="Item"> The item you are adding </param>
        /// <returns> The RowId of the item you just added </returns>
        public static async Task<int> AddItemAndReturnIdAsync<T>( T Item ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            return await instance.AddItemAndReturnRowIdAsync( Item );
        }

        /// <summary>
        /// Deletes the item associated with the passed in id from the db
        /// </summary>
        /// <typeparam name="T"> The Service type to use </typeparam>
        /// <param name="id"> The id of the item to Delete </param>
        public static async Task DeleteItemAsync<T>( int id ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            await instance.DeleteItemAsync( id );
        }

        /// <summary>
        /// Gets all items from the appropriate table using the specified Service type
        /// </summary>
        /// <typeparam name="T"> The Service type to use </typeparam>
        /// <returns> An Enumberable collection of all the objects found in the specified table </returns>
        public static async Task<IEnumerable<T>> GetAllItemsAsync<T>() where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            IEnumerable<T> data = await instance.GetAllItemsAsync();
            return data;
        }

        /// <summary>
        /// Get only items with Ids matching the passed in List<T> from the appropriate Table
        /// </summary>
        /// <typeparam name="T">Service Type</typeparam>
        /// <param name="ids">List of Ids to get</param>
        /// <returns>IEnumerable<T></returns>
        public static async Task<IEnumerable<T>> GetItemRangeAsync<T>( IEnumerable<int> ids ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();

            IEnumerable<T> data = await instance.GetItemRangeAsync( ids );
            return data;
        }

        /// <summary>
        /// Get only items with Ids matching the passed in List<T> from the appropriate Table
        /// </summary>
        /// <typeparam name="T">Service Type</typeparam>
        /// <param name="ids">List of Ids to get</param>
        /// <returns>IEnumerable<T></returns>
        public static async Task<IEnumerable<T>> GetItemRangeBasedOnSearchTermAsync<T>( string searchTerm ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();

            IEnumerable<T> data = await instance.GetItemRangeBasedOnSearchTermAsync(searchTerm);
            return data;
        }

        /// <summary>
        /// Gets all items created on or between the passed in DateTimes.
        /// </summary>
        /// <typeparam name="T">Service Type</typeparam>
        /// <param name="newestDateCreated">The most recent items to return</param>
        /// <param name="oldestDateCreated">The oldest items to return</param>
        /// <param name="returnAll">Optional parameter if you want only a certain number of items.</param>
        /// <param name="returnCount">The number of items to return if you don't return all.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetItemsInDateRange<T>( DateTime newestDateCreated, DateTime oldestDateCreated, bool returnAll = true, int returnCount = 0 ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            IEnumerable<T> data = await instance.GetItemsInDateRangeAsync(newestDateCreated, oldestDateCreated, returnAll, returnCount);

            return data;
        }

        /// <summary>
        /// Gets the item with the passed in id from the approprate table
        /// </summary>
        /// <typeparam name="T"> The Service type to use </typeparam>
        /// <param name="id"> The id of the item to return </param>
        /// <returns> The item with the matching id to the one passed in, if any. </returns>
        public static async Task<T> GetItemAsync<T>( int id ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            return await instance.GetItemAsync( id );
        }

        /// <summary>
        /// Updates the item in the appropriate table with it's new values
        /// </summary>
        /// <typeparam name="T"> The Service type to use </typeparam>
        /// <param name="item"> The item to update (the version currently in the db will be updated to match this item) </param>
        public static async Task UpdateItemAsync<T>( T item ) where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            await instance.UpdateItemAsync( item );
        }

        /// <summary>
        /// Deletes ALL items from the table containing the specified type. ALL of them. They will be gone forever. ALL of them. Not just one, ALL of them. Completely irretreavable. Gone. ALL I repeat ALL of them. Permanently.
        /// </summary>
        /// <typeparam name="T">The type of tableyou want to clear</typeparam>
        /// <returns>The number of deleted rows</returns>
        public static async Task<int> DeleteAllAsync<T>() where T : IStorableObject, new()
        {
            Service<T> instance = await GetService<T>();
            return await instance.DeleteAllAsync<T>();
        }
    }
}
