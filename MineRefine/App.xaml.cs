using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace MineRefine
{
    public partial class App : Application
    {
        private Window? m_window;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-07 08:34:00";

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Start with Settings/Player Setup window first
            m_window = new SettingsWindow();
            m_window.Activate();
        }
    }
}