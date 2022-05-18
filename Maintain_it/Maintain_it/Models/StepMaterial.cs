using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using SQLite;

using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class StepMaterial : IStorableObject, IEquatable<StepMaterial>
    {
        public StepMaterial() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        #region Properties
#nullable enable
        public string? Name { get; set; }
#nullable disable
        public int Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
        #endregion

        #region Many To One Relationships

        // Material
        [ForeignKey( typeof( Material ) )]
        public int MaterialId { get; set; }
        [ManyToOne]
        public Material Material { get; set; }
        
        // Step
        [ForeignKey( typeof( Step ) )]
        public int StepId { get; set; }
        [ManyToOne]
        public Step Step { get; set; }

        #endregion

        public bool Equals( StepMaterial other )
        {
            return other.Id == Id;
        }
    }
}
