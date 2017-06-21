using H4UApp.Stack;
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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace H4UApp
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            title.Text = "Devices";
            contentFrame.ContentTransitions = new TransitionCollection();
            contentFrame.ContentTransitions.Add(new NavigationThemeTransition());
            contentFrame.Navigate(typeof(Views.FeaturePage), null, new EntranceNavigationTransitionInfo());            
        }

        private bool m_navMenuHidden = true;
             
        private void ShowHideNavMenu()
        {
            if (m_navMenuHidden)
            {
                ShowNavMenu();             
            }
            else
            {
                HideNavMenu();
            }
        }

        private void ShowNavMenu()
        {
            Storyboard myStoryboard = (Storyboard)this.Resources["showNav"];
            myStoryboard.Begin();
            m_navMenuHidden = false;
        }

        private void HideNavMenu()
        {
            Storyboard myStoryboard = (Storyboard)this.Resources["hideNav"];
            myStoryboard.Begin();
            m_navMenuHidden = true;
        }

        private void navButton_Click(object sender, EventArgs e)
        {
            ShowHideNavMenu();
        }

        private void navDevices_Click(object sender, EventArgs e)
        {
            HideNavMenu();
            title.Text = "Devices";
            contentFrame.Navigate(typeof(Views.FeaturePage));
        }

        private void navSettings_Click(object sender, EventArgs e)
        {
            HideNavMenu();
            title.Text = "Settings";
            contentFrame.Navigate(typeof(Views.Settings));
        }        
    }        
}
