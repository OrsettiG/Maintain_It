using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    public static class Alerts
    {
        #region Generic Strings
        /// <summary>
        /// "Information"
        /// </summary>
        public const string Information = "Information";



        /// <summary>
        /// "Ok"
        /// </summary>
        public const string Confirmation = "Ok";

        /// <summary>
        /// "Cancel"
        /// </summary>
        public const string Cancel = "Cancel";

        /// <summary>
        /// "Create New"
        /// </summary>
        public const string CreateNew = "Create New";

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

        /// <summary>
        /// "Yes"
        /// </summary>
        public const string Yes = "Yes";

        /// <summary>
        /// "No"
        /// </summary>
        public const string No = "No";
        #endregion Generic Strings

        #region ShoppingList Strings
        /// <summary>
        /// "You already have all the looseMaterials required for this project. Would you still like to add them to a shopping list?"
        /// </summary>
        public const string MaterialsAlreadyOwned = "You already have all the looseMaterials required for this project. Would you still like to add them to a shopping list?";
        #endregion ShoppingList Strings

        #region ServiceItem Strings
        ///<summary>
        ///"Set Project Active?"
        /// </summary>
        public const string SetProjectActive = "Set Project Active?";

        /// <summary>
        /// "This project is currently marked as Inactive and will not show up under \"Active Projects\" or \"Suspended Projects\". Would you like to mark it active?"
        /// </summary>
        public const string UpdateProjectActiveState_InactiveMessage = "This project is currently marked as Inactive and will not show up under \"Active Projects\" or \"Suspended Projects\". Would you like to mark it active?";

        /// <summary>
        /// "This project is currently marked as Suspended and will not show up under \"Active Projects\". Would you like to mark it active?"
        /// </summary>
        public const string UpdateProjectActiveState_SuspendedMessage = "This project is currently marked as Suspended and will not show up under \"Active Projects\". Would you like to mark it active?";
        #endregion ServiceItem Strings

        #region Camera/Image Strings
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
        #endregion Camera/Image Strings

        #region Step Strings
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
        #endregion Step Strings

        #region ShoppingList Strings
        public const string AddToShoppingListTitle = "Available Shopping Lists";
        #endregion ShoppingList Strings

        #region Tag Strings
        public const string UniqueTagNameErrorMessage = "Tags must be unique, please enter a different tag and try again";
        #endregion
    }
}
