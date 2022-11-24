using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Maintain_it.Helpers;
using Maintain_it.Models;

using MvvmHelpers;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    internal class MaterialDetailViewModel : BaseViewModel
    {
        #region Constructors
        public MaterialDetailViewModel() { }

        public MaterialDetailViewModel( Material material )
        {
            Material = material;

            Name = material.Name;
            Description = material.Description;
            QuantityOwned = material.QuantityOwned;
            Size = material.Size;
            Units = material.Units;
            PartNumber = material.PartNumber;
            LifeExpectancy = material.LifeExpectancy;
            LifeExpectancyTimeframe = (Timeframe)material.LifeExpectancyTimeframe;
            imageData = material.ImageBytes;
            Tags.AddRange( material.Tags );

            PreferredRetailerId = material.PreferredRetailerId;
            PreferredRetailer = material.PreferredRetailer;

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

        private int lifeExpectancy;
        public int LifeExpectancy
        {
            get => lifeExpectancy;
            set => SetProperty( ref lifeExpectancy, value );
        }

        private Timeframe lifeExpectancyTimeframe;
        public Timeframe LifeExpectancyTimeframe
        {
            get => lifeExpectancyTimeframe;
            set => SetProperty( ref lifeExpectancyTimeframe, value );
        }

        private byte[] imageData;
        private ImageSource materialImage;
        ImageSource MaterialImage
        {
            get => materialImage ??= imageData != null ? ImageSource.FromStream( () => new MemoryStream( imageData ) ) : default;

            set => SetProperty( ref materialImage, value );
        }

        private int preferredRetailerId;
        public int PreferredRetailerId
        {
            get => preferredRetailerId;
            set => SetProperty( ref preferredRetailerId, value );
        }

        private PreferredRetailer preferredRetailer;
        public PreferredRetailer PreferredRetailer
        {
            get => preferredRetailer;
            set => SetProperty( ref preferredRetailer, value );
        }

        private DateTime createdOn;
        public DateTime CreatedOn
        {
            get => createdOn;
            private set => SetProperty( ref createdOn, value );
        }

        private ObservableRangeCollection<Tag> tags;
        public ObservableRangeCollection<Tag> Tags
        {
            get => tags ??= new ObservableRangeCollection<Tag>();
            set => SetProperty( ref tags, value );
        }

        private ConcurrentDictionary<int, StepViewModel> uniqueSteps = new ConcurrentDictionary<int, StepViewModel>();

        private ObservableRangeCollection<SimpleStepViewModel> steps;
        public ObservableRangeCollection<SimpleStepViewModel> Steps
        {
            get => steps ??= new ObservableRangeCollection<SimpleStepViewModel>();
            set => SetProperty( ref steps, value );
        }

        private ObservableRangeCollection<StepMaterialViewModel> stepMaterials;
        public ObservableRangeCollection<StepMaterialViewModel> StepMaterials
        {
            get => stepMaterials ??= new ObservableRangeCollection<StepMaterialViewModel>();
            set => SetProperty( ref stepMaterials, value );
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
            Name = material.Name;
            Description = material.Description;
            QuantityOwned = material.QuantityOwned;
            Size = material.Size;
            Units = material.Units;
            PartNumber = material.PartNumber;
            LifeExpectancy = material.LifeExpectancy;
            LifeExpectancyTimeframe = (Timeframe)material.LifeExpectancyTimeframe;
            imageData = material.ImageBytes;
            Tags.AddRange( material.Tags );

            PreferredRetailerId = material.PreferredRetailerId;
            PreferredRetailer = material.PreferredRetailer;

            CreatedOn = material.CreatedOn;

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
            List<SimpleStepViewModel> simpleVMs = await StepManager.GetItemRangeAsSimpleViewModel( stepIds );
            Steps.Clear();
            Steps.AddRange( simpleVMs );
        }

        private async Task Init( int id )
        {
            Material = await MaterialManager.GetItemRecursiveAsync( id );
            await Init();
        }

        #endregion

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.MaterialID:
                    string id = HttpUtility.UrlDecode( kvp.Value );
                    if( int.TryParse( id, out int materialId ) )
                    {
                        await Init( materialId );
                    }
                    break;
            }
        }
        #endregion
    }
}
