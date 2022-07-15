using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;

using Xamarin.Forms;

namespace Maintain_it.Helpers
{
    internal static class NoteManager
    {
        /// <summary>
        /// Creates a new note without an associated <see cref="Step"/> and adds it to the database.
        /// </summary>
        public static async Task<int> NewNote( string text, string ImagePath = "", string name = "", byte[] imageData = default )
        {
            return await MakeNoteAndReturnId( name, text, ImagePath, imageData );
        }

        /// <summary>
        /// Creates a new Note, adds it to the Db and returns the Id.
        /// </summary>
        private static async Task<int> MakeNoteAndReturnId( string name, string text, string ImagePath, byte[] imageData )
        {
            Note note = new Note()
            {
                Name = name,
                Text = text,
                ImagePath = ImagePath,
                ImageData = imageData,
                CreatedOn = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
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
        public static async Task<int> NewNoteWithStep( string name, string text, int stepId, string imagePath = "", byte[] imageData = default )
        {
            Step step = await DbServiceLocator.GetItemAsync<Step>( stepId );

            Note note = await GetItemAsync( await MakeNoteAndReturnId( name, text, imagePath, imageData ) );

            await UpdateNoteStep( note, step );

            return note.Id;
        }

        public static async Task AddStepToNote( int noteId, int stepId )
        {
            Note note = await GetItemAsync( noteId );
            Step step = await StepManager.GetItemRecursiveAsync( stepId );

            if( step != null )
            {
                if( !step.Notes.GetIds( out IList<int> stepNoteIds ).Contains( noteId ) )
                {
                    stepNoteIds.Add( noteId );
                    await StepManager.UpdateItemAsync( step.Id, noteIds: stepNoteIds );
                }

                note.Step = step;
            }

            await UpdateItemAsync( note );
        }

        public static async Task AddImageToNote( int noteId, string imagePath )
        {
            Note note = await GetItemRecursiveAsync( noteId );

            note.ImagePath = imagePath;
            note.ImageData = default;
            note.LastUpdated = DateTime.UtcNow;

            await UpdateItemAsync( note );
        }

        public static async Task AddImageToNote( int noteId, ImageSource source )
        {
            Note note = await GetItemRecursiveAsync( noteId );
            byte[] bytes;

            if( source != null )
            {
                StreamImageSource streamIS = (StreamImageSource)source;
                CancellationToken cToken = CancellationToken.None;

                using( Stream imageStream = await streamIS.Stream( cToken ) )
                using( MemoryStream ms = new MemoryStream() )
                {
                    imageStream.CopyTo( ms );
                    bytes = ms.ToArray();
                }

                note.ImageData = bytes;
                note.ImagePath = string.Empty;
                note.LastUpdated = DateTime.UtcNow;

                await UpdateItemAsync( note );
            }
        }

        public static async Task AddImageToNote( int noteId, byte[] sourceData )
        {
            Note note = await GetItemRecursiveAsync( noteId );

            note.ImageData = sourceData;
            note.ImagePath = string.Empty;
            note.LastUpdated = DateTime.UtcNow;

            await UpdateItemAsync( note );
        }

        public static async Task UpdateItemAsync( Note note )
        {
            await DbServiceLocator.UpdateItemAsync( note );
        }
#nullable enable
        public static async Task UpdateItemAsync( int noteId, string? text = null, byte[]? imageData = null, string? imagePath = null )
        {
            Note note = await GetItemAsync(noteId);

            note.Text = text ?? note.Text;
            note.ImageData = imageData ?? note.ImageData;
            note.ImagePath = imagePath ?? note.ImagePath;
            note.LastUpdated = DateTime.UtcNow;

            await UpdateItemAsync( note );
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

        /// <summary>
        /// Gets the Note with the passed in Id, but no inverse relationship data.
        /// </summary>
        public static async Task<Note> GetItemAsync( int id )
        {
            return await DbServiceLocator.GetItemAsync<Note>( id );
        }

        internal static async Task<List<Note>> GetItemRangeAsync( IEnumerable<int> noteIds )
        {
            return await DbServiceLocator.GetItemRangeAsync<Note>( noteIds ) as List<Note>;
        }


        public static async Task<NoteViewModel> GetItemAsViewModelAsync( int id )
        {
            Note note = await GetItemAsync(id);

            NoteViewModel vm = new NoteViewModel();
            vm.InitWithoutImage( note );

            // Process the image stuff on another thread so that the UI doesn't freeze. Maybe. Hopefully. I think...
            await Task.Run( () =>
            {
                vm.InitImage( note );
            } );

            return vm;
        }

        public static async Task<IEnumerable<NoteViewModel>> GetItemRangeAsViewModelsAsync( IEnumerable<int> ids )
        {
            List<NoteViewModel> vms = new List<NoteViewModel>();

            foreach( int id in ids )
            {
                NoteViewModel vm = await GetItemAsViewModelAsync( id );
                vms.Add( vm );
            }

            return vms;
        }

        /// <summary>
        /// Gets the Note with the passed in Id recursively and ensures that all properties are fully populated. 
        /// 
        ///<para><i>Inverse relationships are not populated recursively</i></para>
        /// </summary>
        public static async Task<Note> GetItemRecursiveAsync( int id )
        {
            Note note = await DbServiceLocator.GetItemRecursiveAsync<Note>( id );

            note.Step = note.StepId < 1 ? null : await StepManager.GetItemAsync( note.StepId ) ?? null;

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
