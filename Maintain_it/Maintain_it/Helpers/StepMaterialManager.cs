using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;

namespace Maintain_it.Helpers
{
    internal static class StepMaterialManager
    {
        /// <summary>
        /// Creates a new <see cref="StepMaterial"/> and adds it to the database. DOES NOT UPDATE THE MATERIAL.
        /// </summary>
        public static async Task<int> NewStepMaterial( string name, int quantity, int materialId )
        {
            Material material = await MaterialManager.GetItemAsync( materialId );

            return await MakeStepMaterialWithoutStep( name, quantity, materialId, material );

        }

        /// <summary>
        /// Creates a new <see cref="StepMaterial"/> and adds it to the database. DOES NOT MODIFY THE PASSED IN STEP OR MATERIAL, MAKE SURE TO UPDATE THEM SEPERATELY
        /// </summary>
        public static async Task<int> NewStepMaterial( string name, int quantity, int materialId, int stepId )
        {
            Material material = await MaterialManager.GetItemAsync( materialId );
            Step step = await StepManager.GetItemAsync( stepId );

            return await MakeStepMaterialWithStep( name, quantity, materialId, stepId, material, step );
        }

        private static async Task<int> MakeStepMaterialWithoutStep( string name, int quantity, int materialId, Material material )
        {
            StepMaterial stepMaterial = new StepMaterial()
            {
                Name = name,
                Quantity = quantity,
                CreatedOn = DateTime.UtcNow,
                MaterialId = materialId,
                Material = material
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( stepMaterial );
        }

        private static async Task<int> MakeStepMaterialWithStep( string name, int quantity, int materialId, int stepId, Material material, Step step )
        {
            StepMaterial stepMaterial = new StepMaterial()
            {
                Name = name,
                Quantity = quantity,
                CreatedOn = DateTime.UtcNow,
                MaterialId = materialId,
                Material = material,
                StepId = stepId,
                Step = step
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( stepMaterial );
        }

        /// <summary>
        /// Updates the properties whose associated parameters are not null and updates the Db.
        /// </summary>
        public static async Task UpdateStepMaterial( int stepMaterialId, string? name, int? quantity )
        {
            StepMaterial stepMaterial = await GetItemRecursiveAsync( stepMaterialId );

            stepMaterial.Name = name ?? stepMaterial.Name;
            stepMaterial.Quantity = quantity ?? stepMaterial.Quantity;

            await DbServiceLocator.UpdateItemAsync( stepMaterial );
        }


        /// <summary>
        /// Updates the Step and StepId properties of the passed in StepMaterial. DOES NOT UPDATE THE PASSED IN STEP.
        /// </summary>
        public static async Task UpdateStepMaterialStep( int stepMaterialId, int stepId )
        {
            StepMaterial stepMaterial = await GetItemRecursiveAsync( stepMaterialId );

            Step step = await StepManager.GetItemRecursiveAsync( stepId );

            if( stepMaterial.StepId != stepId )
            {
                stepMaterial.Step = step;
                stepMaterial.StepId = stepId;
            }

            await DbServiceLocator.UpdateItemAsync( stepMaterial );
        }

        internal static async Task<List<StepMaterial>> GetItemRangeAsync( IEnumerable<int> stepMaterialIds )
        {
            return await DbServiceLocator.GetItemRangeAsync<StepMaterial>( stepMaterialIds ) as List<StepMaterial>;
        }

        /// <summary>
        /// Updates the Material and MaterialId properties of the passed in StepMaterial. DOES NOT UPDATE THE PASSED IN MATERIAL.
        /// </summary>
        public static async Task UpdateStepMaterialMaterial( int stepMaterialId, int materialId )
        {
            StepMaterial stepMaterial = await GetItemRecursiveAsync(stepMaterialId);
            Material material = await MaterialManager.GetItemRecursiveAsync( materialId );

            if( stepMaterial.MaterialId != materialId )
            {
                stepMaterial.MaterialId = materialId;
                stepMaterial.Material = material;
            }

            await DbServiceLocator.UpdateItemAsync( stepMaterial );
        }

        public static async Task<StepMaterial> GetItemRecursiveAsync( int stepMaterialId )
        {
            StepMaterial stepMaterial = await DbServiceLocator.GetItemRecursiveAsync<StepMaterial>(stepMaterialId);

            stepMaterial.Step ??= await StepManager.GetItemAsync( stepMaterial.StepId );
            stepMaterial.Material ??= await MaterialManager.GetItemAsync( stepMaterialId );

            return stepMaterial;
        }

        public static async Task<List<StepMaterial>> GetItemRangeRecursiveAsync( IEnumerable<int> ids )
        {
            List<StepMaterial> stepMaterials = new List<StepMaterial>();

            foreach( int id in ids )
            {
                if( id == 0 ) continue;

                StepMaterial stepMaterial = await GetItemRecursiveAsync( id );
                stepMaterials.Add( stepMaterial );
            }

            return stepMaterials;
        }

        /// <summary>
        /// Deletes the StepMaterial with the passed in id
        /// </summary>
        public static async Task DeleteItem( int id )
        {
            await DbServiceLocator.DeleteItemAsync<StepMaterial>( id );
        }

        /// <summary>
        /// Deletes all the StepMaterials with the passed in ids
        /// </summary>
        public static async Task DeleteItemRange( IEnumerable<int> ids )
        {
            foreach( int id in ids )
            {
                await DeleteItem( id );
            }
        }

        /// <summary>
        /// Retrieves all the StepMaterials, converts them into ViewModels, and returns the List.
        /// </summary>
        /// <param name="ids">IEnumerable of StepMaterial Ids</param>
        /// <returns>List of type <see cref="StepMaterialViewModel"/></returns>
        public static async Task<List<StepMaterialViewModel>> GetItemRangeAsViewModelAsync( IEnumerable<int> ids )
        {
            List<StepMaterial> stepMaterials = await GetItemRangeRecursiveAsync(ids);
            List<StepMaterialViewModel> vms = new List<StepMaterialViewModel>();
            foreach( StepMaterial stepMaterial in stepMaterials )
            {
                StepMaterialViewModel vm = new StepMaterialViewModel()
                {
                    StepMaterial = stepMaterial,
                    Quantity = stepMaterial.Quantity
                };

                vms.Add( vm );
            }

            return vms;
        }

        ///// <summary>
        ///// Not Implemented
        ///// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        //public static async Task<StepMaterial> GetItemAsync( int id )
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Not Implemented
        ///// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        //public static async Task<List<StepMaterial>> GetAllItemsAsync()
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Not Implemented
        ///// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        //public static async Task<List<StepMaterial>> GetAllItemsRecursiveAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
