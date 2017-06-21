using H4UApp.Stack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class Slider : UserControl
    {
        private Stopwatch m_stopWatch;
        private bool m_valueNeedToBeSet;
        private double m_valueToSet;

        public Slider()
        {
            this.InitializeComponent();
            m_valueNeedToBeSet = false;
            m_stopWatch = Stopwatch.StartNew();

            Task t = new Task(KeepValueConsistent);
            t.Start();
        }

        private async void KeepValueConsistent()
        {
            while (true)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (m_valueNeedToBeSet)
                    {
                        SetValue();
                    }
                });

                await Task.Delay(2000);
            }
        }
        
        public H4UDeviceNumericFeature Feature
        {
            get { return (H4UDeviceNumericFeature)GetValue(FeatureProperty); }
            set { SetValue(FeatureProperty, value); }
        }
        
        public static readonly DependencyProperty FeatureProperty =
            DependencyProperty.Register("Feature", typeof(H4UDeviceBooleanFeature), typeof(Slider), new PropertyMetadata(0));


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(Slider), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnLabelPropertyChanged)));

        private static void OnLabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Slider)d;
            sw.tbLabel.Text = (string)e.NewValue;
        }

        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(Slider), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnCategoryPropertyChanged)));

        private static void OnCategoryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Slider)d;
            sw.ciCategory.Category = (string)e.NewValue;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Slider), new PropertyMetadata(0, new PropertyChangedCallback(OnValuePropertyChanged)));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sw = (Slider)d;
            sw.slValue.Value = (double)e.NewValue;
            sw.tbValue.Text = e.NewValue.ToString();
        }

        private void slValue_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_valueNeedToBeSet = true;
            m_valueToSet = e.NewValue;

            if (m_stopWatch.ElapsedMilliseconds > 2000)
            {
                SetValue();
            }
        }

        private void SetValue()
        {
            Feature.SetValue(m_valueToSet);
            m_stopWatch.Restart();
            m_valueNeedToBeSet = false;
        }
    }
}
