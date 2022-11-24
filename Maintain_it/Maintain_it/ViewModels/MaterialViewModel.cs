using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;

using Xamarin.Essentials;

namespace Maintain_it.ViewModels
{
    public class MaterialViewModel : BaseViewModel
    {
        #region Constructors
        public MaterialViewModel() { }


        public MaterialViewModel( Material material )
        {
            this.material = material;

            Name = material.Name;
            Description = material.Description;
            QuantityOwned = material.QuantityOwned;
            Size = material.Size;
            Units = material.Units;
            PartNumber = material.PartNumber;
            Tags.AddRange( material.Tags );

            CreatedOn = material.CreatedOn;
        }

        #endregion

        #region Properties

        private Material material;
        public Material Material
        {
            get => material;
            private set => SetProperty( ref material, value );
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

        private string partNumber;
        public string PartNumber
        {
            get => partNumber;
            set => SetProperty( ref partNumber, value );
        }

        private double? size;
        public double? Size
        {
            get => size;
            set => SetProperty( ref size, value );
        }

        private string units;
        public string Units
        {
            get => units;
            set => SetProperty( ref units, value );
        }

        private int quantityOwned;
        public int QuantityOwned
        {
            get => quantityOwned;
            set => SetProperty( ref quantityOwned, value );
        }

        private DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn;
            private set => SetProperty(ref createdOn, value );
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
            HashSet<int>stepIds = new HashSet<int>();
            foreach( StepMaterial mat in Material.StepMaterials )
            {
                _ = stepIds.Add( mat.StepId );
            }

            HashSet<int>shoppingListIds = new HashSet<int>();
            foreach( ShoppingListMaterial mat in Material.ShoppingListMaterials )
            {
                _ = shoppingListIds.Add( mat.ShoppingListId );
            }

            List<Step> steps = await DbServiceLocator.GetItemRangeRecursiveAsync<Step>( stepIds ) as List<Step>;


            List<ShoppingList> shoppingLists = await DbServiceLocator.GetItemRangeRecursiveAsync<ShoppingList>(shoppingListIds) as List<ShoppingList>;

            //Steps
            _ = Parallel.ForEach( steps, step =>
            {
                if( step != null )
                {
                    _ = uniqueSteps.GetOrAdd( step.Id, new StepViewModel( step ) );
                }
            } );

            //ShoppingLists
            _ = Parallel.ForEach( shoppingLists, shoppingList =>
            {
                if( shoppingList != null )
                {
                    _ = uniqueShoppingLists.GetOrAdd( shoppingList.Id, new ShoppingListViewModel( shoppingList ) );
                }
            } );

            Name = Material.Name;
            Description = Material.Description;
            PartNumber = Material.PartNumber;
            QuantityOwned = Material.QuantityOwned;
            Size = Material.Size;
            Units = Material.Units ?? "N/A";
            CreatedOn = Material.CreatedOn;
            Tags.Clear();
            Tags.AddRange(Material.Tags);
            Steps.AddRange( uniqueSteps.Values );
            ShoppingLists.AddRange( uniqueShoppingLists.Values );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.MaterialID:
                    if( int.TryParse( kvp.Value, out int materialId ) )
                    {
                        Material = await DbServiceLocator.GetItemRecursiveAsync<Material>( materialId );
                        await Init();
                    }

                    break;
            }
        }
        #endregion
        #endregion
    }
}