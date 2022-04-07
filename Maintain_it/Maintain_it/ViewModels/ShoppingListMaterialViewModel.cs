using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialViewModel : BaseViewModel
    {

        public ShoppingListMaterialViewModel( ShoppingListMaterial shoppingListMaterial )
        {
            _shoppingListMaterial = shoppingListMaterial;
            MaterialId = shoppingListMaterial.MaterialId;
            Material = shoppingListMaterial.Material;
            ShoppingListId = shoppingListMaterial.ShoppingListId;
            ShoppingList = shoppingListMaterial.ShoppingList;
            Name = shoppingListMaterial.Name;
            Quantity = shoppingListMaterial.Quantity;
            Purchased = shoppingListMaterial.Purchased;
        }

        #region Properties

        private ShoppingListMaterial _shoppingListMaterial;
        public ShoppingListMaterial ShoppingListMaterial { get => _shoppingListMaterial; }

        private int materialId;
        public int MaterialId { get => materialId; set => SetProperty( ref materialId, value); }

        private Material material;
        public Material Material { get => material; set => SetProperty( ref material, value); }

        private int shoppingListId;
        public int ShoppingListId { get => shoppingListId; set => SetProperty( ref shoppingListId, value); }

        private ShoppingList shoppingList;
        public ShoppingList ShoppingList { get => shoppingList; set => SetProperty( ref shoppingList, value); }

        private string name;
        public string Name { get => name; set => SetProperty( ref name, value ); }

        private int quantity;
        public int Quantity { get => quantity; set => SetProperty( ref quantity, value ); }

        private bool purchased;
        public bool Purchased { get => purchased; set => SetProperty( ref purchased, value ); }
               
        #endregion


    }
}
