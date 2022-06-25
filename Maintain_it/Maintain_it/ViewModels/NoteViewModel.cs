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
using Xamarin.Essentials;
using Maintain_it.Views;

namespace Maintain_it.ViewModels
{
    public class NoteViewModel : BaseViewModel
    {
        public NoteViewModel() { }

        public NoteViewModel( int id )
        {
            NoteId = id;
        }

        #region PROPERTIES
        private Note note;

        private int noteId;
        public int NoteId
        {
            get => noteId;
            set => SetProperty( ref noteId, value );
        }

        private string text;
        public string Text
        {
            get => text;
            set => SetProperty( ref text, value );
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
            get => createdOn.ToLocalTime();
            set => SetProperty( ref createdOn, value );
        }

        public byte[] imageData { get; private set; }

        private ImageSource image;

        public ImageSource Image
        {
            get => image;
            set => SetProperty( ref image, value );
        }

        private int stepId;
        public int StepId
        {
            get => stepId;
            set => SetProperty( ref stepId, value );
        }

        #endregion

        #region COMMANDS
        private AsyncCommand editNoteCommand;
        public ICommand EditNoteCommand
        {
            get => editNoteCommand ??= new AsyncCommand( EditNote );
        }

        private async Task EditNote()
        {
            _ = Shell.Current.GoToAsync( $"{nameof( EditNoteView )}?{RoutingPath.NoteId}={NoteId}" );
        }

        private AsyncCommand takePhotoCommand;
        public ICommand TakePhotoCommand
        {
            get => takePhotoCommand ??= new AsyncCommand( TakePhoto );
        }

        private async Task TakePhoto()
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

            Stream stream = await photo.OpenReadAsync();
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo( memoryStream );
            
            imageData = memoryStream.ToArray();

            Image = ImageSource.FromStream( () => new MemoryStream( imageData ) );

            await NoteManager.AddImageToNote( NoteId, imageData );
            await Refresh();
        }

        #endregion

        #region METHODS

        public async Task Init( int id )
        {
            note = await NoteManager.GetItemAsync( id );

            NoteId = note.Id;
            Text = note.Text;
            Image = note.ImageData != default( byte[] )
                ? ImageSource.FromStream( () => new MemoryStream( note.ImageData ) )
                : note.ImagePath != string.Empty
                ? ImageSource.FromFile( note.ImagePath )
                : default; // this last option will be a default "no photo" image of some sort.
            StepId = note.StepId;
            LastUpdated = note.LastUpdated.ToLocalTime();
            CreatedOn = note.CreatedOn.ToLocalTime();
        }

        private async Task Refresh()
        {
            note = await NoteManager.GetItemAsync( NoteId );

            Text = note.Text;
            Image = note.ImageData != default( byte[] )
                ? ImageSource.FromStream( () => new MemoryStream( note.ImageData ) )
                : note.ImagePath != string.Empty
                ? ImageSource.FromFile( note.ImagePath )
                : default; // this last option will be a default "no photo" image of some sort.
            StepId = note.StepId;
            LastUpdated = note.LastUpdated.ToLocalTime();
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                default:
                    break;
            }
        }
        #endregion

    }
}
