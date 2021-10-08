using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SQLite;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public static class AsyncDatabaseConnection
    {
        private static readonly SQLiteAsyncConnection db;

        internal static SQLiteAsyncConnection Db => db ?? new SQLiteAsyncConnection( Path.Combine( FileSystem.AppDataDirectory, "MaintainIt.db" ) );
    }
}
