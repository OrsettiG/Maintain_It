using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;

using Xamarin.Essentials;

using static Maintain_it.Helpers.NullCheck;

namespace Maintain_it.Helpers
{
    internal static class StepManager
    {
        #region Step Creation

        /// <summary>
        /// Builds a new step and adds it to the database, returns the new step Id.Can be used to create empty steps if needed.
        /// </summary>
        /// <returns></returns>
        public static async Task<int> NewStep( bool isFirstStep, string name = "New Step", string description = "No Description", bool isCompleted = false, double timeRequired = 10, int timeframe = 0 )
        {
            Step step = new Step()
            {
                Name = name,
                Description = description,
                IsCompleted = isCompleted,
                TimeRequired = timeRequired,
                Timeframe = timeframe,
                NextNodeId = 0,
                PreviousNodeId = 0,
                StepMaterials = new List<StepMaterial>(),
                Notes = new List<Note>(),
                CreatedOn = DateTime.UtcNow
            };

            if( isFirstStep )
            {
                step.Index = 1;
            }

            int id = await AddItemAndReturnIdAsync( step );

            return id;
        }

        #endregion

        #region Property Management

        #region Update

#nullable enable
        /// <summary>
        /// Updates the passed in Step in the database.
        /// </summary>
        public static async Task UpdateItemAsync( int stepId, string? name = null, string? description = null, bool? isCompleted = null, double? timeRequired = null, int? timeFrame = null, int? index = null, int? nextNodeId = null, int? previousNodeId = null, int? maintenanceItemId = null, IEnumerable<int>? stepMaterialIds = null, IEnumerable<int>? noteIds = null )
        {
            Step step = await GetItemRecursiveAsync(stepId);

            await UpdateItemAsync( step, name, description, isCompleted, timeRequired, timeFrame, index, nextNodeId, previousNodeId, maintenanceItemId, stepMaterialIds, noteIds );
        }

        private static async Task UpdateItemAsync( Step item, string? name = null, string? description = null, bool? isCompleted = null, double? timeRequired = null, int? timeFrame = null, int? index = null, int? nextNodeId = null, int? previousNodeId = null, int? maintenanceItemId = null, IEnumerable<int>? stepMaterialIds = null, IEnumerable<int>? noteIds = null )
        {
            item.Name = name ?? item.Name;
            item.Description = description ?? item.Description;
            item.IsCompleted = isCompleted ?? item.IsCompleted;
            item.TimeRequired = timeRequired ?? item.TimeRequired;
            item.Timeframe = timeFrame ?? item.Timeframe;
            item.Index = index ?? item.Index;
            item.MaintenanceItemId = maintenanceItemId ?? item.MaintenanceItemId;

            if( stepMaterialIds != null )
                item.StepMaterials = await StepMaterialManager.GetItemRangeAsync( stepMaterialIds );

            if( noteIds != null )
                item.Notes = await NoteManager.GetItemRangeAsync( noteIds );

            await DbServiceLocator.UpdateItemAsync( item );
        }

#nullable disable

        public static async Task CompleteStep( int id )
        {
            Step step = await GetItemRecursiveAsync(id);

            await MaintenanceItemManager.UpdateServiceRecord( step.MaintenanceItem.ServiceRecords.Last().Id, false, true, step.Index );

            foreach( StepMaterial stepMaterial in step.StepMaterials )
            {
                int quantityOwned = stepMaterial.Material.QuantityOwned -= stepMaterial.Quantity;

                await MaterialManager.UpdateMaterial( stepMaterial.Material.Id, quantityOwned: quantityOwned );
            }

            await UpdateItemAsync( id, isCompleted: true );
        }


        #endregion
        #region MaintenanceItem Management
        /// <summary>
        /// Changes the passed in Step's MaintenanceItem to match the passed in stepId and updates database. DOES NOT MODIFY THE <see cref="MaintenanceItem"/> MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> UpdateMaintenanceItem( int maintenanceItemId, int stepId )
        {
            MaintenanceItem item = null;
            Step step = null;

            try
            {
                item = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( maintenanceItemId );

                step = await DbServiceLocator.GetItemRecursiveAsync<Step>( stepId );
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex.StackTrace );
            }

            return item != null && step != null && await UpdateMaintenanceItem( item, step );
        }

        /// <summary>
        /// Changes the passed in Step's MaintenanceItem to match the passed in stepId and updates database. DOES NOT MODIFY THE <see cref="MaintenanceItem"/> MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> UpdateMaintenanceItem( MaintenanceItem item, Step step )
        {
            if( step.MaintenanceItem != item || step.MaintenanceItemId != item.Id )
            {
                step.MaintenanceItemId = item.Id;
                step.MaintenanceItem = item;
                await DbServiceLocator.UpdateItemAsync( step );
                return true;
            }

            return false;
        }
        #endregion

        #region Step Material Management
        // STEP MATERIAL ADDITION

        /// <summary>
        /// Adds the passed in <see cref="StepMaterial"/> to the <see cref="Step"/>'s StepMaterials List and the passed in <see cref="Step"/> to the <see cref="StepMaterial"/>'s <see cref="Step"/> field and updates the database.
        /// </summary>
        /// <returns></returns>
        public static async Task AddStepMaterial( int stepMaterialId, int stepId, bool updateDb = true )
        {
            StepMaterial stepMaterial = await DbServiceLocator.GetItemRecursiveAsync<StepMaterial>(stepMaterialId);

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            await AddStepMaterial( stepMaterial, step, updateDb );
        }

        /// <summary>
        /// Adds the passed in <see cref="StepMaterial"/> to the <see cref="Step"/>'s StepMaterials List and updates the database DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>, MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task AddStepMaterial( StepMaterial stepMaterial, Step step, bool updateDb = true )
        {
            if( !step.StepMaterials.Contains( stepMaterial ) )
            {
                step.StepMaterials.Add( stepMaterial );
            }

            if( updateDb )
            {
                await DbServiceLocator.UpdateItemAsync( step );
            }
        }

        /// <summary>
        /// Adds the each of the passed in <see cref="StepMaterial"/>s to the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        /// <returns></returns>
        public static async Task AddStepMaterials( IEnumerable<int> stepMaterialIds, int stepId )
        {
            List<StepMaterial> stepMaterials = await DbServiceLocator.GetItemRangeRecursiveAsync<StepMaterial>(stepMaterialIds) as List<StepMaterial>;

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            foreach( StepMaterial stepMaterial in stepMaterials )
            {
                await AddStepMaterial( stepMaterial, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( step );
        }



        /// <summary>
        /// Adds the each of the passed in <see cref="StepMaterial"/>s to the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task AddStepMaterials( IEnumerable<StepMaterial> stepMaterials, Step step, bool updateDb = true )
        {
            foreach( StepMaterial stepMaterial in stepMaterials )
            {
                await AddStepMaterial( stepMaterial, step, false );
            }

            if( updateDb )
                await DbServiceLocator.UpdateItemAsync( step );
        }

        // STEP MATERIAL REMOVAL

        /// <summary>
        /// Removes the passed in <see cref="StepMaterial"/> from the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>, MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        public static async Task RemoveStepMaterial( int stepMaterialId, int stepId, bool updateDb = true )
        {
            StepMaterial stepMaterial = await DbServiceLocator.GetItemRecursiveAsync<StepMaterial>(stepMaterialId);

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            await RemoveStepMaterial( stepMaterial, step, updateDb );
        }

        /// <summary>
        /// Removes the passed in <see cref="StepMaterial"/> from the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>, MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task RemoveStepMaterial( StepMaterial stepMaterial, Step step, bool updateDb = true )
        {
            if( step.StepMaterials.Contains( stepMaterial ) )
            {
                _ = step.StepMaterials.RemoveAll( x => x.Id == stepMaterial.Id );
            }

            if( updateDb )
            {
                await DbServiceLocator.UpdateItemAsync( step );
            }
        }

        /// <summary>
        /// Removes the each of the passed in <see cref="StepMaterial"/>s from the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>s, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        /// <returns></returns>
        public static async Task RemoveStepMaterials( IEnumerable<int> stepMaterialIds, int stepId )
        {
            List<StepMaterial> stepMaterials = await DbServiceLocator.GetItemRangeRecursiveAsync<StepMaterial>(stepMaterialIds) as List<StepMaterial>;

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            foreach( StepMaterial stepMaterial in stepMaterials )
            {
                await RemoveStepMaterial( stepMaterial, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( step );
        }

        /// <summary>
        /// Removes the each of the passed in <see cref="StepMaterial"/>s from the <see cref="Step"/>'s StepMaterials List and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="StepMaterial"/>s, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task RemoveStepMaterials( IEnumerable<StepMaterial> stepMaterials, Step step )
        {
            foreach( StepMaterial stepMaterial in stepMaterials )
            {
                await RemoveStepMaterial( stepMaterial, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( step );
        }
        #endregion

        #region Note Management
        // NOTE INSERTION
        /// <summary>
        /// Adds the passed in <see cref="Note"/> to the <see cref="Step"/>'s Notes List and the passed in <see cref="Step"/> to the <see cref="Note"/>'s <see cref="Step"/> field and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="Note"/>, MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        public static async Task AddNote( int noteId, int stepId, bool updateDb = true )
        {
            Note note = await DbServiceLocator.GetItemRecursiveAsync<Note>(noteId);

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            await AddNote( note, step, updateDb );
        }

        /// <summary>
        /// Adds the passed in <see cref="Note"/> to the <see cref="Step"/>'s Notes List and the passed in <see cref="Step"/> to the <see cref="Note"/>'s <see cref="Step"/> field and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="Note"/>, MAKE SURE TO UPDATE THAT AS WELL.
        /// </summary>
        /// <returns></returns>
        private static async Task AddNote( Note note, Step step, bool updateDb = true )
        {
            if( !step.Notes.Contains( note ) )
            {
                step.Notes.Add( note );
            }

            if( updateDb )
            {
                await DbServiceLocator.UpdateItemAsync( step );
            }
        }

        /// <summary>
        /// Adds the passed in <see cref="Note"/> to the <see cref="Step"/>'s Notes List and the passed in <see cref="Step"/> to the <see cref="Note"/>'s <see cref="Step"/> field and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="Note"/>s, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        private static async Task AddNotes( IEnumerable<Note> notes, Step step )
        {
            foreach( Note note in notes )
            {
                await AddNote( note, step, false );
            }

            await DbServiceLocator.UpdateItemAsync( step );
        }

        /// <summary>
        /// Adds the passed in <see cref="Note"/> to the <see cref="Step"/>'s Notes List and the passed in <see cref="Step"/> to the <see cref="Note"/>'s <see cref="Step"/> field and updates the database. DOES NOT MODIFY THE PASSED IN <see cref="Note"/>s, MAKE SURE TO UPDATE THOSE AS WELL.
        /// </summary>
        public static async Task AddNotes( IEnumerable<int> noteIds, int stepId )
        {
            List<Note> notes = await DbServiceLocator.GetItemRangeRecursiveAsync<Note>(noteIds) as List<Note>;

            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>(stepId);

            await AddNotes( notes, step );
        }

        // NOTE REMOVAL

        public static async Task DeleteNote( int noteId )
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Node Management
        public static async Task UpdateItemIndexAsync( int stepId, int newIndex, int? newNextNodeId = null, int? newPreviousNodeId = null )
        {
            Step step = await GetItemRecursiveAsync( stepId );
            await UpdateItemIndexAsync( step, newIndex, newNextNodeId, newPreviousNodeId );
        }

#nullable enable
        private static async Task UpdateItemIndexAsync( Step node, int newIndex, int? newNextNodeId = null, int? newPreviousNodeId = null, Step? nextNode = null, Step? previousNode = null )
        {
            node.Index = newIndex;
            node.NextNodeId = newNextNodeId ?? node.NextNodeId;
            node.PreviousNodeId = newPreviousNodeId ?? node.PreviousNodeId;

            await DbServiceLocator.UpdateItemAsync( node );
        }
#nullable disable

        public static async Task UpdatePrevAndNextNodes( Step node )
        {
            Step item = await GetItemRecursiveAsync(node.Id);

            item.NextNodeId = node.NextNodeId;

            item.PreviousNodeId = node.PreviousNodeId;


            await DbServiceLocator.UpdateItemAsync( item );
        }
        #endregion

        #endregion

        #region Item Retrieval

        public static async Task<List<Step>> GetItemRange( IEnumerable<int> stepIds )
        {
            return await DbServiceLocator.GetItemRangeAsync<Step>( stepIds ) as List<Step>;
        }

        public static async Task<List<StepViewModel>> GetItemRangeAsViewModel( IEnumerable<int> stepIds, MaintenanceItemViewModel mIVM )
        {
            List<StepViewModel> vms = new List<StepViewModel>();
            if( stepIds.Count() == 0 )
            {
                return vms;
            }
            List<Step> steps = await GetItemRangeRecursiveAsync(stepIds) as List<Step>;

            foreach( Step step in steps )
            {
                StepViewModel vm = CreateViewModel(step, mIVM);
                await vm.DeepInitAsync();
                vms.Add( vm );
            };

            return vms;
        }

        public static async Task<StepViewModel> GetItemAsViewModel( int id, MaintenanceItemViewModel mIVM )
        {
            Step step = await GetItemRecursiveAsync(id);

            StepViewModel vm = CreateViewModel(step, mIVM);

            await vm.DeepInitAsync();

            return vm;
        }

        private static StepViewModel CreateViewModel( Step step, MaintenanceItemViewModel mIVM )
        {
            StepViewModel vm = new StepViewModel(mIVM)
            {
                Step = step,
                Name = step.Name,
                Description = step.Description,
                TimeRequired = step.TimeRequired,
                Timeframe = (Timeframe)step.Timeframe,
                IsCompleted = step.IsCompleted,
                StepNum = step.Index
            };

            return vm;
        }

        public static async Task<Step> GetItemAsync( int stepId )
        {
            return await DbServiceLocator.GetItemAsync<Step>( stepId );
        }

        public static async Task<Step> GetItemRecursiveAsync( int stepId )
        {
            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>( stepId );

            step.MaintenanceItem = step.MaintenanceItemId == 0 ? null : await MaintenanceItemManager.GetItemAsync( step.MaintenanceItemId );

            foreach( StepMaterial stepMat in step.StepMaterials )
            {
                stepMat.Material = await MaterialManager.GetItemAsync( stepMat.MaterialId );
            }

            return step;
        }

        internal static async Task<IEnumerable<Step>> GetItemRangeRecursiveAsync( IEnumerable<int> stepIds )
        {
            List<Step> steps = new List<Step>();

            foreach( int id in stepIds )
            {
                steps.Add( await GetItemRecursiveAsync( id ) );
            }

            return steps;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<List<Step>> GetAllItemsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task<List<Step>> GetAllItemsRecursiveAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Item Insertion

        private static async Task<int> AddItemAndReturnIdAsync( Step step )
        {
            return await DbServiceLocator.AddItemAndReturnIdAsync( step );
        }

        #endregion

        #region Item Deletion

        /// <summary>
        /// Deletes the step from the database along with any associated notes.
        /// </summary>
        public static async Task DeleteItem( int id )
        {
            Step step = await GetItemRecursiveAsync(id);
            await DeleteItem( step );
        }

        /// <summary>
        /// Deletes all steps from database along with any associated notes.
        /// </summary>
        /// <param name="ids"></param>
        public static async Task DeleteItemRange( IEnumerable<int> ids )
        {
            List<Step> steps = await GetItemRangeRecursiveAsync(ids) as List<Step>;

            foreach( Step step in steps )
            {
                await DeleteItem( step );
            }
        }

        /// <summary>
        /// Deletes the step and its notes from the database.
        /// </summary>
        private static async Task DeleteItem( Step step )
        {
            if( step.Notes != null && step.Notes.Count > 0 )
            {
                foreach( Note note in step.Notes )
                {
                    await DbServiceLocator.DeleteItemAsync<Note>( note.Id );
                }
            }

            if( step.StepMaterials != null && step.StepMaterials.Count > 0 )
            {
                await StepMaterialManager.DeleteItemRange( step.StepMaterials.GetIds() );
            }

            if( step.PreviousNodeId != 0 )
            {
                Step preStep = await GetItemRecursiveAsync( step.PreviousNodeId );
                preStep.NextNodeId = step.NextNodeId;
            }

            if( step.NextNodeId != 0 )
            {
                Step nextStep = await GetItemRecursiveAsync( step.NextNodeId );
                nextStep.PreviousNodeId = step.PreviousNodeId;

                _ = await UpdateAllIndicesInStepSequence( step.NextNodeId, step.Index );
            }

            await DbServiceLocator.DeleteItemAsync<Step>( step.Id );
        }

        /// <summary>
        /// Updates all Steps in the sequence from this step onwards with a new index (the passed in index + however many iterations)
        /// </summary>
        /// <param name="fromStepId">The step to start updating indices at (inclusive)</param>
        /// <param name="index">The index to start updating at</param>
        /// <returns>true if the sequence was successfully updated, false otherwise.</returns>
        private static async Task<bool> UpdateAllIndicesInStepSequence( int fromStepId, int index )
        {
            Step step = await GetItemAsync( fromStepId );

            int nextNodeId = await UpdateStepIndex( step, index );

            return nextNodeId == 0 || await UpdateAllIndicesInStepSequence( nextNodeId, index + 1 );
        }

        /// <summary>
        /// Updates the Step index to the passed in index value and outs the nextNodeId of the passed in step.
        /// </summary>
        /// <param name="step">The Step to update the index of</param>
        /// <param name="newIndex">The index that the pass in step should be changed to</param>
        /// <returns>The NextNodeId value of the step</returns>
        private static async Task<int> UpdateStepIndex( Step step, int newIndex )
        {
            //PICK UP HERE
            await UpdateItemAsync( step.Id, index: newIndex );
            return step.NextNodeId;
        }

        #endregion
    }
}
