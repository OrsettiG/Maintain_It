using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialViewModel : BaseViewModel
    {

        public ShoppingListMaterialViewModel( ShoppingListMaterial shoppingListMaterial )
        {
            _shoppingListMaterial = shoppingListMaterial;
            Material = shoppingListMaterial.Material;
            ShoppingList = shoppingListMaterial.ShoppingList;
            Name = shoppingListMaterial.Name;
            Quantity = shoppingListMaterial.Quantity;
            Purchased = shoppingListMaterial.Purchased;
            Tags = shoppingListMaterial.Material.Tags.Where( x => x.TagType == TagType.ShoppingList || x.TagType == TagType.General ) as ObservableRangeCollection<Tag>;
        }
        public ShoppingListMaterialViewModel( Material material, ShoppingList shoppingList )
        {
            _shoppingListMaterial = new ShoppingListMaterial()
            {
                Material = material,
                MaterialId = material.Id,
                ShoppingList = shoppingList,
                Name = material.Name,
                Quantity = 1,
                Purchased = false
            };

            Material = material;
            ShoppingList = shoppingList;
            Name = material.Name;
            Quantity = 1;
            Purchased = false;

            Tags = material.Tags.Where(x => x.TagType == TagType.ShoppingList || x.TagType == TagType.General ) as ObservableRangeCollection<Tag>;
        }

        #region Properties

        private ShoppingListMaterial _shoppingListMaterial;
        public ShoppingListMaterial ShoppingListMaterial { get => _shoppingListMaterial; }

        private Material material;
        public Material Material { get => material; set => SetProperty( ref material, value); }

        private ShoppingList shoppingList;
        public ShoppingList ShoppingList { get => shoppingList; set => SetProperty( ref shoppingList, value); }

        private string name;
        public string Name { get => name; set => SetProperty( ref name, value ); }

        private int quantity;
        public int Quantity { get => quantity; set => SetProperty( ref quantity, value ); }

        private bool purchased;
        public bool Purchased { get => purchased; set => SetProperty( ref purchased, value ); }

        private ObservableRangeCollection<Tag> tags;
        public ObservableRangeCollection<Tag> Tags { get => tags ??= new ObservableRangeCollection<Tag>(); set => SetProperty( ref tags, value ); }

        #endregion

        #region Commands
        private AsyncCommand crossOffCommand;
        public ICommand CrossOffCommand { get => crossOffCommand ??= new AsyncCommand( CrossOff ); }
        private async Task CrossOff()
        {
            Purchased = true;
        }

        internal async Task<int> UpdateAndReturnIdAsync()
        {
            ShoppingListMaterial.Name = Name;
            ShoppingListMaterial.Quantity = Quantity;
            ShoppingListMaterial.Material = Material;
            ShoppingListMaterial.Purchased = Purchased;
            ShoppingListMaterial.ShoppingList = ShoppingList;

            return await DbServiceLocator.AddItemAndReturnIdAsync( ShoppingListMaterial );
        }
        #endregion
    }
}
