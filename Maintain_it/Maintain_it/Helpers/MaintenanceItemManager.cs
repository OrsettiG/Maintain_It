using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    internal static class MaintenanceItemManager
    {
        public static async Task<int> NewMaintenanceItem( string name, DateTime firstServiceDate, string comment = "", int recursEvery = 0, int timeframe = 0, bool notifyOfNextServiceDate = false )
        {
            MaintenanceItem item = new MaintenanceItem()
            {
                Name = name == "" ? "New Project" : name,
                FirstServiceDate = firstServiceDate,
                NextServiceDate = CalculateNextServiceDate( firstServiceDate, recursEvery, timeframe ),
                Comment = comment,
                RecursEvery = recursEvery,
                Timeframe = timeframe,
                NotifyOfNextServiceDate = notifyOfNextServiceDate,
                CreatedOn = DateTime.Now,
                Steps = new List<Step>(),
                ServiceRecords = new List<ServiceRecord>()
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( item );
        }

        private static DateTime? CalculateNextServiceDate( DateTime lastServiceDate, int recursEvery, int timeframe )
        {
            if( timeframe == 0 )
                return null;

            Timeframe Timeframe = (Timeframe)timeframe;

            DateTime? nextServiceDate = Timeframe switch
            {
                Timeframe.Minutes => lastServiceDate.AddMinutes( recursEvery ),
                Timeframe.Hours => lastServiceDate.AddHours( recursEvery ),
                Timeframe.Days => lastServiceDate.AddDays( recursEvery ),
                Timeframe.Weeks => lastServiceDate.AddDays( recursEvery * 7 ),
                Timeframe.Months => lastServiceDate.AddMonths( recursEvery ),
                Timeframe.Years => lastServiceDate.AddYears( recursEvery ),
                _ => null
            };

            return nextServiceDate;
        }

        /// <summary>
        /// Retrieves the passed in <see cref="MaintenanceItem"/> and <see cref="Step"/> from the Db, adds the Step to the MaintenanceItem's Steps List, if it is not already there, and updates the Db. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task AddStep( int maintenanceItemId, int stepId )
        {
            MaintenanceItem item = await GetItemRecursiveAsync( maintenanceItemId );

            Step step = await StepManager.GetItemRecursiveAsync( stepId );

            await AddStep( item, step );
        }

        /// <summary>
        /// Adds the Step to the MaintenanceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        private static async Task AddStep( MaintenanceItem item, Step step, bool updateDb = true )
        {
            if( !item.Steps.Contains( step ) )
            {
                item.Steps.Add( step );



                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( item );
            }
        }

        public static async Task UpdateStepSequence( int itemId )
        {
            MaintenanceItem item = await GetItemRecursiveAsync(itemId);

            await UpdateStepSequence( item );
        }

        private static async Task UpdateStepSequence( MaintenanceItem item )
        {
            SortedSet<Step> steps = item.Steps.OrderBy(x => x.Index) as SortedSet<Step>;

            Step downstream = null;

            foreach( Step step in steps )
            {
                // Set the step's previous node to the downstream node. This will always be null on the first node.
                step.PreviousNode = downstream;
                step.PreviousNodeId = downstream.Id;

                // Set the downstream node's next node to this one. This will be null on the first iteration so we have to null check.
                switch( downstream )
                {
                    case null:
                        break;

                    default:
                        downstream.NextNode = step;
                        downstream.NextNodeId = step.Id;
                        break;
                }

                // Now that downstream is all tied up, we can update it in the db.
                await StepManager.UpdateItemIndexAsync( downstream );

                // Set this nodes next node to null. If there is another node in the sequence, this will be set to the correct node on the next loop and if we are on the last node then we want this to be null anyways.
                step.NextNode = null;
                step.NextNodeId = null;

                // Set the downstream node to this one
                downstream = step;
            }

            // Update the last item in the step sequence.
            await StepManager.UpdateItemIndexAsync( downstream );
        }


        /// <summary>
        /// Retrieves the passed in <see cref="MaintenanceItem"/> and <see cref="Step"/> from the Db. Adds the Step to the MaintenanceItem's Steps List, if it is not already there, and updates the Db. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task AddSteps( int itemId, IEnumerable<int> stepIds )
        {
            MaintenanceItem item = await GetItemRecursiveAsync(itemId);
            List<Step> steps = await StepManager.GetItemRange( stepIds );

            await AddSteps( item, steps, false );

            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Adds the Step to the MaintenanceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        private static async Task AddSteps( MaintenanceItem item, IEnumerable<Step> steps, bool updateDb = true )
        {
            foreach( Step step in steps )
            {
                await AddStep( item, step, false );
            }

            if( updateDb )
                await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Retrieves the passed in <see cref="MaintenanceItem"/> and <see cref="Step"/> from the Db. Adds the Step to the MaintenanceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task UpdateSteps( int maintenanceItemId, IEnumerable<int> stepIds )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);
            List<Step> steps = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>(stepIds) as List<Step>;

            await RemoveMissingSteps( item, steps );

            await AddSteps( item, steps, false );

            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Removes the <see cref="Step"/> with the passed in Id from the <see cref="MaintenanceItem.Steps"/> collection if it exists and updates the Db. ONLY UPDATES THE MAINTENANCE ITEM, DOES NOT UPDATE STEP DB RECORD.
        /// </summary>
        public static async Task RemoveStep( int maintenanceItemId, int stepId )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            await RemoveStep( item, step );
        }

        private static async Task RemoveStep( MaintenanceItem item, Step step, bool updateDb = true )
        {
            if( item.Steps.Contains( step ) )
            {
                _ = item.Steps.RemoveAll( x => x.Id == step.Id );

                await StepManager.DeleteItem( step.Id );

                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( item );
            }
        }

        /// <summary>
        /// Removes the <see cref="Step"/>s with the passed in Ids from the <see cref="MaintenanceItem.Steps"/> collection if they exist and updates the Db. ONLY UPDATES THE MAINTENANCE ITEM, DOES NOT UPDATE STEP DB RECORDS.
        /// </summary>
        public static async Task RemoveMissingSteps( int maintenanceItemId, IEnumerable<int> stepIds )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);

            List<Step> steps = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>(stepIds) as List<Step>;

            await RemoveMissingSteps( item, steps );
        }

        private static async Task RemoveMissingSteps( MaintenanceItem item, IEnumerable<Step> steps )
        {
            foreach( Step step in item.Steps )
            {
                if( !steps.Contains( step ) )
                    await RemoveStep( item, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Removes the <see cref="Step"/>s with the passed in Ids from the <see cref="MaintenanceItem.Steps"/> collection if they exist and updates the Db. ONLY UPDATES THE MAINTENANCE ITEM, DOES NOT UPDATE STEP DB RECORDS.
        /// </summary>
        public static async Task RemoveSteps( int maintenanceItemId, IEnumerable<int> stepIds )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);

            List<Step> steps = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>(stepIds) as List<Step>;

            await RemoveSteps( item, steps );
        }

        private static async Task RemoveSteps( MaintenanceItem item, IEnumerable<Step> steps )
        {
            foreach( Step step in steps )
            {
                if( item.Steps.Contains( step ) )
                    await RemoveStep( item, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( item );
        }

#nullable enable

        /// <summary>
        /// Populates any non-null properties with the passed in value and updates the database.
        /// </summary>
        public static async Task UpdateProperties( int maintenanceItemId, string? name = null, DateTime? firstServiceDate = null, string? comment = null, int? recursEvery = null, int? timeframe = null, bool? notifyOfNextServiceDate = null )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);

            item.Name = name ?? item.Name;
            item.FirstServiceDate = firstServiceDate ?? item.FirstServiceDate;
            item.Comment = comment ?? item.Comment;
            item.RecursEvery = recursEvery ?? item.RecursEvery;
            item.Timeframe = timeframe ?? item.Timeframe;
            item.NotifyOfNextServiceDate = notifyOfNextServiceDate ?? item.NotifyOfNextServiceDate;

            if( item.ServiceRecords.Count > 0 )
            {
                item.NextServiceDate = CalculateNextServiceDate( item.ServiceRecords[^1].ServiceCompletionDate, item.RecursEvery, item.Timeframe );
            }
            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Populates any non-null properties with the passed in value and updates the database.
        /// </summary>
        public static async Task UpdateServiceRecord( int serviceRecordId, bool? serviceCompleted = null, DateTime? serviceCompletionDate = null )
        {
            ServiceRecord record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>(serviceRecordId);

            record.ServiceCompleted = serviceCompleted ?? record.ServiceCompleted;
            record.ServiceCompletionDate = serviceCompletionDate ?? record.ServiceCompletionDate;

            await DbServiceLocator.UpdateItemAsync( record );
        }
#nullable disable

        /// <summary>
        /// Inserts a new ServiceRecord to the <see cref="MaintenanceItem.ServiceRecords"/> collection with the passed in values. Updates both the MaintenanceItem and ServiceRecord in the database.
        /// </summary>
        public static async Task<ServiceRecord> InsertServiceRecord( int maintenanceItemId, bool serviceCompleted, DateTime completionDate )
        {
            MaintenanceItem item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>(maintenanceItemId);

            ServiceRecord record = new ServiceRecord()
            {
                Name = $"{item.Name}_Service_{item.ServiceRecords.Count + 1}",
                ServiceCompleted = serviceCompleted,
                TargetServiceCompletionDate = (DateTime)item.NextServiceDate,
                ServiceCompletionDate = completionDate,
                CreatedOn = DateTime.Now,
                Item = item,
                MaintenanceItemId = item.Id
            };

            int recordId = await DbServiceLocator.AddItemAndReturnIdAsync( record );
            record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>( recordId );

            item.ServiceRecords.Add( record );

            if( serviceCompleted )
            {
                DateTime? nextServiceDate = CalculateNextServiceDate( DateTime.Now, item.RecursEvery, item.Timeframe );

                item.NextServiceDate = nextServiceDate;
            }
            await DbServiceLocator.UpdateItemAsync( item );

            return record;
        }

        public static async Task<MaintenanceItem> GetItemAsync( int maintenanceItemId )
        {
            return await DbServiceLocator.GetItemAsync<MaintenanceItem>( maintenanceItemId );
        }

        public static async Task<MaintenanceItem> GetItemRecursiveAsync( int maintenanceItemId )
        {
            return await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( maintenanceItemId );
        }

        public static async Task<List<MaintenanceItem>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<MaintenanceItem>() as List<MaintenanceItem>;
        }

        public static async Task<List<MaintenanceItem>> GetAllItemsRecursiveAsync()
        {
            return await DbServiceLocator.GetAllItemsRecursiveAsync<MaintenanceItem>() as List<MaintenanceItem>;
        }

        public static async Task<ServiceRecord> GetServiceRecordAsync( int recordId )
        {
            return await DbServiceLocator.GetItemAsync<ServiceRecord>( recordId );
        }

        public static async Task<ServiceRecord> GetServiceRecordRecursiveAsync( int recordId )
        {
            ServiceRecord record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>( recordId );

            record.Item ??= await DbServiceLocator.GetItemAsync<MaintenanceItem>( record.MaintenanceItemId );

            return record;
        }

        public static async Task<List<ServiceRecord>> GetAllServiceRecordsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<ServiceRecord>() as List<ServiceRecord>;
        }


        /// <summary>
        /// Gets all ServiceRecords and their associated MaintenanceItems from the database. This is a memory intensive call, so it is probably better to find the ServiceRecord/MaintenanceItem you want some other way.
        /// </summary>
        public static async Task<List<ServiceRecord>> GetAllServiceRecordsRecursiveAsync()
        {
            List<ServiceRecord> records = await DbServiceLocator.GetAllItemsRecursiveAsync<ServiceRecord>() as List<ServiceRecord>;

            foreach( ServiceRecord record in records )
            {
                record.Item ??= await DbServiceLocator.GetItemAsync<MaintenanceItem>( record.MaintenanceItemId );
            }

            return records;
        }

#nullable enable

        public static async Task UpdateItemStepsAsync( int itemId, IEnumerable<int> stepIds )
        {
            MaintenanceItem item = await GetItemRecursiveAsync(itemId);
            List<Step> steps = await StepManager.GetItemRangeRecursiveAsync( stepIds ) as List<Step>;

            await UpdateItemStepsAsync( item, steps );
        }

        private static async Task UpdateItemStepsAsync( MaintenanceItem item, IEnumerable<Step> steps )
        {
            item.Steps = steps as List<Step>;
            await DbServiceLocator.UpdateItemAsync( item );
        }

#nullable disable

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task DeleteItem( int id )
        {
            throw new NotImplementedException();
        }
    }
}
