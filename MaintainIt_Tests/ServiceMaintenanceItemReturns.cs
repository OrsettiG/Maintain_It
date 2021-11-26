using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using NUnit.Framework;

using SQLite;

namespace MaintainIt_Tests
{
    public class ServiceMaintenanceItemReturns
    {
        Service<MaintenanceItem>? service;
        SQLiteAsyncConnection connection;

        [SetUp]
        public void SetUp()
        {
            service = new Service<MaintenanceItem>();
            //connection = AsyncDatabaseConnection.Db;
        }

        [Test]
        public async Task SuccessfulInsert()
        {

        }
    }
}
