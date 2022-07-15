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

        //TODO: Create Custom Editor Renderer for Android and IOS
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

        private bool imageIsVisible = false;
        public bool HasImage
        {
            get => imageIsVisible;
            set => SetProperty( ref imageIsVisible, value );
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
            if( NoteId > 0 )
                _ = Shell.Current.GoToAsync( $"{nameof( EditNoteView )}?{QueryParameters.NoteId}={NoteId}" );
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

            using( Stream stream = await photo.OpenReadAsync() )
            using( MemoryStream mStream = new MemoryStream() )
            {
                stream.CopyTo( mStream );

                imageData = mStream.ToArray();
                Image = ImageSource.FromStream( () => new MemoryStream( imageData ) );

            };

            if( NoteId > 0 )
            {
                await NoteManager.AddImageToNote( NoteId, imageData );
                await Refresh();
            }

            HasImage = true;
        }

        private AsyncCommand refreshCommand;
        public AsyncCommand RefreshCommand
        {
            get => refreshCommand ??= new AsyncCommand( Refresh );
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
            HasImage = Image != default;
        }

        public void Init( Note note )
        {
            this.note = note;

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
            HasImage = Image != default;
        }

        public void InitWithoutImage( Note note )
        {
            this.note = note;

            NoteId = note.Id;
            Text = note.Text;
            StepId = note.StepId;
            LastUpdated = note.LastUpdated.ToLocalTime();
            CreatedOn = note.CreatedOn.ToLocalTime();
            HasImage = Image != default;
        }

        public void InitImage( Note note )
        {
            Image = note.ImageData != default( byte[] )
            ? ImageSource.FromStream( () => new MemoryStream( note.ImageData ) )
            : note.ImagePath != string.Empty
            ? ImageSource.FromFile( note.ImagePath )
            : default; // this last option will be a default "no photo" image of some sort.
        }

        public async Task<int> Save()
        {
            int id = await NoteManager.NewNote(Text);
            NoteId = id;
            if( Image != null )
            {
                await NoteManager.AddImageToNote( id, Image );
            }

            if( StepId > 0 )
            {
                await NoteManager.AddStepToNote( id, StepId );
            }

            return id;
        }

        private async Task Refresh()
        {
            if( NoteId > 0 )
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
                HasImage = Image != default;
            }
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
