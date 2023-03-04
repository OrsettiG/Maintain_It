using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;

namespace Maintain_it.Helpers
{
    public static class ServiceItemManager
    {
        public static async Task<int> NewMaintenanceItem( string name, DateTime firstServiceDate, string comment = "", int recursEvery = 0, bool hasServiceLimit = false, int timesToRepeatService = 0, int serviceTimeframe = 3, int timesToRemind = 0, bool notifyOfNextServiceDate = false, int advanceNotice = 1, int noticeTimeframe = 3, IEnumerable<int> stepIds = null, int? isActive = null )
        {
            ServiceItem item = new ServiceItem()
            {
                Name = name,
                NextServiceDate = Config.DefaultServiceDateTime,
                //NextServiceDate = CalculateNextServiceDate( Config.DefaultServiceDateTime, recursEvery, serviceTimeframe ),
                Comment = comment,
                RecursEvery = recursEvery,
                ServiceTimeframe = serviceTimeframe,
                HasServiceLimit = hasServiceLimit,
                TimesToRepeatService = timesToRepeatService,
                NotifyOfNextServiceDate = notifyOfNextServiceDate,
                CreatedOn = DateTime.UtcNow,
                Steps = new List<Step>(),
                ServiceRecords = new List<ServiceRecord>(),
                NotificationEventArgsId = 0,
                AdvanceNotice = advanceNotice,
                NoticeTimeframe = noticeTimeframe,
                ActiveState = isActive ?? (int)ActiveStateFlag.Active
            };

            if( notifyOfNextServiceDate )
            {
                item.NotificationEventArgsId = await LocalNotificationManager.GetNewScheduledNotification( item.Name, Config.DefaultServiceDateTime, advanceNotice, noticeTimeframe, timesToRemind );
            }

            if( stepIds != null )
                item.Steps = await UpdateStepSequence( stepIds );

            int id = await DbServiceLocator.AddItemAndReturnIdAsync( item );

            await InsertServiceRecord( id );

            return id;

        }

        /// <summary>
        /// Calculates the next service date based on the passed in recurrance parameters.
        /// </summary>
        /// <returns>The next service date if recurrance params allow it, null otherwise.</returns>
        private static DateTime CalculateNextServiceDate( DateTime lastServiceDate, int recursEvery, int timeframe )
        {
            if( timeframe == 0 || recursEvery == 0 )
                return DateTime.MinValue;

            Timeframe Timeframe = (Timeframe)timeframe;

            DateTime nextServiceDate = Timeframe switch
            {
                Timeframe.Minutes => lastServiceDate.AddMinutes( recursEvery ),
                Timeframe.Hours => lastServiceDate.AddHours( recursEvery ),
                Timeframe.Days => lastServiceDate.AddDays( recursEvery ),
                Timeframe.Weeks => lastServiceDate.AddDays( recursEvery * 7 ),
                Timeframe.Months => lastServiceDate.AddMonths( recursEvery ),
                Timeframe.Years => lastServiceDate.AddYears( recursEvery ),
                _ => DateTime.MinValue
            };

            return nextServiceDate;
        }

        /// <summary>
        /// Retrieves the passed in <see cref="ServiceItem"/> and <see cref="Step"/> from the Db, adds the Step to the ServiceItem's Steps List, if it is not already there, and updates the Db. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task AddStep( int maintenanceItemId, int stepId )
        {
            ServiceItem item = await GetItemRecursiveAsync( maintenanceItemId );

            Step step = await StepManager.GetItemRecursiveAsync( stepId );

            await AddStep( item, step );
        }

        /// <summary>
        /// Adds the Step to the ServiceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        private static async Task AddStep( ServiceItem item, Step step, bool updateDb = true )
        {
            if( !item.Steps.Contains( step ) )
            {
                item.Steps.Add( step );

                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( item );
            }
        }

        public static async Task<List<Step>> UpdateStepSequence( IEnumerable<int> stepIds, int? startingIndex = null )
        {
            List<Step> steps = await StepManager.GetItemRangeRecursiveAsync(stepIds) as List<Step>;

            steps = await UpdateStepSequence( steps, startingIndex );
            return steps;
        }

        private static async Task<List<Step>> UpdateStepSequence( IEnumerable<Step> newSteps, int? startingIndex = null )
        {
            IOrderedEnumerable<Step> steps = newSteps.OrderBy(x => x.Index);
            List<Step> updatedSteps = new List<Step>();

            switch( newSteps.Count() )
            {
                case 0:
                    return updatedSteps;
                case 1:
                    updatedSteps.Add( steps.FirstOrDefault() );
                    return updatedSteps;
            }


            Step upStream = null;

            foreach( Step step in steps )
            {
                if( startingIndex != null )
                {
                    step.Index = startingIndex.Value;
                    startingIndex++;
                }

                // Set the step's previous node to the upStream node. This will always be null on the first node so we need the null propogation on the id.
                step.PreviousNodeId = upStream != null ? upStream.Id : 0;

                // Set the upStream node's next node to this one. This will be null on the first iteration so we have to null check.
                switch( upStream )
                {
                    case null:
                        break;

                    default:
                        upStream.NextNodeId = step.Id;
                        break;
                }

                // Now that upStream is all tied up, we can update it in the db.
                if( upStream != null )
                {
                    updatedSteps.Add( upStream );
                    await DbServiceLocator.UpdateItemAsync( upStream );
                }

                // Set this nodes next node to null. If there is another node in the sequence, this will be set to the correct node on the next loop and if we are on the last node then we want this to be null anyways.
                step.NextNodeId = 0;

                // Set the upStream node to this one
                upStream = step;
            }

            updatedSteps.Add( upStream );
            await DbServiceLocator.UpdateItemAsync( upStream );

            return updatedSteps;
        }




        /// <summary>
        /// Retrieves the passed in <see cref="ServiceItem"/> and <see cref="Step"/> from the Db. Adds the Step to the ServiceItem's Steps List, if it is not already there, and updates the Db. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task AddSteps( int itemId, IEnumerable<int> stepIds )
        {
            ServiceItem item = await GetItemRecursiveAsync(itemId);
            List<Step> steps = await StepManager.GetItemRange( stepIds );

            await AddSteps( item, steps, false );

            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Adds the Step to the ServiceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        private static async Task AddSteps( ServiceItem item, IEnumerable<Step> steps, bool updateDb = true )
        {
            foreach( Step step in steps )
            {
                await AddStep( item, step, false );
            }

            if( updateDb )
                await DbServiceLocator.UpdateItemAsync( item );
        }

        public static async Task<bool> RemoveStep( int itemId, int stepId )
        {
            ServiceItem item = await GetItemRecursiveAsync(itemId);

            return await RemoveStep( item, stepId );
        }

        private static async Task<bool> RemoveStep( ServiceItem item, int stepId )
        {
            if( item.Steps.RemoveAll( x => x.Id == stepId ) > 0 )
            {
                List<Step> steps = await UpdateStepSequence(item.Steps.GetIds(), 1);
                await UpdateProperties( item.Id, steps: steps.GetIds() );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the passed in <see cref="ServiceItem"/> and <see cref="Step"/> from the Db. Adds the Step to the ServiceItem's Steps List, if it is not already there, and updates the Db if specified. DOES NOT UPDATE THE PASSED IN STEP, MAKE SURE TO UPDATE IT SEPARATELY.
        /// </summary>
        public static async Task UpdateSteps( int maintenanceItemId, IEnumerable<int> stepIds )
        {
            ServiceItem item = await DbServiceLocator.GetItemRecursiveAsync<ServiceItem>(maintenanceItemId);

            item.Steps = await UpdateStepSequence( stepIds );
            await DbServiceLocator.UpdateItemAsync( item );
        }



#nullable enable

        /// <summary>
        /// Populates any non-null properties with the passed in value and updates the database.
        /// </summary>
        public static async Task UpdateProperties( int maintenanceItemId, string? name = null, DateTime? serviceDate = null, string? comment = null, int? recursEvery = null, int? timeframe = null, bool? notifyOfNextServiceDate = null, bool? hasServiceLimit = null, int? timesToRepeatService = null, int? advanceNotice = null, int? advanceNoticeTimeframe = null, int? noticeTimeframe = null, bool? notificationActive = null, int? reminders = null, IEnumerable<int>? steps = null, int? isActive = null )
        {
            ServiceItem item = await DbServiceLocator.GetItemRecursiveAsync<ServiceItem>(maintenanceItemId);

            item.Name = name ?? item.Name;
            item.NextServiceDate = serviceDate ?? item.NextServiceDate;
            item.Comment = comment ?? item.Comment;
            item.RecursEvery = recursEvery ?? item.RecursEvery;
            item.ServiceTimeframe = timeframe ?? item.ServiceTimeframe;
            item.NotifyOfNextServiceDate = notifyOfNextServiceDate ?? item.NotifyOfNextServiceDate;
            item.HasServiceLimit = hasServiceLimit ?? item.HasServiceLimit;
            item.TimesToRepeatService = timesToRepeatService ?? item.TimesToRepeatService;
            item.AdvanceNotice = advanceNotice ?? item.AdvanceNotice;
            item.NoticeTimeframe = noticeTimeframe ?? item.NoticeTimeframe;
            item.ActiveState = isActive ?? item.ActiveState;

            if( steps != null )
            {
                item.Steps = await StepManager.GetItemRangeRecursiveAsync( steps ) as List<Step>;

                item.Steps = await UpdateStepSequence( item.Steps );
                (item.ServiceCompletionTimeEst, item.ServiceCompletionTimeEstTimeframe) = CalculateServiceCompletionTimeEstimate( item.Steps );
            }

            if( notifyOfNextServiceDate.HasValue && notifyOfNextServiceDate.Value )
            {
                if( item.NotificationEventArgsId > 0 )
                {
                    NotificationEventArgs args = await DbServiceLocator.GetItemAsync<NotificationEventArgs>(item.NotificationEventArgsId);

                    args.Active = notificationActive ?? args.Active;
                    args.TimesToRemind = reminders ?? args.TimesToRemind;
                    args.NotifyTime = serviceDate ?? args.NotifyTime;

                    await LocalNotificationManager.UpdateScheduledNotification( item.NotificationEventArgsId, item.Name, serviceDate ?? item.NextServiceDate, advanceNotice ?? 2, advanceNoticeTimeframe ?? (int)Timeframe.Days, true, reminders ?? int.MinValue );

                    await DbServiceLocator.UpdateItemAsync( args );
                }
                else
                {
                    item.NotificationEventArgsId = await LocalNotificationManager.GetNewScheduledNotification(
                        item.Name,
                        item.NextServiceDate,
                        advanceNotice ?? Config.DefaultAdvanceNotice,
                        advanceNoticeTimeframe ?? (int)Config.DefaultNoticeTimeframe,
                        reminders ?? int.MinValue );
                }
            }

            await DbServiceLocator.UpdateItemAsync( item );
        }

        private static (double, int) CalculateServiceCompletionTimeEstimate( IEnumerable<Step> Steps )
        {
            double minutes = 0;
            TimeInMinutes largestTimeframe = TimeInMinutes.None;

            foreach( Step step in Steps )
            {
                switch( (Timeframe)step.Timeframe )
                {
                    case Timeframe.Minutes:
                        minutes += step.TimeRequired;

                        if( largestTimeframe < TimeInMinutes.Minutes )
                            largestTimeframe = TimeInMinutes.Minutes;
                        break;

                    case Timeframe.Hours:
                        minutes += step.TimeRequired * 60;

                        if( largestTimeframe < TimeInMinutes.Hours )
                            largestTimeframe = TimeInMinutes.Hours;
                        break;

                    case Timeframe.Days:
                        minutes += step.TimeRequired * 1440;

                        if( largestTimeframe < TimeInMinutes.Days )
                            largestTimeframe = TimeInMinutes.Days;
                        break;

                    case Timeframe.Weeks:
                        minutes += step.TimeRequired * 10080;

                        if( largestTimeframe < TimeInMinutes.Weeks )
                            largestTimeframe = TimeInMinutes.Weeks;
                        break;

                    case Timeframe.Months:
                        minutes += step.TimeRequired * 43800;
                        if( largestTimeframe < TimeInMinutes.Months )
                            largestTimeframe = TimeInMinutes.Months;
                        break;

                    case Timeframe.Years:
                        minutes += step.TimeRequired * 525600;
                        if( largestTimeframe < TimeInMinutes.Years )
                            largestTimeframe = TimeInMinutes.Years;
                        break;
                }
            }

            return (Math.Round( minutes / (int)largestTimeframe, 1 ), (int)largestTimeframe);
            //CompletionTimeEstimate = largestTimeframe != TimeInMinutes.None ? $"{Math.Round( minutes / (int)largestTimeframe, 1 )} {largestTimeframe}" : "Unavailable";
        }

        /// <summary>
        /// Populates any non-null properties with the passed in value and updates the database.
        /// </summary>
        public static async Task UpdateServiceRecord( int serviceRecordId, bool? serviceCompleted = null, bool? serviceStarted = null, int? currentStepIndex = null, double? serviceTime = null )
        {
            ServiceRecord record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>(serviceRecordId);

            record.ServiceCompleted = serviceCompleted ?? record.ServiceCompleted;

            record.ServiceStarted = ( serviceStarted ?? record.ServiceCompleted ) || record.ServiceStarted;

            record.CurrentStepIndex = currentStepIndex ?? record.CurrentStepIndex;

            record.ActualServiceCompletionDate = serviceCompleted == true ? DateTime.UtcNow : record.ActualServiceCompletionDate;

            await DbServiceLocator.UpdateItemAsync( record );
        }
#nullable disable

        public static async Task CompleteMaintenance( int maintenanceItemId, double timeTaken )
        {
            ServiceItem item = await GetItemRecursiveAsync( maintenanceItemId );
            ServiceRecord record = item.ServiceRecords.Last();

            await UpdateServiceRecord( record.Id, true, serviceTime: timeTaken );

            DateTime nextServiceDate = default;
            if( item.TimesToRepeatService == 0 || item.ServiceRecords.Count < item.TimesToRepeatService )
            {
                nextServiceDate = CalculateNextServiceDate( DateTime.UtcNow, item.RecursEvery, item.ServiceTimeframe );
            }

            /* Should a new service record be inserted here, or when the user starts the service?
             * It probably makes sense to insert the record when the user starts a new service because that way we always know that the last record is current and complete/in progress. If we insert a new record now then we need to check every time we get the last service record whether or not that service record is the one we want. Whereas if we insert the new service record when the users starts a service, we know that the last record has the data for the most recently started/completed service, even if we access is 2 months later.
             */

            if( nextServiceDate > DateTime.MinValue )
            {
                await LocalNotificationManager.UpdateScheduledNotification( item.NotificationEventArgsId, item.Name, nextServiceDate, item.AdvanceNotice, item.NoticeTimeframe, true, int.MinValue );

                foreach( Step step in item.Steps )
                {
                    await StepManager.UpdateItemAsync( step.Id, isCompleted: false );
                }
            }

            item.NextServiceDate = nextServiceDate;
            await DbServiceLocator.UpdateItemAsync( item );
        }

        /// <summary>
        /// Inserts a new ServiceRecord to the <see cref="ServiceItem.ServiceRecords"/> collection with the passed in values. Updates both the ServiceItem and ServiceRecord in the database.
        /// </summary>
        public static async Task<ServiceRecord> InsertServiceRecord( int maintenanceItemId, bool serviceCompleted = false, bool serviceStarted = false, int currentStepIndex = 1 )
        {
            ServiceItem item = await GetItemRecursiveAsync(maintenanceItemId);

            ServiceRecord record = new ServiceRecord();

            record.Name = $"{item.Name}_Service#_{item.ServiceRecords.Count + 1}";
            record.ServiceCompleted = serviceCompleted;
            record.ServiceStarted = serviceStarted;
            record.ServiceTime = 0;
            record.CurrentStepIndex = currentStepIndex;
            record.TargetServiceCompletionDate = item.NextServiceDate;
            record.ActualServiceCompletionDate = serviceCompleted ? DateTime.UtcNow : DateTime.MinValue;
            record.CreatedOn = DateTime.UtcNow;
            record.Item = item;


            int recordId = await DbServiceLocator.AddItemAndReturnIdAsync( record );
            record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>( recordId );

            item.ServiceRecords.Add( record );

            // We don't need to manage the next service date since service records are only inserted when maintenance starts, and are updated when maintenence is completed.
            //if( serviceCompleted )
            //{
            //    item.NextServiceDate = CalculateNextServiceDate( DateTime.UtcNow, item.RecursEvery, item.ServiceTimeframe );
            //}
            await DbServiceLocator.UpdateItemAsync( item );

            return record;
        }

        public static async Task<ServiceItem> GetItemAsync( int maintenanceItemId )
        {
            return await DbServiceLocator.GetItemAsync<ServiceItem>( maintenanceItemId );
        }

        public static async Task<ServiceItem> GetItemRecursiveAsync( int maintenanceItemId )
        {

            return await DbServiceLocator.GetItemRecursiveAsync<ServiceItem>( maintenanceItemId );
        }

        public static async Task<List<ServiceItem>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<ServiceItem>() as List<ServiceItem>;
        }

        public static async Task<List<ServiceItem>> GetAllItemsRecursiveAsync()
        {
            return await DbServiceLocator.GetAllItemsRecursiveAsync<ServiceItem>() as List<ServiceItem>;
        }

        public static async Task<ServiceRecord> GetServiceRecordAsync( int recordId )
        {

            ServiceRecord record = await DbServiceLocator.GetItemAsync<ServiceRecord>( recordId );

            return record;
        }

        public static async Task<ServiceRecord> GetServiceRecordRecursiveAsync( int recordId )
        {
            ServiceRecord record = await DbServiceLocator.GetItemRecursiveAsync<ServiceRecord>( recordId );

            record.Item ??= await DbServiceLocator.GetItemAsync<ServiceItem>( record.MaintenanceItemId );

            return record;
        }

        public static async Task<List<ServiceRecord>> GetAllServiceRecordsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<ServiceRecord>() as List<ServiceRecord>;
        }


        /// <summary>
        /// Gets all ServiceRecords and their associated ServiceItems from the database. This is a memory intensive call, so it is probably better to find the ServiceRecord/ServiceItem you want some other way.
        /// </summary>
        public static async Task<List<ServiceRecord>> GetAllServiceRecordsRecursiveAsync()
        {
            List<ServiceRecord> records = await DbServiceLocator.GetAllItemsRecursiveAsync<ServiceRecord>() as List<ServiceRecord>;

            foreach( ServiceRecord record in records )
            {
                record.Item ??= await DbServiceLocator.GetItemAsync<ServiceItem>( record.MaintenanceItemId );
            }

            return records;
        }

        public static async Task<MaintenanceItemViewModel> GetItemAsViewModelAsync( int id )
        {
            if( id == 0 )
                return null;
            MaintenanceItemViewModel vm = new MaintenanceItemViewModel( id );

            await vm.InitCommand.ExecuteAsync();

            return vm;
        }

        public static async Task<List<MaintenanceItemViewModel>> GetItemRangeAsViewModelAsync( IEnumerable<int> ids )
        {
            List<MaintenanceItemViewModel> viewModels = new List<MaintenanceItemViewModel>();

            if( ids.Count() == 0 )
            {
                return viewModels;
            }

            foreach( int id in ids )
            {
                if( id == 0 )
                {
                    continue;
                }

                MaintenanceItemViewModel vm = new MaintenanceItemViewModel( id );
                viewModels.Add( vm );
                await vm.InitCommand.ExecuteAsync();
            };

            return viewModels;
        }

#nullable enable

        public static async Task UpdateItemStepsAsync( int itemId, IEnumerable<int> stepIds )
        {
            ServiceItem item = await GetItemRecursiveAsync(itemId);
            List<Step> steps = (List<Step>)await StepManager.GetItemRangeRecursiveAsync( stepIds );

            await UpdateItemStepsAsync( item, steps );
        }

        private static async Task UpdateItemStepsAsync( ServiceItem item, IEnumerable<Step> steps )
        {
            item.Steps = steps as List<Step>;
            await DbServiceLocator.UpdateItemAsync( item );
        }

#nullable disable

        /// <summary>
        /// Deletes the ServiceItem with the passed in id and all the unique data associated with it (does not delete LooseMaterials but does delete Steps, StepMaterials, Notes etc.)
        /// </summary>
        public static async Task DeleteItem( int id )
        {
            ServiceItem item = await GetItemRecursiveAsync(id);

            if( item.Steps != null && item.Steps.Count > 0 )
            {
                await StepManager.DeleteItemRange( item.Steps.GetIds() );
            }

            if( item.ServiceRecords != null && item.ServiceRecords.Count > 0 )
            {
                await DeleteServiceRecordRange( item.ServiceRecords );
            }
            // PICK UP HERE: Something is causing a null reference error after this point.
            if( item.NotificationEventArgsId > 0 )
            {
                await DbServiceLocator.DeleteItemAsync<NotificationEventArgs>( item.NotificationEventArgsId );
            }

            await DbServiceLocator.DeleteItemAsync<ServiceItem>( item.Id );
        }

        public static async Task DeleteServiceRecord( int id )
        {
            await DbServiceLocator.DeleteItemAsync<ServiceRecord>( id );
        }

        public static async Task DeleteServiceRecordRange( IEnumerable<int> ids )
        {
            foreach( int id in ids )
            {
                await DeleteServiceRecord( id );
            }
        }

        private static async Task DeleteServiceRecordRange( IEnumerable<ServiceRecord> records )
        {
            foreach( ServiceRecord record in records )
            {
                await DeleteServiceRecord( record.Id );
            }
        }

        internal static async Task<List<ServiceItem>> GetItemRangeRecursiveAsync( IEnumerable<int> serviceItemIds )
        {
            List<ServiceItem> items = await DbServiceLocator.GetItemRangeRecursiveAsync<ServiceItem>( serviceItemIds ) as List<ServiceItem>;

            return items;
        }
    }
}
