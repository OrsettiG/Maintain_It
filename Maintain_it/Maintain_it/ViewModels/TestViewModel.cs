﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
//using System.Windows.Input;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Essentials;
using Xamarin.Forms;

using static System.Net.Mime.MediaTypeNames;

namespace Maintain_it.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public TestViewModel()
        {
        }

        private Note note;
        private byte[] imageData;

        private ObservableRangeCollection<NoteViewModel> noteViewModels;
        public ObservableRangeCollection<NoteViewModel> NoteViewModels
        {
            get => noteViewModels ??= new ObservableRangeCollection<NoteViewModel>();
            set => SetProperty( ref noteViewModels, value );
        }

        private ImageSource image;
        public ImageSource Image
        {
            get => image;
            set => SetProperty( ref image, value );
        }

        private string noteText;
        public string NoteText
        {
            get => noteText;
            set => SetProperty( ref noteText, value );
        }

        private ImageSource databaseImage;
        public ImageSource DatabaseImage
        {
            get => databaseImage;
            set => SetProperty( ref databaseImage, value );
        }

        private string databaseNoteText;
        public string DatabaseNoteText
        {
            get => databaseNoteText;
            set => SetProperty( ref databaseNoteText, value );
        }

        // AddShallow Note
        private AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand
        {
            get => addNoteCommand ??= new AsyncCommand( AddNote );
        }
        private async Task AddNote()
        {
            int id = await NoteManager.NewNote( NoteText );

            if( Image != default )
            {
                await NoteManager.AddImageToNote( id, Image );
            }
            await Refresh();
        }

        // Take Photo
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
        }

        // ItemDeleted View
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand
        {
            get => refreshCommand ??= new AsyncCommand( Refresh );
        }

        private async Task Refresh()
        {
            List<Note> notes = await NoteManager.GetAllItemsAsync();

            ConcurrentBag<NoteViewModel> vms = new ConcurrentBag<NoteViewModel>();

            ParallelLoopResult parallelLoopResult = Parallel.ForEach( notes, note =>
            {
                
                using MemoryStream mStream = new MemoryStream();
                NoteViewModel vm = new NoteViewModel( note.Id )
                {

                    Text = note.Text,


                    Image = note.ImageData != default( byte[] )
                            ? ImageSource.FromStream( () => new MemoryStream( note.ImageData ) )
                            : note.ImagePath != string.Empty
                            ? ImageSource.FromFile( note.ImagePath )
                            : default, // this last option will be a default "no photo" image of some sort.
                    StepId = note.StepId,
                    LastUpdated = note.LastUpdated.ToLocalTime(),
                    CreatedOn = note.CreatedOn.ToLocalTime()
                };

                vms.Add( vm );

            });

            if( parallelLoopResult.IsCompleted )
            {

                NoteViewModels.Clear();

                NoteViewModels.AddRange( vms.OrderByDescending( x => x.LastUpdated ) );

                Image = default;
                NoteText = string.Empty;
            }
            else
            {
                await Shell.Current.DisplayAlert( Alerts.Error, Alerts.DatabaseErrorMessage, Alerts.Confirmation );
            }
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

    }
}
