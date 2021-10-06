using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    public class StepsToStepMaterials
    {
        [ForeignKey( typeof( Step ) )]
        public int StepId { get; set; }

        [ForeignKey( typeof( StepMaterial ) )]
        public int StepMaterialId { get; set; }
    }
}