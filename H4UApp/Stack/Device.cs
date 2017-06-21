using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace H4UApp.Stack
{
    public interface IH4U
    {
        ObservableCollection<H4UDevice> Devices { get; set; }

        ObservableCollection<H4UDevice> GetDevices();
    }

    public interface IH4UFeature<T>
    {    
        T GetValue();

        void SetValue(T value);
    }

    public class H4UDevice : INotifyPropertyChanged
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public bool IsOnline { get; set; }

        public string State { get; set; }

        public ObservableCollection<H4UDeviceFeature> Features { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }

    public abstract class H4UDeviceFeature : INotifyPropertyChanged
    {   
        string m_id;
        public string Id
        {
            get { return m_id; }
            set { m_id = value; RaisePropertyChanged(); }
        }               

        public string Name { get; set; }

        public string Category { get; set; }

        public bool IsEnabled { get; set; } = true;

        public H4UDevcieFeatureType FeatureType { get; set; }

        public ObservableCollection<string> Tags { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }

    public class H4UDeviceBooleanFeature : H4UDeviceFeature
    {
        public IH4UFeature<bool> Source { get; private set; }

        public H4UDeviceBooleanFeature(IH4UFeature<bool> source)
        {
            Source = source;
        }

        bool m_value;
        public bool Value
        {
            get { return m_value; }
            set { m_value = value; RaisePropertyChanged(); }
        }

        public void SetValue(bool value)
        {
            Value = value;
            Source.SetValue(value);
        }
    }

    public class H4UDeviceNumericFeature : H4UDeviceFeature
    {
        public IH4UFeature<double> Source { get; private set; }

        public H4UDeviceNumericFeature(IH4UFeature<double> source)
        {
            Source = source;
        }

        double m_value;
        public double Value
        {
            get { return m_value; }
            set { m_value = value; RaisePropertyChanged(); }
        }

        public void SetValue(double value)
        {
            Value = value;
            Source.SetValue(value);
        }
    }

    public enum H4UDevcieFeatureType
    {
        Switch = 0,
        Dimmer = 2,
        Numeric = 3,
        Color = 4,
    }
}
