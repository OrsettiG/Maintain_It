using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Maintain_it.Helpers;
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

        private ConcurrentDictionary<int, StepViewModel> uniqueSteps = new ConcurrentDictionary<int, StepViewModel>();
        private ObservableRangeCollection<StepViewModel> steps;
        public ObservableRangeCollection<StepViewModel> Steps
        {
            get => steps ??= new ObservableRangeCollection<StepViewModel>();
            set => SetProperty( ref steps, value );
        }

        private ConcurrentDictionary<int, ShoppingListViewModel> uniqueShoppingLists = new ConcurrentDictionary<int, ShoppingListViewModel>();
        private ObservableRangeCollection<ShoppingListViewModel> shoppingLists;
        public ObservableRangeCollection<ShoppingListViewModel> ShoppingLists
        {
            get => shoppingLists ??= new ObservableRangeCollection<ShoppingListViewModel>();
            set => SetProperty( ref shoppingLists, value );
        }
        #endregion

        #region Methods
        private async Task Init()
        {
            HashSet<int>StepIds = new HashSet<int>();
            foreach( StepMaterial mat in Material.StepMaterials )
            {
                _ = StepIds.Add( mat.StepId );
            }
            
            HashSet<int>ShoppingListIds = new HashSet<int>();
            foreach( ShoppingListMaterial mat in Material.ShoppingListMaterials )
            {
                _ = ShoppingListIds.Add( mat.ShoppingListId );
            }
            List<Step> steps = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>( StepIds ) as List<Step>;


            List<ShoppingList> shoppingListMaterials = await DbServiceLocator.GetItemRangeRecursiveAsync<ShoppingList>(ShoppingListIds) as List<ShoppingList>;

            //Steps
            _ = Parallel.ForEach( steps, sM =>
            {
                if( sM != null )
                {
                    _ = uniqueSteps.GetOrAdd( sM.Id, new StepViewModel( sM ) );
                }
            } );

            //ShoppingLists
            _ = Parallel.ForEach( shoppingListMaterials, sLM =>
            {
                if( sLM != null )
                {
                    _ = uniqueShoppingLists.GetOrAdd( sLM.Id, new ShoppingListViewModel( sLM ) );
                }
            } );

            Steps.AddRange( uniqueSteps.Values );
            ShoppingLists.AddRange( uniqueShoppingLists.Values );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case RoutingPath.MaterialID:
                    if( int.TryParse( kvp.Value, out int materialId ) )
                    {
                        _material = await DbServiceLocator.GetItemRecursiveAsync<Material>( materialId );
                        await Init();
                    }

                    break;
            }
        }
        #endregion
        #endregion
    }
}