namespace Maintain_it.Models
{
    public class Material
    {
        public Material( string Name, string Retailer, double UnitPrice, int Quantity )
        {
            this.Name = Name;
            this.Retailer = Retailer;
            this.UnitPrice = UnitPrice;
            this.Quantity = Quantity;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Retailer { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
    }
}