using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class TagViewModel : BaseViewModel, IEquatable<TagViewModel>
    {
        #region Constructors
        public TagViewModel() { }

        public TagViewModel( Tag tag )
        {
            this.tag = tag;
            id = tag.Id;
            Name = tag.Name;
            Materials = new ObservableRangeCollection<Material>( tag.Materials );
            createdOn = tag.CreatedOn;
        }
        
        public TagViewModel( Tag tag, Material material )
        {
            this.tag = tag;
            id = tag.Id;
            Name = tag.Name;
            Materials = new ObservableRangeCollection<Material>( tag.Materials );
            createdOn = tag.CreatedOn;

            if( Materials.Contains(material) )
            {
                selected = true;
            }
        }
        #endregion Constructors

        #region Events
        public event Action SelectionChanged;
        #endregion

        #region Properties
        private readonly int id;
        public int Id
        {
            get => id;
        }

        private readonly Tag tag;
        public Tag Tag
        {
            get => tag;
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                _ = SetProperty( ref selected, value );
                SelectionChanged?.Invoke();
            }
        }

        ObservableRangeCollection<Material> materials;
        ObservableRangeCollection<Material> Materials
        {
            get => materials ??= new ObservableRangeCollection<Material>();
            set => SetProperty( ref materials, value );
        }

        private readonly DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn;
        }

        #endregion Properties

        #region Commands
        private AsyncCommand toggleSelectionStateCommand;
        public ICommand ToggleSelectionStateCommand
        {
            get => toggleSelectionStateCommand ??= new AsyncCommand( ToggleSelectionState );
        }
        private async Task ToggleSelectionState()
        {
            Selected = !Selected;
        }
        #endregion Commands
        #region IEquatable
        public bool Equals( TagViewModel other )
        {
            if( other == null )
                return false;
            if( tag == null )
                return false;

            return tag.Name == other.Name;
        }
        #endregion IEquatable
    }
}
