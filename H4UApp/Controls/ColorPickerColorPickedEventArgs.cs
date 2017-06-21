using System;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls
{
    public class ColorPickerColorPickedEventArgs : EventArgs
    {
        public ColorPickerColorPickedEventArgs(RGBWColor rgbwColor)
        {
            RGBWColor = rgbwColor;
        }

        public RGBWColor RGBWColor { get; set; }
    }
}
