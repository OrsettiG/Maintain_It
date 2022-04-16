using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;

namespace Maintain_it.ViewModels
{
    public class MaterialViewModel : BaseViewModel
    {
        #region Constructors
        public MaterialViewModel() { }


        public MaterialViewModel( Material material )
        {
            _material = material;

            Name = material.Name;
            Description = material.Description;
            Size = material.Size;
            QuantityOwned = material.QuantityOwned;
            Tags.AddRange( material.Tags );
        }

        #endregion
        #region Properties

        private Material _material;
        public Material Material 
        { 
            get => _material; 
        }

        private string name;
        public string Name 
        { 
            get => name; 
            set => SetProperty( ref name, value ); 
        }

        private string description;
        public string Description 
        { 
            get => description; 
            set => SetProperty( ref description, value ); 
        }

        private double? size;
        public double? Size 
        { 
            get => size; 
            set => SetProperty( ref size, value ); 
        }

        private int quantityOwned;
        public int QuantityOwned 
        { 
            get => quantityOwned; 
            set => SetProperty( ref quantityOwned, value ); 
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set => SetProperty( ref selected, value );
        }

        private ObservableRangeCollection<Tag> tags;
        public ObservableRangeCollection<Tag> Tags 
        { 
            get => tags ??= new ObservableRangeCollection<Tag>(); 
            set => SetProperty( ref tags, value ); 
        }

        private ConcurrentDictionary<int, byte> uniqueSteps = new ConcurrentDictionary<int, byte>();
        private ObservableRangeCollection<Step> steps;
        public ObservableRangeCollection<Step> Steps 
        { 
            get => steps ??= new ObservableRangeCollection<Step>(); 
            set => SetProperty( ref steps, value ); 
        }

        private ConcurrentDictionary<int, byte> uniqueShoppingLists = new ConcurrentDictionary<int, byte>();
        private ObservableRangeCollection<ShoppingList> shoppingLists;
        public ObservableRangeCollection<ShoppingList> ShoppingLists 
        { 
            get => shoppingLists ??= new ObservableRangeCollection<ShoppingList>(); 
            set => SetProperty( ref shoppingLists, value ); 
        }

        private ConcurrentDictionary<int, byte> uniqueRetailers = new ConcurrentDictionary<int, byte>();
        private ObservableRangeCollection<Retailer> retailers;
        public ObservableRangeCollection<Retailer> Retailers 
        { 
            get => retailers ??= new ObservableRangeCollection<Retailer>(); 
            set => SetProperty( ref retailers, value ); 
        }
        #endregion

        #region Methods
        private async Task Init()
        {
            List<StepMaterial> stepMaterials = await DbServiceLocator.GetAllItemsAsync<StepMaterial>() as List<StepMaterial>;
            List<ShoppingListMaterial> shoppingListMaterials = await DbServiceLocator.GetAllItemsAsync<ShoppingListMaterial>() as List<ShoppingListMaterial>;
            List<RetailerMaterial> retailerMaterials = await DbServiceLocator.GetAllItemsAsync<RetailerMaterial>() as List<RetailerMaterial>;

            //Steps
            _ = Parallel.ForEach( stepMaterials, sM =>
            {
                if( sM.MaterialId == _material.Id )
                {
                    _ = uniqueSteps.GetOrAdd( sM.StepId, byte.MinValue );
                }
            } );

            //ShoppingLists
            _ = Parallel.ForEach( shoppingListMaterials, sLM =>
            {
                if( sLM.MaterialId == _material.Id )
                {
                    _ = uniqueShoppingLists.GetOrAdd( sLM.ShoppingListId, byte.MinValue );
                }
            } );

            //Retailers
            _ = Parallel.ForEach( retailerMaterials, rM => 
            {
                if( rM.MaterialId == _material.Id )
                {
                    _ = uniqueRetailers.GetOrAdd( rM.RetailerId, byte.MinValue );
                }
            } );

            Steps.AddRange( await DbServiceLocator.GetItemRangeAsync<Step>( uniqueSteps.Keys ) );
            ShoppingLists.AddRange( await DbServiceLocator.GetItemRangeAsync<ShoppingList>( uniqueShoppingLists.Keys ) );
            Retailers.AddRange( await DbServiceLocator.GetItemRangeAsync<Retailer>( uniqueRetailers.Keys ) );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await Init();
        }
        #endregion
        #endregion
    }
}