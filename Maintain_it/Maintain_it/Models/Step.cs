using System;
using System.Collections.Generic;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Step : IStorableObject
    {
        public Step() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region One To Many Relationships

        [OneToMany( CascadeOperations = CascadeOperation.All )]
        public List<Note> Notes { get; set; }

        #endregion

        #region Many To One Relationships

        [ForeignKey( typeof( MaintenanceItem ) )]
        public int MaintenanceItemId { get; set; }
        [ManyToOne]
        public MaintenanceItem MaintenanceItem { get; set; }

        #endregion

        #region Many to Many Relationships

        [ManyToMany( typeof( StepsToStepMaterials ) )]
        public List<StepMaterial> StepMaterials { get; set; }

        #endregion

        #region Properties

#nullable enable
        public string? Name { get; set; }
#nullable disable
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public double TimeRequired { get; set; }
        public int Timeframe { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion
    }
}