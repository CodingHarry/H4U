using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls
{
    public sealed partial class ColorPicker : UserControl
    {
        // 0x00, 0x40, 0x80, 0xC0, 0xFF
        static RGBWColor[] _RGBWColors = new [] {
                       
            new RGBWColor(0x00, 0x00, 0xFF, 0x40, 0x40),   
            new RGBWColor(0x00, 0x00, 0xFF, 0x40, 0xFF),
            new RGBWColor(0x00, 0x00, 0xFF, 0x00, 0xFF),
            new RGBWColor(0x00, 0x00, 0xC0, 0x00, 0xFF),
            new RGBWColor(0x00, 0x00, 0x80, 0x00, 0xFF),

            new RGBWColor(0x00, 0x00, 0x80, 0x80, 0xFF),
            new RGBWColor(0x00, 0x00, 0x40, 0x40, 0xFF), 
            new RGBWColor(0x00, 0x00, 0x00, 0x40, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0x00, 0xFF),
            new RGBWColor(0x00, 0x00, 0x40, 0x00, 0xFF),

            new RGBWColor(0x00, 0x00, 0x40, 0xFF, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0xC0, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0xA0, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0x80, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0x60, 0xFF),            

            new RGBWColor(0x00, 0x00, 0x00, 0xFF, 0xFF),
            new RGBWColor(0x00, 0x00, 0x00, 0xFF, 0xC0),
            new RGBWColor(0x00, 0x00, 0x00, 0xFF, 0x80),
            new RGBWColor(0x00, 0x00, 0x00, 0xFF, 0x40),
            new RGBWColor(0x00, 0x00, 0x40, 0xFF, 0x40),

            new RGBWColor(0x00, 0x00, 0xA0, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0x80, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0x60, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0x40, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0x00, 0xFF, 0x00),

            new RGBWColor(0x00, 0x00, 0xC0, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0xFF, 0xC0, 0x00),
            new RGBWColor(0x00, 0x00, 0xFF, 0x80, 0x00),   
            new RGBWColor(0x00, 0x00, 0xFF, 0x40, 0x00),
            new RGBWColor(0x00, 0x00, 0xFF, 0x00, 0x00),

            new RGBWColor(0x00, 0x00, 0xFF, 0xFF, 0x00),
            new RGBWColor(0x00, 0x00, 0xFF, 0xFF, 0x40),
            new RGBWColor(0xFF, 0x00, 0x00, 0x00, 0x00, Color.FromArgb(0xFF, 0xFF, 0xFF, 0x80)),
            new RGBWColor(0xFF, 0xFF, 0x00, 0x00, 0x00, Color.FromArgb(0xFF, 0xFF, 0xFF, 0xC0)),
            new RGBWColor(0x00, 0xFF, 0x00, 0x00, 0x00, Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),


        };
                
        public delegate void ColorPickedEventHandler(object sender, ColorPickerColorPickedEventArgs e);
        public event ColorPickedEventHandler ColorPicked;

        private void RaiseColorPicked(RGBWColor rgbwColor)
        {
            ColorPicked?.Invoke(this, new ColorPickerColorPickedEventArgs(rgbwColor));
        }

        public ColorPicker()
        {
            this.InitializeComponent();
            this.DataContext = this;

            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());
            gdColors.RowDefinitions.Add(new RowDefinition());

            gdColors.ColumnDefinitions.Add(new ColumnDefinition());
            gdColors.ColumnDefinitions.Add(new ColumnDefinition());
            gdColors.ColumnDefinitions.Add(new ColumnDefinition());
            gdColors.ColumnDefinitions.Add(new ColumnDefinition());
            gdColors.ColumnDefinitions.Add(new ColumnDefinition());

            gdColors.Children.Clear();

            var count = 0;

            foreach (var rgbwColor in _RGBWColors)
            {
                var row = count / gdColors.ColumnDefinitions.Count;
                var column = count % gdColors.ColumnDefinitions.Count;

                Button btn = new Button();
                btn.Tag = rgbwColor;
                btn.Background = new SolidColorBrush(rgbwColor.GuiColor);
                btn.VerticalAlignment = VerticalAlignment.Stretch;
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.Click += btn_Click;
                gdColors.Children.Add(btn);
                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, column);

                count++;
            }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(ColorPicker),
            new PropertyMetadata("", LabelPropertyChanged));

        private static void LabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string label = e.NewValue as string;
            (d as ColorPicker).tbColorPicker.Text = label;
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            var rgbwColor = btn.Tag as RGBWColor;

            RaiseColorPicked(rgbwColor);
        }

        private void gdColors_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
