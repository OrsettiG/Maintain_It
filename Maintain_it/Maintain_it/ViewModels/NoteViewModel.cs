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

namespace Maintain_it.ViewModels
{
    public class NoteViewModel : BaseViewModel
    {
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
                string intent = await Shell.Current.DisplayPromptAsync( Alerts.ReplaceImageTitle, Alerts.ReplaceImageMessage, Alerts.Confirmation, Alerts.Cancel );

                switch( intent )
                {
                    case Alerts.Confirmation:
                        break;
                    case Alerts.Cancel:
                        return;
                }
            }

            FileResult photo = await MediaPicker.CapturePhotoAsync();

            if(photo == null )
            {
                return;
            }

            Stream stream = await photo.OpenReadAsync();
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo( memoryStream );
            byte[] photoBytes = memoryStream.ToArray();

            note.ImageData = photoBytes;
            Image = ImageSource.FromStream( () => memoryStream );
        }

        #endregion

        #region METHODS

        public async Task Init( int id )
        {
            note = await NoteManager.GetItemAsync( id );

            if( note.ImageData != default )
            {
                Image = ImageSource.FromStream( () => new MemoryStream( note.ImageData ) );
            }
            else if( note.ImagePath != string.Empty )
            {
                Image = ImageSource.FromFile( note.ImagePath );
            }
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
            
            }
        }
        #endregion

    }
}
