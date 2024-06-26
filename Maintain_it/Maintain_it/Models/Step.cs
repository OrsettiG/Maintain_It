﻿using System;
using System.Collections.Generic;
using Maintain_it.Models.Interfaces;
using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Step : IStorableObject, IEquatable<Step>, INode<Step>
    {
        public Step() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region One To Many Relationships

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead )]
        public List<Note> Notes { get; set; }

        [OneToMany( CascadeOperations = CascadeOperation.CascadeRead ), NotNull]
        public List<StepMaterial> StepMaterials { get; set; }

        #endregion

        #region Many To One Relationships

        [ForeignKey( typeof( ServiceItem ) )]
        public int MaintenanceItemId { get; set; }
        [ManyToOne]
        public ServiceItem MaintenanceItem { get; set; }

        #endregion

        #region INode Implementation
        [NotNull]
        public int Index { get; set; }

#nullable enable
        #region One to One Relationships

        public int NextNodeId { get; set; }
        public int PreviousNodeId { get; set; }

        #endregion
        #endregion

        #region Properties

        public string? Name { get; set; }
#nullable disable
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public double TimeRequired { get; set; }
        public int Timeframe { get; set; }
        public DateTime CreatedOn { get; set; }

        public int CompareTo( Step other )
        {
            return other == null ? 1 : Index.CompareTo( other.Index );
        }

        public bool Equals( Step other )
        {
            return other?.Id == Id;
        }

        #endregion
    }
}