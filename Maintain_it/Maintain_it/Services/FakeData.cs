﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Maintain_it.Models;

using NUnit.Framework;

using Xamarin.Forms;

using static Xamarin.Essentials.Permissions;

namespace Maintain_it.Services
{
    public class FakeData
    {
        public FakeData()
        {
            maintenanceItem = defaultMaintenanceItem;
            material = defaultMaterial;
            step = defaultStep;
            stepMaterial = defaultStepMaterial;
            shoppingList = defaultShoppingList;
            shoppingListItem = defaultShoppingListItem;
            retailer = defaultRetailer;
            note = defaultNote;
        }

        public MaintenanceItem maintenanceItem { get; set; }
        public Material material { get; set; }
        public Step step { get; set; }
        public StepMaterial stepMaterial { get; set; }
        public ShoppingList shoppingList {  get; set; }
        public ShoppingListMaterial shoppingListItem { get; set; }
        public Retailer retailer { get; set; }
        public Note note { get; set; }

        public static MaintenanceItem defaultMaintenanceItem = new MaintenanceItem ()
        {
            Name = "Default MaintenanceItem",
            NextServiceDate = DateTime.Now.AddDays( 10 ),
            FirstServiceDate = DateTime.Now.AddDays( -10 ),
            PreviousServiceDate = DateTime.Now,
            PreviousServiceCompleted = true,
            IsRecurring = true,
            RecursEvery = 10,
            Frequency = (int)Timeframe.Days,
            TimesServiced = 2,
            NotifyOfNextServiceDate = true,
            Comment = "Default Maintenance Item Comment",

            Steps = new List<Step>()
        };

        public static Material defaultMaterial = new Material()
        {
            Name = "Default",
            StepMaterials = new List<StepMaterial>(),
            RetailerMaterials = new List<Retailer>()
        };

        public static Step defaultStep = new Step()
        {
            Name = "Default Step",
            Description = "Default Step description",
            IsCompleted = false,
            TimeRequired = 1f,
            Timeframe = (int)Timeframe.Hours,
            Notes = new List<Note>(),
            StepMaterials = new List<StepMaterial>()
        };

        public static StepMaterial defaultStepMaterial = new StepMaterial()
        {
            Name="Default"
        };

        public static Retailer defaultRetailer = new Retailer()
        {
            Name = "Default Retailer"
        };

        public static Note defaultNote = new Note()
        {
            Text = "A default note to ensure the database is working correctly",
            ImagePath = "pretend/path/for/testing",
            CreatedOn = DateTime.Now,
            LastUpdated = DateTime.Now
        };

        public static ShoppingListMaterial defaultShoppingListItem = new ShoppingListMaterial()
        {
            Purchased = false
        };

        public static ShoppingList defaultShoppingList = new ShoppingList()
        {
            Name = "Default Shopping List",
            Materials = new List<ShoppingListMaterial>()
        };

    }
}
