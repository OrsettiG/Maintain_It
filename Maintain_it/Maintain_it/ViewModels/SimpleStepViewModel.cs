using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.ViewModels
{
    internal class SimpleStepViewModel : BaseViewModel
    {

        public SimpleStepViewModel( Step step )
        {
            MaintenanceItemName = step.MaintenanceItem.Name;
            StepName = step.Name;
            StepNumber = step.Index;
            StepId = step.Id;
            Description = step.Description;
            IsCompleted = step.IsCompleted;
            TimeRequired = step.TimeRequired;
            Timeframe = step.Timeframe;
            CreatedOn = step.CreatedOn;

            foreach( StepMaterial sm in step.StepMaterials )
            {
                Dictionary<string, string> props = new Dictionary<string, string>
                {
                    { nameof( sm.Quantity ), sm.Quantity.ToString() },
                    { nameof( sm.Name ), sm.Name },
                    { nameof( sm.CreatedOn ), sm.CreatedOn.ToString() },
                    { nameof( sm.MaterialId), sm.MaterialId.ToString() }
                };

                StepMaterialId_Values.Add( sm.Id, props );
            }
        }

        #region Fields
        private int stepId;
        private string stepName;
        private int stepNumber;
        private string description;
        private bool isCompleted;
        private double timeRequired;
        private int timeframe;
        private string maintenanceItemName;
        private DateTime createdOn;
        private Dictionary<int, Dictionary<string, string>> stepMaterialId_Values;
        #endregion

        public string MaintenanceItemName
        {
            get => maintenanceItemName;
            private set => SetProperty( ref maintenanceItemName, value );
        }

        public string StepName
        {
            get => stepName;
            private set => SetProperty( ref stepName, value );
        }

        public string Description
        {
            get => description;
            set => SetProperty( ref description, value);
        }
        public bool IsCompleted
        {
            get => isCompleted;
            set => SetProperty( ref isCompleted, value);
        }
        public double TimeRequired
        {
            get => timeRequired;
            set => SetProperty( ref timeRequired, value);
        }
        public int Timeframe
        {
            get => timeframe;
            set => SetProperty( ref timeframe, value);
        }
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetProperty( ref createdOn, value);
        }

        public int StepNumber
        {
            get => stepNumber;
            private set => SetProperty( ref stepNumber, value );
        }

        public int StepId
        {
            get => stepId;
            private set => SetProperty( ref stepId, value );
        }

        public Dictionary<int, Dictionary<string, string>> StepMaterialId_Values
        {
            get => stepMaterialId_Values ??= new Dictionary<int, Dictionary<string, string>>();
            private set => SetProperty( ref stepMaterialId_Values, value );
        }
    }
}
