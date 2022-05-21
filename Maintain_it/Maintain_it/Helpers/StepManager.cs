using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

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
                CreatedOn = DateTime.Now
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
            //item.PreviousNodeId = previousNodeId ?? item.PreviousNodeId;
            //item.NextNodeId = nextNodeId ?? item.NextNodeId;
            item.MaintenanceItemId = maintenanceItemId ?? item.MaintenanceItemId;

            if( stepMaterialIds != null )
                item.StepMaterials = await StepMaterialManager.GetItemRangeAsync( stepMaterialIds );
            if( noteIds != null )
                item.Notes = await NoteManager.GetItemRangeAsync( noteIds );

            await DbServiceLocator.UpdateItemAsync( item );
        }

#nullable disable
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
        public static async Task UpdateItemIndexAsync(int stepId, int newIndex, int? newNextNodeId = null, int? newPreviousNodeId = null)
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



        //public static async Task DeReference( int stepId, int maintenanceItemId )
        //{
        //    Step step = await GetItemAsync( stepId );
        //    MaintenanceItem item = await MaintenanceItemManager.GetItemRecursiveAsync( maintenanceItemId );

        //    await InsertStepIntoSequenceAfter( step.PreviousNode.Id, step.NextNode.Id );
        //}
        #endregion

        #endregion

        #region Sequence Management

        /// <summary>
        /// Links the two steps NextNode and PreviousNode properties and updates the database.
        /// </summary>
        /// <returns></returns>
        //public static async Task InsertStepIntoSequenceAfter( int previousStepId, int nextStepId )
        //{
        //    Step previousStep = await DbServiceLocator.GetItemRecursiveAsync<Step>(previousStepId);
        //    Step nextStep = await DbServiceLocator.GetItemRecursiveAsync<Step>(nextStepId);

        //    await InsertStepIntoSequenceAfter( previousStep, nextStep );
        //}

        ///// <summary>
        ///// Links the two steps NextNode and PreviousNode properties and updates the database.
        ///// </summary>
        ///// <returns></returns>
        //private static async Task InsertStepIntoSequenceAfter( Step insertAfter, Step stepToInsert )
        //{
        //    // Get the previous and next step of each step that was passed in so that we can link them together
        //    if( insertAfter != null && stepToInsert != null )
        //    {

        //        Step previousNextStep = null;
        //        if( insertAfter.NextNodeId != null )
        //        {
        //            previousNextStep = await DbServiceLocator.GetItemRecursiveAsync<Step>( (int)insertAfter.NextNodeId );
        //        }

        //        Step nextStepPreviousStep = null;
        //        if( stepToInsert.PreviousNodeId != null )
        //        {
        //            nextStepPreviousStep = await DbServiceLocator.GetItemRecursiveAsync<Step>( (int)stepToInsert.PreviousNodeId );
        //        }

        //        Step nextStepNextStep = null;
        //        if( stepToInsert.NextNodeId != null )
        //        {
        //            nextStepNextStep = await DbServiceLocator.GetItemRecursiveAsync<Step>( (int)stepToInsert.NextNodeId );
        //        }

        //        insertAfter.NextNodeId = stepToInsert.Id;
        //        insertAfter.NextNode = stepToInsert;
        //        await DbServiceLocator.UpdateItemAsync( insertAfter );

        //        stepToInsert.PreviousNodeId = insertAfter.Id;
        //        stepToInsert.PreviousNode = insertAfter;
        //        await DbServiceLocator.UpdateItemAsync( stepToInsert );

        //        if( nextStepPreviousStep != null && nextStepNextStep != null )
        //        {
        //            nextStepPreviousStep.NextNode = nextStepNextStep;
        //            nextStepPreviousStep.NextNodeId = nextStepNextStep.Id;

        //            nextStepNextStep.PreviousNode = nextStepPreviousStep;
        //            nextStepNextStep.PreviousNodeId = nextStepPreviousStep.Id;

        //            await DbServiceLocator.UpdateItemAsync( nextStepPreviousStep );
        //            await DbServiceLocator.UpdateItemAsync( nextStepNextStep );
        //        }

        //    }

        //}

        #region Process Visualization
        // Possible stepId movements within list
        //
        // Case 1 - Moving from front of list to middle of list:
        // Starting Sequence =  N <= [1] <=> {2} <|=|> 3 <=> 4 <=> 5 => N
        // Desired Sequence  =  N <= {2} <=> [1] <=> 3 <=> 4 <=> 5 => N
        // Changes required:
        // newNextStep = 1, step = 2, nextStepNext = a.next, nextStepPrev = a.prev, oldNextStep = b.next oldPrevStep = b.prev
        //   Prop       | Current | Into | Operation                           | Result
        //  1.next      |    2    |  3   | 1.ChangeNextStep(oldNextStep)       | 1.next == 3
        //  1.prev      |    N    |  2   | 1.ChangePrevStep(step)              | 1.prev == 2
        //  1.next.prev |    2    |  1   | 1.next.ChangePrevStep(newNextStep)  | 3.prev == 1
        //  1.prev.next |    3    |  1   | 1.prev.ChangeNextStep(newNextStep)  | 2.next == 1

        //  2.next      |    1    |  1   | No Change Required                  | 2.next == 1
        //  2.prev      |    1    |  N   | 2.ChangePrevStep(nextStepPrev)      | 2.prev == N
        //  2.next.prev |    2    |  2   | No Change Required                  | 1.prev == 2
        //  2.prev.next |    N    |  N   | No Change Required                  | N.next == N

        // Case 2 - Moving from middle of list to middle of list:
        // Starting Sequence =  N <= 1 <=> {2} <|=|> 3 <=> [4] <=> 5 => N
        // Desired Sequence  =  N <= 1 <=> {2} <=> [4] <=> 3 <=> 5 => N
        // Changes required:
        // newNextStep = 4, step = 2, nextStepNext = a.next\5\, nextStepPrev = a.prev\3\, oldNextStep = b.next\3\ oldPrevStep = b.prev\1\
        //   Prop       | Current | Into | Operation                           | Result
        //  4.next      |    5    |  3   | 4.ChangeNextStep(oldNextStep)       | 4.next == 3
        //  4.prev      |    3    |  2   | 4.ChangePrevStep(step)              | 4.prev == 2
        //  4.next.prev |    2    |  4   | 4.next.ChangePrevStep(newNextStep)  | 3.prev == 4
        //  4.prev.next |    3    |  4   | 4.prev.ChangeNextStep(newNextStep)  | 2.next == 4

        //  2.next      |    4    |  4   | No Change Required                  | 2.next == 4
        //  2.prev      |    1    |  1   | No Change Required                  | 2.prev == 5
        //  2.next.prev |    2    |  2   | No Change Required                  | 4.prev == 2
        //  2.prev.next |    2    |  2   | No Change Required                  | 1.next == 2

        // Case 3 - Moving from end of list to middle of list:
        // Starting Sequence =  N <= 1 <=> {2} <|=|> 3 <=> 4 <=> [5] => N
        // Desired Sequence  =  N <= 1 <=> {2} <=> [5] <=> 3 <=> 4 => N
        // Variables:
        // step = 2, newNextStep = 5, stepNextStep = 3, stepPrevStep = 1, nnsOldNextStep = N, nnsOldPrevStep = 4
        // Changes required:
        //   Prop  | Current | Into | Operation           | Result
        //  5.next |    N    |  3   | 5.ChangeNextStep(3) | 5.next == 3
        //  5.prev |    4    |  2   | 5.ChangePrevStep(2) | 5.prev == 2
        //  N.prev |    N    |  N   | No Change Required  | N.prev == N
        //  4.next |    5    |  N   | 4.ChangeNextStep(N) | 4.next == N

        //  2.next |    3    |  5   | 2.ChangeNextStep(5) | 2.next == 5
        //  2.prev |    1    |  1   | No Change Required  | 2.prev == 1
        //  3.prev |    2    |  2   | No Change Required  | 5.prev == 2
        //  1.next |    2    |  2   | No Change Required  | 1.next == 2

        // | <=> 1 <=> 2 <=> 3 <|=|> 4 <=> 5 <=> 6 <=> [7] <=> 8 <=> |
        // 6 <=> 8
        // 3 <=> 7
        // 4 <=> 7
        // | <-> 1 <=> 2 <=> 3 <=> 7 <=> 4 <=> 5 <=> 6 <=> 8 <-> |
        #endregion

        //public static async Task<bool> AddToBackOfSequence( int sequenceMemberId, int nodeId )
        //{
        //    INode<Step> sequenceMember = await GetItemAsync( sequenceMemberId );
        //    INode<Step> backNode = await GetItemAsync( sequenceMember.BackNodeId );
        //    backNode.NextNodeId = nodeId;

        //    //await Update
        //}

        //public static async Task<bool> AddToSequenceAtIndex( int nodeId, int index )
        //{
        //    INode<Step> step = await GetItemRecursiveAsync( nodeId );

        //    if( index > await step.CountSequenceLength() )

        //    if( step.Index > index )
        //    {

        //    }
        //}

        //public static async Task<bool> AddToSequenceAtIndex( INode<Step> step, int sequenceIndex )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> AddToSequenceAfterNode( int stepId, int stepToAddAfterId )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> AddToSequenceAfterNode( INode<Step> stepId, INode<Step> stepToAddAfterId )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> AddToSequenceBeforeNode( int stepIdToAdd, int stepIdToAddBefore )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> AddToSequenceBeforeNode( INode<Step> stepToAdd, INode<Step> stepToAddBefore )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> RemoveFromSequence( int stepId )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<bool> RemoveFromSequence( INode<Step> step )
        //{
        //    throw new NotImplementedException();
        //}

        //public static async Task<INode<Step>> GetNodeAtIndex( this INode<Step> step, int index )
        //{
        //    if( step.Index == index )
        //    {
        //        return step;
        //    }

        //    if( index < step.Index && step.PreviousNodeId != null )
        //    {
        //        INode<Step> prev = await GetItemAsync( (int)step.PreviousNodeId );
        //        return await prev.GetNodeAtIndex( index );
        //    }

        //    if( index > step.Index && step.NextNodeId != null )
        //    {
        //        INode<Step> next = await GetItemAsync( (int)step.NextNodeId);
        //        return await GetNodeAtIndex( next, index );
        //    }

        //    throw new ArgumentOutOfRangeException();
        //}


        #endregion

        #region Item Retrieval

        public static async Task<List<Step>> GetItemRange( IEnumerable<int> stepIds )
        {
            return await DbServiceLocator.GetItemRangeAsync<Step>( stepIds ) as List<Step>;
        }

        public static async Task<Step> GetItemAsync( int stepId )
        {
            return await DbServiceLocator.GetItemAsync<Step>( stepId );
        }

        public static async Task<Step> GetItemRecursiveAsync( int stepId )
        {
            Step step = await DbServiceLocator.GetItemRecursiveAsync<Step>( stepId );

            step.MaintenanceItem = step.MaintenanceItemId == 0 ? null : await MaintenanceItemManager.GetItemAsync( step.MaintenanceItemId );

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
        /// Not Implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task DeleteItem( int id )
        {
            Step step = await GetItemRecursiveAsync(id);

            if( step.MaintenanceItem.Steps.Contains( step ) )
            {

            }
        }

        #endregion
    }
}
