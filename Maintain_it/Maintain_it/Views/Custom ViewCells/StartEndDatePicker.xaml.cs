﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it.Views.Custom_ViewCells
{
    [XamlCompilation( XamlCompilationOptions.Compile )]
    public partial class StartEndDatePicker : ViewCell
    {
        public StartEndDatePicker()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create( "Text", typeof( string ), typeof( ViewCell ), null, defaultBindingMode: BindingMode.OneWay );

        public string Text
        {
            get => GetValue( TextProperty ) as string;
            set => SetValue( TextProperty, value );
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create( "TextColor", typeof( Color ), typeof( ViewCell ), null, defaultBindingMode: BindingMode.OneWay );

        public Color TextColor
        {
            get => (Color)GetValue( TextColorProperty );
            set => SetValue( TextColorProperty, value );
        }

        public static readonly BindableProperty StartDateProperty = BindableProperty.Create( "StartDate", typeof( DateTime ), typeof( ViewCell ), DateTime.UtcNow.ToLocalTime(), defaultBindingMode: BindingMode.TwoWay );

        public DateTime StartDate
        {
            get => (DateTime)GetValue( StartDateProperty );
            set => SetValue( StartDateProperty, value );
        }

        public static readonly BindableProperty EndDateProperty = BindableProperty.Create( "EndDate", typeof( DateTime ), typeof( ViewCell ), DateTime.UtcNow.AddDays( 7 ).ToLocalTime(), defaultBindingMode: BindingMode.TwoWay );

        public DateTime EndDate
        {
            get => (DateTime)GetValue( EndDateProperty );
            set => SetValue( EndDateProperty, value );
        }

        public static readonly BindableProperty IsDatePickerEnabledProperty = BindableProperty.Create( "IsDatePickerEnabled", typeof( bool ), typeof( ViewCell ), true, BindingMode.TwoWay );

        public bool IsDatePickerEnabled
        {
            get => (bool)GetValue( IsDatePickerEnabledProperty );
            set => SetValue( IsDatePickerEnabledProperty, value );
        }

    }
}