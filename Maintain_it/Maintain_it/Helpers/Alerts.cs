using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public static class Alerts
    {
        /// <summary>
        /// "Information"
        /// </summary>
        public const string Information = "Information";

        /// <summary>
        /// "You already have all the materials required for this project. You can get right to work, no need for shopping."
        /// </summary>
        public const string MaterialsAlreadyOwned = "You already have all the materials required for this project. You can get right to work, no need for shopping.";

        /// <summary>
        /// "Ok"
        /// </summary>
        public const string Confirmation = "Ok";

        public const string Cancel = "Cancel";

        /// <summary>
        /// "Error"
        /// </summary>
        public const string Error = "Error";

        ///<summary>
        /// "Oops! Looks like something went wrong saving that. Make sure all the data is correct and try again."
        /// </summary>
        public const string DatabaseErrorMessage = "Oops! Looks like something went wrong saving that. Make sure all the data is correct and try again.";

        public const string DiscardChangesTitle = "Discard Changes?";

        public const string SaveOrDiscardChangesMessage = "Would you like to save your changes, or discard them?";

        public const string Discard = "Discard";

        public const string Save = "Save";
    }
}
