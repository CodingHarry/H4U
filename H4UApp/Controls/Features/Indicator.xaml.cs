using H4UApp.Stack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls.Features
{
    public sealed partial class Indicator : UserControl
    {
        public Indicator()
        {
            this.InitializeComponent();
        }
        
        public H4UDeviceBooleanFeature Feature
        {
            get { return (H4UDeviceBooleanFeature)GetValue(FeatureProperty); }
            set { SetValue(FeatureProperty, value); }
        }
        
        public static readonly DependencyProperty FeatureProperty =
            DependencyProperty.Register("Feature", typeof(H4UDeviceBooleanFeature), typeof(Indicator), new PropertyMetadata(0));


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(Indicator), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnLabelPropertyChanged)));

        private static void OnLabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Indicator)d;
            sw.tbLabel.Text = (string)e.NewValue;
        }

        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(Indicator), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnCategoryPropertyChanged)));

        private static void OnCategoryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Indicator)d;
            sw.ciCategory.Category = (string)e.NewValue;
        }

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }
        
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(Indicator), new PropertyMetadata(false, new PropertyChangedCallback(OnIsOnPropertyChanged)));

        private static void OnIsOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Indicator)d;
            sw.rbIndicator.IsChecked = (bool)e.NewValue;
        }
    }
}
