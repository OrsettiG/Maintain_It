using SQLiteNetExtensions.Attributes;

namespace Maintain_it.Models
{
    internal class ItemsToMaterials
    {
        [ForeignKey( typeof( Material ) )]
        public int MaterialId { get; set; }

        [ForeignKey( typeof( MaintenanceItem ) )]
        public int MaintenanceItemId { get; set; }
    }
}