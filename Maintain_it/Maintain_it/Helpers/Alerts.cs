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

        /// <summary>
        /// "Cancel"
        /// </summary>
        public const string Cancel = "Cancel";

        /// <summary>
        /// "Error"
        /// </summary>
        public const string Error = "Error";

        ///<summary>
        /// "Oops! Looks like something went wrong saving that. Make sure all the data is correct and try again."
        /// </summary>
        public const string DatabaseErrorMessage = "Oops! Looks like something went wrong saving that. Make sure all the data is correct and try again.";

        /// <summary>
        /// "Discard Changes?"
        /// </summary>
        public const string DiscardChangesTitle = "Discard Changes?";

        /// <summary>
        /// "Would you like to save your changes, or discard them?"
        /// </summary>
        public const string SaveOrDiscardChangesMessage = "Would you like to save your changes, or discard them?";

        /// <summary>
        /// "Discard"
        /// </summary>
        public const string Discard = "Discard";

        /// <summary>
        /// "Save"
        /// </summary>
        public const string Save = "Save";

        ///<summary>
        ///"Set Project Active?"
        /// </summary>
        public const string SetProjectActive = "Set Project Active?";

        /// <summary>
        /// "This project is currently not active and will not show up in your "Upcoming Projects". Would you like to mark it active?"
        /// </summary>
        public const string ProjectActiveStateMessage = "This project is currently not active and will not show up in your \"Upcoming Projects\". Would you like to mark it active?";

        /// <summary>
        /// "Yes"
        /// </summary>
        public const string Yes = "Yes";

        /// <summary>
        /// "No"
        /// </summary>
        public const string No = "No";

        /// <summary>
        /// "There was an error accessing the camera. Please ensure that the app has permission and try again."
        /// </summary>
        public const string CameraErrorMessage = "There was an error accessing the camera. Please ensure that the app has permission and try again.";

        /// <summary>
        /// "Replace Image?"
        /// </summary>
        public const string ReplaceImageTitle = "Replace Image?";

        /// <summary>
        /// "This Item already has an image associated with it, are you sure you want to replace it?"
        /// </summary>
        public const string ReplaceImageMessage = "This Item already has an image associated with it, are you sure you want to replace it?";


        /// <summary>
        /// "Are you sure you want to permenantly delete this step? This action is irreversable."
        /// </summary>
        public const string DeleteStepMessage = "Are you sure you want to permenantly delete this step? This action is irreversable.";

        /// <summary>
        /// "Yes, delete it"
        /// </summary>
        public const string ConfirmDelete = "Yes, delete it";

        /// <summary>
        /// "No, keep it"
        /// </summary>
        public const string CancelDelete = "No, keep it";

        /// <summary>
        /// "Delete Step?"
        /// </summary>
        public const string DeleteStepTitle = "Delete Step?";
    }
}
