﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Maintain_it.Android")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Exit Eleven Enterprises")]
[assembly: AssemblyProduct("Maintain_it.Android")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Needed for Picking photo/video
[assembly: UsesPermission( Android.Manifest.Permission.ReadExternalStorage )]

// Needed for Taking photo/video
[assembly: UsesPermission( Android.Manifest.Permission.WriteExternalStorage )]
[assembly: UsesPermission( Android.Manifest.Permission.Camera )]

// Add these properties if you would like to filter out devices that do not have cameras, or set to false to make them optional
[assembly: UsesFeature( "android.hardware.camera", Required = true )]
[assembly: UsesFeature( "android.hardware.camera.autofocus", Required = true )] 