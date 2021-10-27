using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SQLite;

using SQLiteNetExtensionsAsync.Extensions;

using Xamarin.Essentials;

namespace Maintain_it.Services
{
    public static class AsyncDatabaseConnection
    {
        private static SQLiteAsyncConnection db;

        public static SQLiteAsyncConnection Db => db ??= new SQLiteAsyncConnection( Path.Combine( FileSystem.AppDataDirectory, "MaintainIt.db" ) );
    }
}
