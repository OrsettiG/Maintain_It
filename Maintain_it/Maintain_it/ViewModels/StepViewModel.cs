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
using System.Linq;
using System.Web;

namespace Maintain_it.ViewModels
{
    public class StepViewModel : BaseViewModel
    {
        public StepViewModel() { }

        #region Parameters

        private string _name;
        public string Name { get => _name; set => SetProperty( ref _name, value ); }

        private int stepNum;
        public int StepNum { get => stepNum; set => SetProperty( ref stepNum, value ); }

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

        private int _stepMatCounter;
        public int StepMatCounter { get => _stepMatCounter; set => SetProperty( ref _stepMatCounter, value >= 0 ? value : 0 ); }

        private ObservableRangeCollection<StepMaterial> _stepMaterials;
        public ObservableRangeCollection<StepMaterial> StepMaterials { get => _stepMaterials ??= new ObservableRangeCollection<StepMaterial>(); set => SetProperty( ref _stepMaterials, value ); }
        private ObservableRangeCollection<Note> notes;
        public ObservableRangeCollection<Note> Notes => notes ??= new ObservableRangeCollection<Note>();

        public List<Timeframe> timeframes => Options.timeframes;

        private Step step;
        public Step Step { get => step; set => step = value; }

        private Step nextStep;
        public Step NextStep { get => nextStep; set => SetProperty( ref nextStep, value ); }

        private Step previousStep;
        public Step PreviousStep { get => previousStep; set => SetProperty( ref previousStep, value ); }

        private NodeList<Step> node;
        public NodeList<Step> Node { get => node ??= new NodeList<Step>(Step); set => SetProperty( ref node, value ); } 

        #region Query Parameters
        private HashSet<int> stepMaterialIds = new HashSet<int>();
        private int previousStepId;
        private bool isFirstStep = false;
        #endregion

        #endregion

        #region COMMANDS

        private AsyncCommand addStepCommand;
        public ICommand AddStepCommand => addStepCommand ??= new AsyncCommand( AddStep );

        private AsyncCommand saveStepCommand;
        public ICommand SaveStepCommand => saveStepCommand ??= new AsyncCommand( SaveStep );

        private AsyncCommand selectMaterialsCommand;
        public ICommand SelectMaterialsCommand => selectMaterialsCommand ??= new AsyncCommand( SelectMaterials );

        private AsyncCommand addNoteCommand;
        public ICommand AddNoteCommand => addNoteCommand ??= new AsyncCommand( AddNote );

        private AsyncCommand takePhotoCommand;
        public ICommand TakePhotoCommand => takePhotoCommand ??= new AsyncCommand( TakePhoto );

        private Command decrementStepMatQuantityCommand;
        public ICommand DecrementStepMatQuantityCommand => decrementStepMatQuantityCommand ??= new Command( DecrementStepMatCounter );

        private Command incrementStepMatQuantityCommand;
        public ICommand IncrementStepMatQuantityCommand => incrementStepMatQuantityCommand ??= new Command( IncrementStepMatCounter );

        #endregion

        #region METHODS

        public void Init()
        {
            Name = step.Name;
            Description = step.Description;
            TimeRequired = step.TimeRequired;
            Timeframe = (Timeframe)step.Timeframe;
            IsCompleted = step.IsCompleted;
            StepNum = step.StepNumber;
            StepMaterials.AddRange( step.StepMaterials );
            Notes.AddRange( step.Notes );
            //Node.InsertNewNodeAhead( Step.NextStep );
        }

        public async Task InitAsync()
        {
            Task[] tasks = new Task[]
            {
                new Task( () => Name = step.Name ),
                new Task( () => Description = step.Description ),
                new Task( () => TimeRequired = step.TimeRequired ),
                new Task( () => Timeframe = (Timeframe)step.Timeframe ),
                new Task( () => IsCompleted = step.IsCompleted ),
                new Task( () => StepNum = step.StepNumber ),
                new Task( () => StepMaterials.AddRange( step.StepMaterials ) ),
                new Task( () => Notes.AddRange( step.Notes ) )
            };

            await Task.WhenAll( tasks ).ConfigureAwait( false );
        }

        private void DecrementStepMatCounter()
        {
            StepMatCounter--;
        }

        private void IncrementStepMatCounter()
        {
            StepMatCounter++;
        }

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
                StepNumber = StepNum,
                PreviousStep = PreviousStep,
                NextStep = NextStep,
                StepMaterials = StepMaterials.Count < 1 ? new List<StepMaterial>() : await ConvertToListAsync( StepMaterials ),
                Notes = await ConvertToListAsync( Notes )
            };

            int stepId = await DbServiceLocator.AddItemAndReturnIdAsync( step );

            if( StepNum > 1 )
            {
                step.PreviousStep.NextStep = await DbServiceLocator.GetItemAsync<Step>( stepId ).ConfigureAwait( false );
            }

            string encodedId = HttpUtility.UrlEncode( stepId.ToString());
            await Shell.Current.GoToAsync( $"..?stepIds={encodedId}" );
        }

        private async Task SaveStep()
        {
            await DbServiceLocator.UpdateItemAsync( step ).ConfigureAwait( false );
        }

        private async Task SelectMaterials()
        {
            if( stepMaterialIds.Count > 0 )
            {
                string encodedIds = HttpUtility.UrlEncode( string.Join( ',', stepMaterialIds ) );
                await Shell.Current.GoToAsync( $"/{nameof( AddStepMaterialsToStepView )}?preselectedStepMaterialIds={encodedIds}" );
            }
            else
            {
                await Shell.Current.GoToAsync( $"/{nameof( AddStepMaterialsToStepView )}?refresh=true" );
            }
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

        #endregion

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case nameof( stepMaterialIds ):
                    ParseStepMaterialIds( kvp.Value );
                    await RetrieveStepMaterialsFromDb();
                    break;
                case nameof( previousStepId ):
                    await ParseStepNumber( kvp.Value );
                    break;
                case nameof( isFirstStep ):
                    isFirstStep = true;
                    StepNum = 1;
                    PreviousStep = null;
                    NextStep = null;
                    break;
                default:
                    break;
            }
        }

        private void ParseStepMaterialIds( string encodedValue )
        {
            string decodedValue = HttpUtility.UrlDecode( encodedValue );
            string[] ids = decodedValue.Split(',');
            stepMaterialIds = new HashSet<int>( ids.Length );
            foreach( string id in ids )
            {
                if( int.TryParse( id, out int result ) )
                {
                    _ = stepMaterialIds.Add( result );
                }
            }
        }

        private async Task ParseStepNumber( string value )
        {
            string num = HttpUtility.UrlDecode( value );


            if( int.TryParse( num, out previousStepId ) )
            {
                PreviousStep = await DbServiceLocator.GetItemRecursiveAsync<Step>( previousStepId ).ConfigureAwait( false );
                stepNum = PreviousStep.StepNumber + 1;

                NextStep = null;
            }
        }

        private async Task RetrieveStepMaterialsFromDb()
        {
            List<StepMaterial> stepMats = await DbServiceLocator.GetItemRangeAsync<StepMaterial>( stepMaterialIds ) as List<StepMaterial>;
            StepMaterials.Clear();
            StepMaterials.AddRange( stepMats );
        }
        #endregion
    }
}
