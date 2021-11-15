using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;
using Command = MvvmHelpers.Commands.Command;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class StepViewModel : BaseViewModel
    {
        public StepViewModel()
        {
            _ = Task.Run( async () => await DbServiceLocator.Init<Step>() );
            StepMaterials = new ObservableRangeCollection<StepMaterial>();
        }

        private string _name;
        public string Name { get => _name; set => SetProperty( ref _name, value ); }
        private string _description;
        public string Description { get => _description; set => SetProperty( ref _description, value ); }
        private bool _isCompleted;
        public bool IsCompleted { get => _isCompleted; set => SetProperty( ref _isCompleted, value ); }
        private double _timeRequired;
        public double TimeRequired { get => _timeRequired; set => SetProperty( ref _timeRequired, value ); }
        private Timeframe _timeframe;
        public Timeframe Timeframe { get => _timeframe; set => SetProperty( ref _timeframe, value ); }

        private string _noteText;
        public string NoteText { get => _noteText; set => SetProperty( ref _noteText, value ); }

        private string _noteImagePath;
        public FileImageSource NoteImagePath { get => _noteImagePath; set => SetProperty( ref _noteImagePath, value.File ); }


        public ObservableRangeCollection<StepMaterial> StepMaterials;
        private ObservableRangeCollection<Note> notes;
        public ObservableRangeCollection<Note> Notes => notes ??= new ObservableRangeCollection<Note>();

        public List<Timeframe> timeframes => Options.timeframes;

        private Step step;


        #region COMMANDS

        AsyncCommand addStepCommand;
        public ICommand AddStepCommand => addStepCommand ??= new AsyncCommand( AddStep );

        AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand => addNoteCommand ??= new AsyncCommand( AddNote );

        AsyncCommand takePhotoCommand;
        public ICommand TakePhotoCommand => takePhotoCommand ??= new AsyncCommand( TakePhoto );
        #endregion

        #region METHODS

        private async Task AddStep()
        {
            step = new Step()
            {
                Name = Name,
                Description = Description,
                TimeRequired = TimeRequired,
                Timeframe = (int)Timeframe.Minutes,
                IsCompleted = false,
                CreatedOn = DateTime.Now,

                StepMaterials = await ConvertToListAsync( StepMaterials ),
                Notes = await ConvertToListAsync( Notes )
            };

            int stepId = await DbServiceLocator.AddItemAndReturnIdAsync( step );
            Console.WriteLine( $"StepId returned: {stepId}" );
            await Shell.Current.GoToAsync( $"..?newStepId={stepId}" );
        }

        private async Task AddNote()
        {
            Note n = new Note()
            {
                CreatedOn = DateTime.Now,
                Text = NoteText,
                ImagePath = NoteImagePath.IsEmpty ? string.Empty : NoteImagePath.File,
                LastUpdated = DateTime.Now
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync( n );

            Notes.Add( await DbServiceLocator.GetItemAsync<Note>( id ) );
            
            NoteText = string.Empty;
            NoteImagePath = string.Empty;
        }

        private async Task TakePhoto()
        {
            // TODO
        }

        private protected override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
