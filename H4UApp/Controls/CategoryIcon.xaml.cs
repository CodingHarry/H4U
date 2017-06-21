using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls
{
    public sealed partial class CategoryIcon : UserControl
    {
        public CategoryIcon()
        {
            this.InitializeComponent();
        }

        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        private static void OnIsOnlinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var category = (string)e.NewValue;
            switch(category)
            {
                case "Energy":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE945";
                    break;

                case "Switch":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE7E8";
                    break;

                case "Temperature":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE9CA";
                    break;

                case "Light":
                case "DimmableLight":
                    ((CategoryIcon)d).tbIndicator.Text = "\xEA80";
                    break;

                case "Color":
                case "ColorLight":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE790";
                    break;

                case "Luminosity":
                case "Brightness":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE706";
                    break;
                    
                case "Contrast":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE793";
                    break;

                case "Battery":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE856";
                    break;

                case "TV":
                case "Television":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE7F4";
                    break;

                case "System":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE770";
                    break;

                case "Alarm":
                    ((CategoryIcon)d).tbIndicator.Text = "\xEDAC";
                    break;
                    
                case "Remote":
                    ((CategoryIcon)d).tbIndicator.Text = "\xE83B";
                    break;

                default:
                    ((CategoryIcon)d).tbIndicator.Text = "\xE11B";
                    break;
            }             
        }

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(CategoryIcon), new PropertyMetadata("", new PropertyChangedCallback(OnIsOnlinePropertyChanged)));
    }
}
