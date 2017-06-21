using H4UApp.Stack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace H4UApp.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Settings : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string m_openHabUrl;
        public string OpenHabUrl { get { return m_openHabUrl; } set { m_openHabUrl = value; RaisePropertyChanged(); } }

        string m_openHabRemoteUrl;
        public string OpenHabRemoteUrl { get { return m_openHabRemoteUrl; } set { m_openHabRemoteUrl = value; RaisePropertyChanged(); } }

        string m_openHabRemoteUser;
        public string OpenHabRemoteUser { get { return m_openHabRemoteUser; } set { m_openHabRemoteUser = value; RaisePropertyChanged(); } }

        string m_openHabRemotePwd;
        public string OpenHabRemotePwd { get { return m_openHabRemotePwd; } set { m_openHabRemotePwd = value; RaisePropertyChanged(); } }

        public Settings()
        {
            this.InitializeComponent();
            DataContext = this;
            OpenHabUrl = (string)ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_URL_SETTING_NAME];
            OpenHabRemoteUrl = (string)ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_URL_SETTING_NAME];
            OpenHabRemoteUser = (string)ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_USER_SETTING_NAME];
            OpenHabRemotePwd = (string)ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_PWD_SETTING_NAME];
        }

        private void tbOpenHabUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_URL_SETTING_NAME] = tb.Text;
        }

        private void tbOpenHabRemoteUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_URL_SETTING_NAME] = tb.Text;
        }

        private void tbOpenHabRemoteUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_USER_SETTING_NAME] = tb.Text;
        }

        private void tbOpenHabRemotePwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox tb = sender as PasswordBox;             
            ApplicationData.Current.RoamingSettings.Values[OpenHab.OPEN_HAB_REMOTE_PWD_SETTING_NAME] = tb.Password;
        }
    }
}
