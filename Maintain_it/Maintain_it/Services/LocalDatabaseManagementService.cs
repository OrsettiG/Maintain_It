using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Maintain_it.Services
{
    public static class LocalDatabaseManagementService
    {
        private static IDatabaseItemManager _databaseItemManager;

        static LocalDatabaseManagementService()
        {
            _databaseItemManager = DependencyService.Get<IDatabaseItemManager>();

        }
    }
}
