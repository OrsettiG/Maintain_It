﻿using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class Note : IStorableObject
    {
        public Note() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region One To Many Relationships
        #endregion

        #region Many To One Relationships
        [ForeignKey( typeof( Step ) )]
        public int StepId { get; set; }
        [ManyToOne]
        public Step Step { get; set; }
        #endregion

        #region Many To Many Relationships
        #endregion

        #region Properties
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        #endregion
    }
}
