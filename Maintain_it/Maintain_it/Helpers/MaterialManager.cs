using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    internal static class MaterialManager
    {
        /// <summary>
        /// Creates a new <see cref="Material"/> and adds it to the Db before returning the Id.
        /// </summary>
        public static async Task<int> GetNewMaterialAndReturnId( string name, int quantityOwned, double? size = null, string? units = null, string? description = null, string? partNumber = null )
        {
            return await MakeNewMaterial( name, quantityOwned, size, units, description, partNumber );
        }

        /// <summary>
        /// Creates a new <see cref="Material"/> and adds it to the Db before returning the Material.
        /// </summary>
        public static async Task<Material> GetNewMaterial( string name, int quantityOwned, double? size = null, string? units = null, string? description = null, string? partNumber = null )
        {
            int id = await MakeNewMaterial( name, quantityOwned, size, units, description, partNumber );

            return await GetItemRecursiveAsync( id );
        }

        private static async Task<int> MakeNewMaterial( string name, int quantityOwned, double? size, string units, string description, string partNumber )
        {
            Material material = new Material
            {
                Name = name,
                QuantityOwned = quantityOwned,
                Size = size,
                Units = units,
                Description = description,
                PartNumber = partNumber,
                CreatedOn = DateTime.UtcNow,
                Tags = new List<Tag>(),
                RetailerMaterials = new List<RetailerMaterial>(),
                ShoppingListMaterials = new List<ShoppingListMaterial>(),
                StepMaterials = new List<StepMaterial>()
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( material );
        }


        /// <summary>
        /// Adds the passed in tag to the passed in material's Tags list. DOES NOT UPDATE THE TAG, MAKE SURE TO DO THAT.
        /// </summary>
        public static async Task AddTagToMaterial( int materialId, int tagId )
        {
            Material material = await GetItemRecursiveAsync( materialId );
            Tag tag = await TagManager.GetItemRecursiveAsync( tagId );

            await AddTagToMaterial( material, tag );
        }

        /// <summary>
        /// Adds the passed in tag to the passed in material's Tags list. DOES NOT UPDATE THE TAG, MAKE SURE TO DO THAT.
        /// </summary>
        private static async Task AddTagToMaterial( Material material, Tag tag, bool updateDb = true )
        {
            if( !material.Tags.Contains( tag ) )
            {
                material.Tags.Add( tag );

                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( material );
            }
        }

        /// <summary>
        /// Adds each of the passed in tags to the passed in material's Tags list. DOES NOT UPDATE THE TAGS, MAKE SURE TO DO THAT.
        /// </summary>
        public static async Task AddTagsToMaterial( int materialId, IEnumerable<int> tagIds )
        {
            Material material = await GetItemRecursiveAsync( materialId );
            List<Tag> tags = await TagManager.GetItemRangeRecursiveAsync( tagIds );

            await AddTagsToMaterial( material, tags );
        }

        /// <summary>
        /// Adds each of the passed in tags to the passed in material's Tags list. DOES NOT UPDATE THE TAGS, MAKE SURE TO DO THAT.
        /// </summary>
        private static async Task AddTagsToMaterial( Material material, IEnumerable<Tag> tags )
        {
            foreach( Tag tag in tags )
            {
                await AddTagToMaterial( material, tag, false );
            }

            await DbServiceLocator.AddOrUpdateItemAsync( material );
        }

        /// <summary>
        /// Updates any properties on the material whose corresponding method parameters are not null.
        /// </summary>
        public static async Task UpdateMaterial( int materialId, string? name = null, int? quantityOwned = null, double? size = null, string? units = null, string? description = null, string? partNumber = null )
        {
            Material material = await GetItemRecursiveAsync(materialId);

            material.Name = name ?? material.Name;
            material.QuantityOwned = quantityOwned ?? material.QuantityOwned;
            material.Size = size ?? material.Size;
            material.Units = units ?? material.Units;
            material.Description = description ?? material.Description;
            material.PartNumber = partNumber ?? material.PartNumber;

            await DbServiceLocator.UpdateItemAsync( material );
        }

        public static async Task<Material> GetItemAsync( int materialId )
        {
            return await DbServiceLocator.GetItemAsync<Material>( materialId );
        }

        public static async Task<Material> GetItemRecursiveAsync( int materialId )
        {
            return await DbServiceLocator.GetItemRecursiveAsync<Material>( materialId );
        }

        public static async Task<List<Material>> GetItemRangeRecursiveAsync( IEnumerable<int> materialIds )
        {
            return await DbServiceLocator.GetItemRangeRecursiveAsync<Material>( materialIds ) as List<Material>;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task DeleteItem( int id )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all the Materials in the Db
        /// </summary>
        public static async Task<List<Material>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<Material>() as List<Material>;
        }

        /// <summary>
        /// Gets all the Materials in the Db recursively
        /// 
        /// <para><i>Inverse relationships are not populated recursively</i></para>
        /// </summary>
        public static async Task<List<Material>> GetAllItemsRecursiveAsync()
        {
            return await DbServiceLocator.GetAllItemsRecursiveAsync<Material>() as List<Material>;
        }
    }
}
