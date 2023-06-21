using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;

using Maintain_it.Services;

using static Maintain_it.Helpers.Config;

using AndroidApp = Android.App.Application;

[assembly: Dependency( typeof( Maintain_it.Droid.ServiceItemManagement.AndroidServiceItemManager ) )]
namespace Maintain_it.Droid.ServiceItemManagement
{
    public class AndroidServiceItemManager : IDatabaseItemManager
    {

    #region Constants
        const string channelId = "androidServiceItemManager";
        const string channelName = "AndroidServiceItemManager";
        const string channelDescription = "Android Service for managing items";
    #endregion Constants

    #region Fields
        bool channelInitialized = false;
        int mainActivityPendingIntent = 0;
        int serviceItemPendingIntent = 0;



    #endregion Fields

    #region Initialization
    #endregion Initialization

        public event EventHandler RequestReceived;

        public void IndexItems()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}