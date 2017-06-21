using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace H4UApp.Controls
{
    public sealed partial class OnlineIndicator : UserControl
    {
        public OnlineIndicator()
        {
            this.InitializeComponent();
        }

        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        private static void OnIsOnlinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var indicator = (OnlineIndicator)d;
            if ((bool)e.NewValue)
            {
                indicator.tbIndicator.Foreground = (SolidColorBrush)indicator.Resources["Online"];
                indicator.tbIndicator.Text = "\xE930";
            }
            else
            {
                indicator.tbIndicator.Foreground = (SolidColorBrush)indicator.Resources["Offline"];
                indicator.tbIndicator.Text = "\xEA39";
            }                
        }

        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register("IsOnline", typeof(bool), typeof(OnlineIndicator), new PropertyMetadata(false, new PropertyChangedCallback(OnIsOnlinePropertyChanged)));
    }
}
