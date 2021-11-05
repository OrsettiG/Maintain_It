using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maintain_it.ViewModels
{
    public class MaterialViewModel : BaseViewModel
    {
        public MaterialViewModel()
        {

        }

        public string Name { get; set; }
        public double UnitPrice { get; set; }

        private protected override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            throw new System.NotImplementedException();
        }
    }
}