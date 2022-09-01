using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    internal static class TagManager
    {
        /// <summary>
        /// Creates a new tag, adds it to the database and returns the Id
        /// </summary>
        public static async Task<int> GetNewTagAndReturnId( string name, TagType tagType = TagType.General )
        {
            // AddShallow any validation here if needed.

            return await NewTag( name, tagType );
        }

        /// <summary>
        /// Creates a new tag, adds it to the database, and returns it
        /// </summary>
        public static async Task<Tag> GetNewTag( string name, TagType tagType = TagType.General )
        {
            return await GetItemRecursiveAsync( await NewTag( name, tagType ) );
        }

        /// <summary>
        /// Creates a new tag, adds it to the database and returns the Id
        /// </summary>
        private static async Task<int> NewTag( string name, TagType tagType )
        {
            Tag tag = new Tag()
            {
                Name = name,
                TagType = tagType,
                CreatedOn = DateTime.UtcNow
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( tag );
        }

        /// <summary>
        /// Adds the passed in Material to the passed in Tag and updates the db. DOES NOT UPDATE THE MATERIAL, ONLY THE TAG.
        /// </summary>
        public static async Task AddMaterialToTag( int tagId, int materialId )
        {
            Tag tag = await GetItemRecursiveAsync( tagId );
            Material material = await MaterialManager.GetItemAsync(materialId);

            await AddMaterialToTag( tag, material );
        }

        /// <summary>
        /// Adds the passed in Material to the passed in Tag and updates the db unless specified otherwise. DOES NOT UPDATE THE MATERIAL, ONLY THE TAG.
        /// </summary>
        private static async Task AddMaterialToTag( Tag tag, Material material, bool updateDb = true )
        {
            if( !tag.Materials.Contains( material ) )
            {
                tag.Materials.Add( material );

                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( tag );
            };
        }

        /// <summary>
        /// Adds the passed in Material to the passed in Tags and updates the db unless specified otherwise. DOES NOT UPDATE THE MATERIAL, ONLY THE TAGS.
        /// </summary>
        public static async Task AddMaterialToTags( IEnumerable<int> tagIds, int materialId )
        {
            List<Tag> tags = await GetItemRangeRecursiveAsync(tagIds);
            Material material = await MaterialManager.GetItemAsync( materialId);

            await AddMaterialToTags( tags, material );
        }

        /// <summary>
        /// Adds the passed in Material to the passed in Tags and updates the db unless specified otherwise. DOES NOT UPDATE THE MATERIAL, ONLY THE TAGS.
        /// </summary>
        private static async Task AddMaterialToTags( IEnumerable<Tag> tags, Material material )
        {
            foreach( Tag tag in tags )
            {
                await AddMaterialToTag( tag, material );
            }
        }

        /// <summary>
        /// Adds the passed in Materials to the passed in Tags and updates the db unless specified otherwise. DOES NOT UPDATE THE MATERIALS, ONLY THE TAGS.
        /// 
        ///<para> This is a nested <see langword="foreach"/> loop with a lot of Db read/write. Expect it to be quite slow. If you have a lot of data in one or both lists maybe try and find a different way to get it done.</para>
        /// </summary>
        public static async Task AddMaterialsToTags( IEnumerable<int> tagIds, IEnumerable<int> materialIds )
        {
            List<Tag> tags = await GetItemRangeRecursiveAsync(tagIds);
            List<Material> materials = await MaterialManager.GetItemRangeRecursiveAsync(materialIds);

            await AddMaterialsToTags( tags, materials );
        }

        /// <summary>
        /// Adds the passed in Materials to the passed in Tags and updates the db unless specified otherwise. DOES NOT UPDATE THE MATERIALS, ONLY THE TAGS.
        /// 
        ///<para>This is a nested <see langword="foreach"/> loop with a lot of Db read/write. Expect it to be quite slow. If you have a lot of data in one or both lists maybe try and find a different way to get it done.</para>
        /// </summary>
        private static async Task AddMaterialsToTags( IEnumerable<Tag> tags, IEnumerable<Material> materials )
        {
            foreach( Material material in materials )
            {
                await AddMaterialToTags( tags, material );
            }
        }

        /// <summary>
        /// Gets the Tag with the passed in Id from the database recursively.
        /// </summary>
        public static async Task<Tag> GetItemRecursiveAsync( int tagId )
        {
            return await DbServiceLocator.GetItemRecursiveAsync<Tag>( tagId );
        }

        /// <summary>
        /// Gets the range of Tags with the passed in from the database recursively.
        /// </summary>
        public static async Task<List<Tag>> GetItemRangeRecursiveAsync( IEnumerable<int> tagIds )
        {
            return await DbServiceLocator.GetItemRangeRecursiveAsync<Tag>( tagIds ) as List<Tag>;
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
        /// Gets the Tag with the passed in Id from the Db.
        /// </summary>
        public static async Task<Tag> GetItemAsync( int id )
        {
            return await DbServiceLocator.GetItemAsync<Tag>( id );
        }

        /// <summary>
        /// Gets all the Tags in the Db
        /// </summary>
        public static async Task<List<Tag>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<Tag>() as List<Tag>;
        }

        /// <summary>
        /// Gets all the Tags in the Db recursively
        /// 
        /// <para><i>Inverse relationships are not populated recursively</i></para>
        /// </summary>
        public static async Task<List<Tag>> GetAllItemsRecursiveAsync()
        {
            return await DbServiceLocator.GetAllItemsRecursiveAsync<Tag>() as List<Tag>;
        }
    }
}
