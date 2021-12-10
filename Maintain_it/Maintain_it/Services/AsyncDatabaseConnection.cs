using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        private static SQLiteAsyncConnection db = null;
        private static string _filepath => Path.Combine( FileSystem.AppDataDirectory, "MaintainIt.db" );

        /// <summary>
        /// Gets an <see cref="AsyncLazy{T}"/> instance of type <see cref="Database"/> which ensures that all tables are created before the user reads or writes any data. The SQLiteAsyncConnection is cached after the first method call, so subsequent calls are instant.
        /// </summary>
        /// <returns><see cref="Task{TResult}"/> that represents the db connection.</returns>
        public static async Task<SQLiteAsyncConnection> Db() 
        {
            if( db != null )
            {
                return db;
            }

            Database conn = await Database.Instance;
            db = conn.Db();
            return db;
        }

        /// <summary>
        /// !!!USE WITH CAUTION!!! Completely deletes all tables from the database. There is no recovering from this. !!!USE WITH CAUTION!!!
        /// </summary>
        public static async Task DropAllTablesAsync()
        {
            _ = await db.DropTableAsync<MaintenanceItem>().ConfigureAwait(false);
            _ = await db.DropTableAsync<Step>().ConfigureAwait(false);
            _ = await db.DropTableAsync<StepMaterial>().ConfigureAwait(false);
            _ = await db.DropTableAsync<Note>().ConfigureAwait(false);
            _ = await db.DropTableAsync<Retailer>().ConfigureAwait(false);
            _ = await db.DropTableAsync<RetailerMaterial>().ConfigureAwait(false);
            _ = await db.DropTableAsync<ShoppingList>().ConfigureAwait(false);
            _ = await db.DropTableAsync<ShoppingListMaterial>().ConfigureAwait(false);
            _ = await db.DropTableAsync<Material>().ConfigureAwait(false);
        }

        /// <summary>
        /// !!!USE WITH CAUTION!!! Deletes the ENTIRE Database!!!! There will be nothing left after this is run, DO NOT SHIP WITH PRODUCTION PRODUCT!!!
        /// </summary>
        /// <returns> <see langword="true"/> if database was successfully deleted, <see langword="false"/> otherwise.</returns>
        public static async Task<bool> DeleteDb()
        {
            await db.CloseAsync();
            File.Delete( _filepath );
            db = null;
            return File.Exists( _filepath );
        }

        private class Database
        {
            public Database()
            {
                database = new SQLiteAsyncConnection( _filepath );
            }

            private static SQLiteAsyncConnection database;

            public static readonly AsyncLazy<Database> Instance = new AsyncLazy<Database>( async () =>
            {
                Database instance = new Database();
                _ = await database.CreateTableAsync<MaintenanceItem>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<Material>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<Step>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<StepMaterial>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<Note>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<ShoppingList>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<ShoppingListMaterial>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<Retailer>().ConfigureAwait(false);
                _ = await database.CreateTableAsync<RetailerMaterial>().ConfigureAwait(false);

                return instance;
            });

            public SQLiteAsyncConnection Db()
            {
                return database;
            }
        }

        private class AsyncLazy<T> : Lazy<Task<T>>
        {
            public AsyncLazy( Func<Task<T>> taskFactory ) : base( () => Task.Factory.StartNew( () => taskFactory() ).Unwrap() ) { }

            public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }

        }
    }
}
