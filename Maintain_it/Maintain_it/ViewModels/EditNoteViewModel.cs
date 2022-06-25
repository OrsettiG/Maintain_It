using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class EditNoteViewModel : BaseViewModel
    {
        #region PROPERTIES
        private Note note;

        private int noteId;
        public int NoteId
        {
            get => noteId;
            set => SetProperty( ref noteId, value );
        }

        private bool overlayIsVisible = true;
        public bool OverlayIsVisible
        {
            get => overlayIsVisible;
            set => SetProperty( ref overlayIsVisible, value );
        }

        private string text;
        public string Text
        {
            get => text;
            set => SetProperty( ref text, value );
        }

        private ImageSource image;
        public ImageSource Image
        {
            get => image;
            set => SetProperty( ref image, value );
        }

        private DateTime lastUpdated;
        public DateTime LastUpdated
        {
            get => lastUpdated;
            set => SetProperty( ref lastUpdated, value );
        }

        private DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetProperty( ref createdOn, value );
        }

        private Step step;
        public Step Step
        {
            get => step;
            private set => SetProperty( ref step, value );
        }

        #endregion

        #region COMMANDS
        private AsyncCommand toggleOverlayVisibilityCommand;
        public ICommand ToggleOverlayVisibilityCommand
        {
            get => toggleOverlayVisibilityCommand ??= new AsyncCommand( ToggleOverlayVisibility );
        }

        private async Task ToggleOverlayVisibility()
        {
            OverlayIsVisible = !OverlayIsVisible;
        }
        #endregion

        #region METHODS

        private async Task Refresh()
        {
            note = await NoteManager.GetItemRecursiveAsync( NoteId );

            Text = note.Text;
            Image = note.ImageData != null
                ? ImageSource.FromStream( () => new MemoryStream( note.ImageData ) )
                : note.ImagePath != string.Empty
                ? ImageSource.FromFile( note.ImagePath )
                : default;
            LastUpdated = note.LastUpdated;
            CreatedOn = note.CreatedOn;
            Step = note.Step;
        }

        private async Task Refresh( int id )
        {
            NoteId = id;
            await Refresh();
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case RoutingPath.NoteId:
                    if( int.TryParse( kvp.Value, out int id ) )
                    {
                        await Refresh( id );
                    }
                    break;
            }
        }
        #endregion
    }
}
