using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;



namespace Maintain_it.ViewModels
{
    public class CreateNewShoppingListViewModel : BaseViewModel
    {
        #region Constructors

        public CreateNewShoppingListViewModel()
        {
            shoppingList = new ShoppingList();
        }

        #endregion


        #region Properties
        private readonly ShoppingList shoppingList;

        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }

        private List<ShoppingListMaterial> materials;
        public List<ShoppingListMaterial> Materials { get => materials ??= new List<ShoppingListMaterial>(); set => SetProperty(ref materials, value); }

        private List<ShoppingListMaterialViewModel> selectedMaterials;
        public List<ShoppingListMaterialViewModel> SelectedMaterial { get => selectedMaterials ??= new List<ShoppingListMaterialViewModel>(); set => SetProperty(ref selectedMaterials, value); }

        private List<int> preSelectecMaterialIds;
        #endregion

        #region Methods

        private async Task Init()
        {
            Materials = await DbServiceLocator.GetAllItemsAsync<ShoppingListMaterial>() as List<ShoppingListMaterial>;

        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await Init();

            switch( kvp.Key )
            {
                case nameof( preSelectecMaterialIds ):
                    ProcessMaterialIds( kvp.Value );
                    break;
            }

        }

        private void ProcessMaterialIds( string encodedIds )
        {
            string[] ids = HttpUtility.HtmlDecode( encodedIds ).Split(",");
            
            foreach( string id in ids )
            {
                if(int.TryParse( id, out int n ) )
                {
                    preSelectecMaterialIds.Add( n );
                }
            }

            AddPreselectedMaterialsToShoppingList();
        }

        private void AddPreselectedMaterialsToShoppingList()
        {
            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();
            _ = Parallel.ForEach( preSelectecMaterialIds, id =>
            {
                ShoppingListMaterial m = Materials.Where( x => x.Id == id ).First();

                vms.Add( new ShoppingListMaterialViewModel( m ) );
            } );

            SelectedMaterial.AddRange( vms );
        }

        #endregion
        #endregion

    }
}
