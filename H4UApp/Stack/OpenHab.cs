using H4UApp.Stack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace H4UApp.Stack
{
    public class OpenHab : IH4U
    {
        public const string OPEN_HAB_URL_SETTING_NAME = "OpenHabUrl";
        public const string OPEN_HAB_REMOTE_URL_SETTING_NAME = "OpenHabRemoteUrl";
        public const string OPEN_HAB_REMOTE_USER_SETTING_NAME = "OpenHabRemoteUser";
        public const string OPEN_HAB_REMOTE_PWD_SETTING_NAME = "OpenHabRemotePwd";

        private const string DEVICES_URL = "rest/things";
        private const string ITEMS_URL = "rest/items";

        public class OpenHabSettings
        {
            public static OpenHabSettings GetSettings()
            {
                string user = (string)ApplicationData.Current.RoamingSettings.Values[OPEN_HAB_REMOTE_USER_SETTING_NAME];
                string pwd = (string)ApplicationData.Current.RoamingSettings.Values[OPEN_HAB_REMOTE_PWD_SETTING_NAME];
                string remoteUrl = (string)ApplicationData.Current.RoamingSettings.Values[OPEN_HAB_REMOTE_URL_SETTING_NAME];

                string url;
                
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    url = "http://openhabianpi:8080";
                else if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd) && !string.IsNullOrEmpty(remoteUrl))
                    url = remoteUrl;
                else
                    url = (string)ApplicationData.Current.RoamingSettings.Values[OPEN_HAB_URL_SETTING_NAME];

                string str = $"{user}:{pwd}";
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                string token = Convert.ToBase64String(bytes);

                return new OpenHabSettings { Url = url, Token = token };
            }

            public string Url { get; set; }
            public string Token { get; set; }
        }

        public ObservableCollection<H4UDevice> Devices { get; set; } = new ObservableCollection<H4UDevice>();

        public ObservableCollection<IGrouping<string, H4UDevice>> DevicesOrderedByLocation { get; set; } = new ObservableCollection<IGrouping<string, H4UDevice>>();

        private static readonly OpenHab instance = new OpenHab();

        private OpenHab()
        {
            ApplicationData.Current.DataChanged += Current_DataChanged;

            Initialize();            
        }

        private void Initialize()
        {            
            if (m_UpdateTask != null)
            {
                m_cancellationTokenSource.Cancel();
                m_UpdateTask.Wait();
            }

            if(m_client != null)
            {
                m_client.CancelPendingRequests();
                m_client.Dispose();
                m_client = null;
            }

            var settings = OpenHabSettings.GetSettings();
            m_client = new HttpClient();
            m_client.BaseAddress = new Uri(settings.Url);
            m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", settings.Token);

            UpdateCaches();

            Devices = GetDevices();
            DevicesOrderedByLocation = new ObservableCollection<IGrouping<string, H4UDevice>>(Devices.GroupBy(e => e.Location).ToList());

            m_UpdateTask = new Task(UpdateTask, m_cancellationToken);
            m_UpdateTask.Start();            
        }

        private void Current_DataChanged(ApplicationData sender, object args)
        {
            Initialize();
        }

        public static OpenHab Instance => instance;

        private HttpClient m_client;

        private Task m_UpdateTask;

        private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

        private CancellationToken m_cancellationToken;

        private ConcurrentDictionary<string, OpenHabThing> m_thingsCache = new ConcurrentDictionary<string, OpenHabThing>();

        private ConcurrentDictionary<string, OpenHabItem> m_itemsCache = new ConcurrentDictionary<string, OpenHabItem>();

        private void UpdateCaches()
        {
            IEnumerable<OpenHabThing> things = null;
            IEnumerable<OpenHabItem> items = null;

            HttpResponseMessage response = null;
            try
            {
                response = m_client.GetAsync(DEVICES_URL).Result;
                things = response.GetObject<IEnumerable<OpenHabThing>>();
            }
            catch (Exception) { }

            try
            {
                response = m_client.GetAsync(ITEMS_URL).Result;
                items = response.GetObject<IEnumerable<OpenHabItem>>();
            }
            catch (Exception) { }

            foreach (var thing in things)
                m_thingsCache.AddOrUpdate(thing.UId, thing, (key, oldValue) => thing);

            foreach (var item in items)
                m_itemsCache.AddOrUpdate(item.Name, item, (key, oldValue) => item);
        }

        private async void UpdateTask()
        {
            while(!m_cancellationToken.IsCancellationRequested)
            {
                UpdateCaches();

                foreach (var device in Devices)
                {
                    foreach (var feature in device.Features)
                    {
                        OpenHabItem existingItem;
                        if (m_itemsCache.TryGetValue(feature.Id, out existingItem))
                        {
                            switch (feature.FeatureType)
                            {
                                case H4UDevcieFeatureType.Switch:
                                    var sw = (H4UDeviceBooleanFeature)feature;
                                    bool currentBool = ((H4UDeviceBooleanFeature)existingItem.TryGetFeature()).Value;
                                    if (sw.Value != currentBool)
                                    {
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            sw.Value = currentBool;
                                        });
                                    }
                                    break;

                                case H4UDevcieFeatureType.Dimmer:
                                case H4UDevcieFeatureType.Numeric:
                                    var num = (H4UDeviceNumericFeature)feature;
                                    double currentDouble = ((H4UDeviceNumericFeature)existingItem.TryGetFeature()).Value;
                                    if (num.Value != currentDouble)
                                    {
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            num.Value = currentDouble;
                                        });
                                    }
                                    break;
                            }
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }

        public ObservableCollection<H4UDevice> GetDevices()
        {
            var devices = new ObservableCollection<H4UDevice>();
           
            foreach (var thing in m_thingsCache)
            {
                var features = new ObservableCollection<H4UDeviceFeature>();
                foreach (var channel in thing.Value.Channels)
                {
                    foreach (var linkedItem in channel.LinkedItems)
                    {
                        OpenHabItem existingItem;
                        if(m_itemsCache.TryGetValue(linkedItem, out existingItem))
                        {
                            H4UDeviceFeature feature = existingItem.TryGetFeature();
                            if (feature != null)
                                features.Add(feature);
                        }            
                    }
                }

                devices.Add(new H4UDevice
                {
                    Id = thing.Value.UId,
                    Name = thing.Value.Label,
                    Location = string.IsNullOrEmpty(thing.Value.Location) ? "Other" : thing.Value.Location,
                    Features = features,
                    IsOnline = thing.Value.StatusInfo.Status == "ONLINE",
                    State = thing.Value.StatusInfo.StatusDetail == "NONE" ? string.Empty : thing.Value.StatusInfo.StatusDetail
                });
            }

            return devices;
        }

        public string GetItemState(string item)
        {
            try
            {
                var response = m_client.GetAsync(ITEMS_URL + "/" + item + "/state").Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.GetString();
                }
            }
            catch (Exception) { }
            return null;
        }

        public void SetItemState(string item, string state)
        {
            try
            {
                var content = new StringContent(state);
                var response = m_client.PostAsync(ITEMS_URL + "/" + item, content).Result;
            }
            catch (Exception) { }
        }
    }

    public class OpenHabThing
    {
        public string UId { get; set; }
        public string Label { get; set; }
        public OpenHabThingStatusInfo StatusInfo { get; set; }            
        public string Location { get; set; }
        public IEnumerable<OpenHabThingChannel> Channels { get; set; }
    }

    public class OpenHabThingStatusInfo
    {
        public string Status { get; set; }
        public string StatusDetail { get; set; }
    }

    public class OpenHabThingChannel
    {
        public IEnumerable<string> LinkedItems { get; set; }
        public string ItemType { get; set; }
        public string Kind { get; set; }
    }

    public class OpenHabItem 
    {       
        public string Link { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> GroupNames { get; set; }
        public OpenHabItemStateDescription StateDescription { get; set; }

        private T GetConcretItem<T>() where T : OpenHabItem, new()
        {
            return new T
            {
                Link = Link,
                Name = Name,
                Type = Type,
                State = State,
                Label = Label,
                Category = Category,
                Tags = Tags,
                GroupNames = GroupNames,
                StateDescription = StateDescription
            };
        }

        public H4UDeviceFeature TryGetFeature()
        {
            H4UDeviceFeature result;

            switch (Type)
            {
                case "Switch":
                    result = new H4UDeviceBooleanFeature(GetConcretItem<OpenHabSwitchItem>())
                    {
                        Id = Name,
                        Name = Label,
                        Category = Category,
                        Tags = new ObservableCollection<string>(Tags),
                        Value = string.Compare(State, "ON", true) == 0,
                        FeatureType = H4UDevcieFeatureType.Switch
                    };

                    if (StateDescription != null && StateDescription.ReadOnly)
                    {
                        result.IsEnabled = false;
                    }

                    return result;

                case "Number":
                    double num = double.NaN;
                    double.TryParse(State, NumberStyles.Number, CultureInfo.InvariantCulture, out num);

                    result = new H4UDeviceNumericFeature(GetConcretItem<OpenHabDimmerItem>())
                    {
                        Id = Name,
                        Name = Label,
                        Category = Category,
                        Tags = new ObservableCollection<string>(Tags),
                        Value = num,
                        FeatureType = H4UDevcieFeatureType.Numeric
                    };

                    if (StateDescription != null && StateDescription.ReadOnly)
                    {
                        result.IsEnabled = false;
                    }

                    return result;

                case "Dimmer":
                    num = double.NaN;
                    double.TryParse(State, NumberStyles.Number, CultureInfo.InvariantCulture, out num);
                    result = new H4UDeviceNumericFeature(GetConcretItem<OpenHabNumberItem>())
                    {
                        Id = Name,
                        Name = Label,
                        Category = Category,
                        Tags = new ObservableCollection<string>(Tags),
                        Value = num,
                        FeatureType = H4UDevcieFeatureType.Dimmer
                    };

                    if (StateDescription != null && StateDescription.ReadOnly)
                    {
                        result.IsEnabled = false;
                    }

                    return result;

                default: return null;
            }
        }
    }

    public class OpenHabSwitchItem : OpenHabItem, IH4UFeature<bool>
    {
        public bool GetValue()
        {
            //State = OpenHab.Instance.GetItemState(Name);
            return string.Compare(State, "ON", true) == 0;
        }

        public void SetValue(bool value)
        {
            State = value ? "ON" : "OFF";
            OpenHab.Instance.SetItemState(Name, State);
        }        
    }

    public class OpenHabDimmerItem : OpenHabItem, IH4UFeature<double>
    {
        public double GetValue()
        {
            //State = OpenHab.Instance.GetItemState(Name);
            double result;
            if (double.TryParse(State, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;

            return double.NaN;
        }

        public void SetValue(double value)
        {
            State = value.ToString(CultureInfo.InvariantCulture);
            OpenHab.Instance.SetItemState(Name, State);
        }
    }

    public class OpenHabNumberItem : OpenHabItem, IH4UFeature<double>
    {
        public double GetValue()
        {
            //State = OpenHab.Instance.GetItemState(Name);
            double result;
            if (double.TryParse(State, NumberStyles.Number, CultureInfo.InvariantCulture , out result))
                return result;

            return double.NaN;
        }

        public void SetValue(double value)
        {
            State = value.ToString(CultureInfo.InvariantCulture);
            OpenHab.Instance.SetItemState(Name, State);
        }        
    }

    public class OpenHabItemStateDescription
    {
        public string Pattern { get; set; }
        public bool ReadOnly { get; set; }
    }    
}
