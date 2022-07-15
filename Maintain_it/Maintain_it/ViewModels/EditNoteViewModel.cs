using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;

using MvvmHelpers.Commands;

using Xamarin.Essentials;
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

        private AsyncCommand replacePhotoCommand;
        public ICommand ReplacePhotoCommand
        {
            get => replacePhotoCommand ??= new AsyncCommand( ReplacePhoto );
        }

        private async Task ReplacePhoto()
        {
            if( !MediaPicker.IsCaptureSupported )
            {
                await Shell.Current.DisplayAlert( Alerts.Error, Alerts.CameraErrorMessage, Alerts.Confirmation );

                return;
            }

            if( Image != null )
            {
                bool intent = await Shell.Current.DisplayAlert( Alerts.ReplaceImageTitle, Alerts.ReplaceImageMessage, Alerts.Yes, Alerts.Cancel );
                switch( intent )
                {
                    case true:
                        break;
                    case false:
                        return;
                }
            }

            FileResult photo = await MediaPicker.CapturePhotoAsync();

            if( photo == null )
            {
                return;
            }

            byte[] imageData;

            using( Stream stream = await photo.OpenReadAsync() )
            using( MemoryStream mStream = new MemoryStream() )
            {
                stream.CopyTo( mStream );

                imageData = mStream.ToArray();
                Image = ImageSource.FromStream( () => new MemoryStream( imageData ) );
            };
        }

        private AsyncCommand saveCommand;
        public ICommand SaveCommand
        {
            get => saveCommand ??= new AsyncCommand( Save );
        }

        private async Task Save()
        {
            byte[] imageData = await ConvertImageToBytes();
            await NoteManager.UpdateItemAsync( NoteId, Text, imageData );
            string encodedId = HttpUtility.UrlEncode($"{NoteId}");
            await Shell.Current.GoToAsync( $"..?{QueryParameters.RefreshNote}={encodedId}" );
        }

        private AsyncCommand backCommand;
        public ICommand BackCommand
        {
            get => backCommand ??= new AsyncCommand( Back );
        }

        private async Task Back()
        {
            string choice = await Shell.Current.DisplayActionSheet( Alerts.DiscardChangesTitle, Alerts.Cancel, null, Alerts.Discard, Alerts.Save );

            switch( choice )
            {
                case Alerts.Save:
                    await Save();
                    break;
                case Alerts.Discard:
                    await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
                    break;
                default:
                    break;
            }
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

        private async Task<byte[]> ConvertImageToBytes()
        {
            StreamImageSource streamIS = (StreamImageSource)Image;
            CancellationToken cToken = CancellationToken.None;

            using Stream imageStream = await streamIS.Stream( cToken );
            using MemoryStream ms = new MemoryStream();
            imageStream.CopyTo( ms );
            byte[] bytes = ms.ToArray();
            return bytes;
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.NoteId:
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
