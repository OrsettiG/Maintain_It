using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class NoteViewModel : BaseViewModel
    {
        private Note note;

        private int noteId;
        public int NoteId { get => noteId; private set => SetProperty( ref noteId, value ); }

        private string text;
        public string Text { get => text; private set => SetProperty( ref text, value ); }

        private DateTime lastUpdated;
        public DateTime LastUpdated { get => lastUpdated; private set => SetProperty( ref lastUpdated, value ); }

        private DateTime createdOn;
        public DateTime CreatedOn { get => createdOn; private set => SetProperty( ref createdOn, value ); }

        

    }
}
