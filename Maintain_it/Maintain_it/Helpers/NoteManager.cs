using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    internal static class NoteManager
    {
        /// <summary>
        /// Creates a new note without an associated <see cref="Step"/> and adds it to the database.
        /// </summary>
        public static async Task<int> NewNote(  string text, string ImagePath = "", string name = "" )
        {
            return await MakeNoteAndReturnId( name, text, ImagePath );
        }

        /// <summary>
        /// Creates a new Note, adds it to the Db and returns the Id.
        /// </summary>
        private static async Task<int> MakeNoteAndReturnId( string name, string text, string ImagePath )
        {
            Note note = new Note()
            {
                Name = name,
                Text = text,
                ImagePath = ImagePath,
                CreatedOn = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            return await DbServiceLocator.AddItemAndReturnIdAsync( note );
        }

        /// <summary>
        /// Updates the passed in Note's Step property with the passed in Step. Updates the Db by default.
        /// </summary>
        private static async Task UpdateNoteStep( Note note, Step step, bool updateDb = true )
        {
            if( note.StepId != step.Id )
            {
                note.StepId = step.Id;
                note.Step = step;

                if( updateDb )
                    await DbServiceLocator.UpdateItemAsync( note );
            }
        }

        /// <summary>
        /// Creates a new note with an associated <see cref="Step"/> and adds it to the database.
        /// </summary>
        public static async Task<int> NewNoteWithStep( string name, string text, int stepId, string imagePath = "" )
        {
            Step step = await DbServiceLocator.GetItemAsync<Step>( stepId );

            Note note = await GetItemAsync( await MakeNoteAndReturnId( name, text, imagePath ) );

            await UpdateNoteStep( note, step );

            return note.Id;
        }

        public static async Task AddImageToNote( int noteId, string imagePath )
        {
            Note note = await GetItemRecursiveAsync( noteId );

            note.ImagePath = imagePath;

            await UpdateItemAsync( note );
        }

        public static async Task UpdateItemAsync( Note note )
        {
            await DbServiceLocator.UpdateItemAsync( note );
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
        /// Gets the Note with the passed in Id, but no inverse relationship data.
        /// </summary>
        public static async Task<Note> GetItemAsync( int id )
        {
            return await DbServiceLocator.GetItemAsync<Note>( id );
        }

        internal static Task<List<Note>> GetItemRangeAsync( IEnumerable<int> noteIds )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Note with the passed in Id recursively and ensures that all properties are fully populated. 
        /// 
        ///<para><i>Inverse relationships are not populated recursively</i></para>
        /// </summary>
        public static async Task<Note> GetItemRecursiveAsync( int id )
        {
            Note note = await DbServiceLocator.GetItemRecursiveAsync<Note>( id );

            note.Step ??= await StepManager.GetItemAsync( note.StepId );

            return note;
        }


        /// <summary>
        /// Gets all Notes in the Db. Does not load any inverse relationships.
        /// </summary>
        public static async Task<List<Note>> GetAllItemsAsync()
        {
            return await DbServiceLocator.GetAllItemsAsync<Note>() as List<Note>;
        }

        /// <summary>
        /// Gets all Notes in the Db and ensures that all inverse relationships are populated.
        /// <para><i>Inverse relationships are not populated recursively</i></para>
        /// </summary>
        public static async Task<List<Note>> GetAllItemsRecursiveAsync()
        {
            List<Note> notes = await DbServiceLocator.GetAllItemsRecursiveAsync<Note>() as List<Note>;

            foreach( Note note in notes )
            {
                note.Step ??= await StepManager.GetItemAsync( note.StepId );
            }

            return notes;
        }
    }
}
