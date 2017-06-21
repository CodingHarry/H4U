using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace H4UApp.Stack
{    
    public class H4UDeviceFeatureTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var deviceFeature = item as H4UDeviceFeature;
            switch (deviceFeature.FeatureType)
            {
                case H4UDevcieFeatureType.Switch:
                    if (deviceFeature.IsEnabled)
                        return SwitchTemplate;
                    else
                        return SwitchIndicatorTemplate;

                case H4UDevcieFeatureType.Dimmer:
                    return DimmmerTemplate;

                case H4UDevcieFeatureType.Numeric:
                    return NumericTemplate;

                default:
                    return TextTemplate;
            }
        }

        public DataTemplate SwitchTemplate { get; set; }
        public DataTemplate SwitchIndicatorTemplate { get; set; }
        public DataTemplate DimmmerTemplate { get; set; }
        public DataTemplate NumericTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
    }    
}
