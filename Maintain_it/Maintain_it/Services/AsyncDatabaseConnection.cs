using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using SQLite;

using SQLiteNetExtensionsAsync.Extensions;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public static class AsyncDatabaseConnection
    {
        private static SQLiteAsyncConnection db;
        private static string _filepath => Path.Combine( FileSystem.AppDataDirectory, "MaintainIt.db" );

        public static SQLiteAsyncConnection Db => db ??= new SQLiteAsyncConnection( _filepath );

        /// <summary>
        /// !!!USE WITH CAUTION!!! Completely deletes all tables from the database. There is no recovering from this. !!!USE WITH CAUTION!!!
        /// </summary>
        public static async Task DropAllTablesAsync()
        {
            _ = await db.DropTableAsync<MaintenanceItem>();
            _ = await db.DropTableAsync<Step>();
            _ = await db.DropTableAsync<StepMaterial>();
            _ = await db.DropTableAsync<Note>();
            _ = await db.DropTableAsync<Retailer>();
            _ = await db.DropTableAsync<RetailerMaterial>();
            _ = await db.DropTableAsync<ShoppingList>();
            _ = await db.DropTableAsync<ShoppingListMaterial>();
            _ = await db.DropTableAsync<Material>();
        }

        /// <summary>
        /// !!!USE WITH CAUTION!!! Deletes the ENTIRE Database!!!! There will be nothing left after this is run, DO NOT SHIP WITH PRODUCTION PRODUCT!!!
        /// </summary>
        /// <returns> <see langword="true"/> if database was successfully deleted, <see langword="false"/> otherwise.</returns>
        public static bool DeleteDb()
        {
            File.Delete( _filepath );
            db = null;
            return File.Exists( _filepath );
        }
    }
}
