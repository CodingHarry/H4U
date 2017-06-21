using Windows.UI;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls
{

    public class RGBWColor
    {
        public RGBWColor(byte ww, byte cw, byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
            Warmwhite = ww;
            Coldwhite = cw;

            GuiColor = Color.FromArgb(0xFF, r, g, b);
        }

        public RGBWColor(byte ww, byte cw, byte r, byte g, byte b, Color guiColor)
        {
            Red = r;
            Green = g;
            Blue = b;
            Warmwhite = ww;
            Coldwhite = cw;

            GuiColor = guiColor;
        }

        public Color GuiColor { get; set; }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Warmwhite { get; set; }
        public byte Coldwhite { get; set; }
    }
}
