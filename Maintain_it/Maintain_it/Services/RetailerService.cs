using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.Services
{
    public class RetailerService : Service<Retailer>
    {
        internal static Retailer defaultRetailer = new Retailer()
        {
            Name = "Default Retailer",
            Materials = new List<Material>()
            {
                MaterialService.defaultMaterial
            }
        };
    }
}
