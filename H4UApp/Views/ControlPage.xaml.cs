using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using H4UApp.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace H4UApp.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ControlPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        HttpClient _Client;

        public ControlPage()
        {
            this.InitializeComponent();

            this.DataContext = this;

            _Client = new HttpClient();

            InitDeviceStates();
        }
        
        private bool _LightSwitch;

        public bool LightSwitch
        {
            get { return _LightSwitch; }
            set { _LightSwitch = value; RaisePropertyChanged("LightSwitch"); }
        }

        private int _BackgroundLightLevel;

        public int BackgroundLightLevel
        {
            get { return _BackgroundLightLevel; }
            set { _BackgroundLightLevel = value; RaisePropertyChanged("BackgroundLightLevel"); }
        }

        private int _LightLevel;

        public int LightLevel
        {
            get { return _LightLevel; }
            set { _LightLevel = value; RaisePropertyChanged("LightLevel"); }
        }

        private double _ThermostatSetPoint;

        public double ThermostatSetPoint
        {
            get { return _ThermostatSetPoint; }
            set { _ThermostatSetPoint = value; RaisePropertyChanged("ThermostatSetPoint"); }
        }

        private async void InitDeviceStates()
        {

            HttpResponseMessage response;
            //response = await _Client.GetAsync("http://10.0.0.10:8083/ZWaveAPI/Run/devices[19].commandClasses[0x25].data.level");

            //if (response.IsSuccessStatusCode && response.Content != null)
            //{
            //    response.EnsureSuccessStatusCode();
            //    byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            //    string json = Encoding.UTF8.GetString(bytes);
            //    var data = JsonConvert.DeserializeObject<DeviceData>(json);
            //    LightSwitch = Convert.ToBoolean(data.Value);
            //}

            //response = await _Client.GetAsync("http://10.0.0.10:8083/ZWaveAPI/Run/devices[14].instances[0].commandClasses[0x26].data.level");

            //if (response.IsSuccessStatusCode && response.Content != null)
            //{
            //    response.EnsureSuccessStatusCode();
            //    byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            //    string json = Encoding.UTF8.GetString(bytes);
            //    var data = JsonConvert.DeserializeObject<DeviceData>(json);
            //    BackgroundLightLevel = Convert.ToInt16(data.Value);
            //}                

            response = await _Client.GetAsync("http://openhabianpi:8080/rest/items/ZWaveNode33FGD212Dimmer2_Dimmer");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                response.EnsureSuccessStatusCode();
                byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
                string json = Encoding.UTF8.GetString(bytes);
                var data = JsonConvert.DeserializeObject<DeviceData>(json);
                LightLevel = Convert.ToInt16(data.Value);
            }

            //response = await _Client.GetAsync("http://10.0.0.10:8083/ZWaveAPI/Run/devices[20].instances[0].commandClasses[0x43].data[1].setVal.value");

            //if (response.IsSuccessStatusCode && response.Content != null)
            //{
            //    response.EnsureSuccessStatusCode();
            //    byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            //    string value = Encoding.UTF8.GetString(bytes);
            //    ThermostatSetPoint = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            //}
            
        }

        public class DeviceData
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        private async void tsLicht_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch sw = sender as ToggleSwitch;

            if (sw.IsOn)
            {
                var response = await _Client.GetAsync("http://10.0.0.10:8083/ZWaveAPI/Run/devices[19].instances[0].commandClasses[0x25].Set(1)");
            }
            else
            {
                var response = await _Client.GetAsync("http://10.0.0.10:8083/ZWaveAPI/Run/devices[19].instances[0].commandClasses[0x25].Set(0)");
            }
        }

        private async void cpBackgroundLight_ColorPicked(object sender, ColorPickerColorPickedEventArgs e)
        {
            var response = await _Client.GetAsync($"http://10.0.0.10:8083/ZWaveAPI/Run/devices[14].instances[0].commandClasses[51].SetMultiple([0,1,2,3,4],[{e.RGBWColor.Warmwhite},{e.RGBWColor.Coldwhite},{e.RGBWColor.Red},{e.RGBWColor.Green},{e.RGBWColor.Blue}],0x00)");
        }

        private async void slLight_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Slider sl = sender as Slider;
            StringContent content = new StringContent(sl.Value.ToString());
            var response = await _Client.PostAsync($"http://openhabianpi:8080/rest/items/ZWaveNode33FGD212Dimmer2_Dimmer", content);
        }

        private async void slBackgroundLight_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Slider sl = sender as Slider;
            var response = await _Client.GetAsync($"http://10.0.0.10:8083/ZWaveAPI/Run/devices[14].instances[0].commandClasses[0x26].Set({sl.Value})");
        }

        private async void slThermostatSetPoint_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Slider sl = sender as Slider;
            string value = Convert.ToString(sl.Value, System.Globalization.CultureInfo.InvariantCulture);
            var response = await _Client.GetAsync($"http://10.0.0.10:8083/ZWaveAPI/Run/devices[20].instances[0].commandClasses[0x43].Set(1,{value})");
        }
    }
}
