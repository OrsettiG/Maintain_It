using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Models;

namespace Maintain_it.Helpers
{
    static internal class MaterialConverter
    {
        internal static StepMaterial ConvertToStepMaterial( ShoppingListMaterial mat, Step step )
        {
            StepMaterial stepMat = new StepMaterial()
            {
                MaterialId = mat.MaterialId,
                Material = mat.Material,
                Name = mat.Name,
                Quantity = mat.Quantity,
                CreatedOn = DateTime.UtcNow,
                Step = step,
                StepId = step.Id
            };

            return stepMat;
        }
        
        internal static StepMaterial ConvertToStepMaterial( RetailerMaterial mat, Step step, int quantity = 1 )
        {
            StepMaterial stepMat = new StepMaterial()
            {
                MaterialId = mat.MaterialId,
                Material = mat.Material,
                Name = mat.Name,
                Quantity = quantity,
                CreatedOn = DateTime.UtcNow,
                Step = step,
                StepId = step.Id
            };

            return stepMat;
        }

        internal static ShoppingListMaterial ConvertToShoppingListMaterial( StepMaterial mat, ShoppingList sList )
        {
            ShoppingListMaterial shopMat = new ShoppingListMaterial()
            {
                Material = mat.Material,
                MaterialId = mat.MaterialId,
                Name = mat.Name,
                Quantity = mat.Quantity,
                Purchased = false,
                CreatedOn = DateTime.UtcNow,
                ShoppingList = sList,
                ShoppingListId = sList.Id
            };

            return shopMat;
        }
        internal static ShoppingListMaterial ConvertToShoppingListMaterial( RetailerMaterial mat, ShoppingList sList, int quantity = 1 )
        {
            ShoppingListMaterial shopMat = new ShoppingListMaterial()
            {
                Material = mat.Material,
                MaterialId = mat.MaterialId,
                Name = mat.Name,
                Quantity = quantity,
                Purchased = false,
                CreatedOn = DateTime.UtcNow,
                ShoppingList = sList,
                ShoppingListId = sList.Id
            };

            return shopMat;
        }

        internal static RetailerMaterial ConvertToRetailerMaterial( ShoppingListMaterial mat, Retailer retailer )
        {
            RetailerMaterial retMat = new RetailerMaterial()
            {
                Material = mat.Material,
                MaterialId = mat.MaterialId,
                Retailer = retailer,
                RetailerId = retailer.Id,
                Name = mat.Name,
                CreatedOn = DateTime.UtcNow
            };

            return retMat;
        }
        internal static RetailerMaterial ConvertToRetailerMaterial( StepMaterial mat, Retailer retailer )
        {
            RetailerMaterial retMat = new RetailerMaterial()
            {
                Material = mat.Material,
                MaterialId = mat.MaterialId,
                Retailer = retailer,
                RetailerId = retailer.Id,
                Name = mat.Name,
                CreatedOn = DateTime.UtcNow
            };

            return retMat;
        }
    }
}
