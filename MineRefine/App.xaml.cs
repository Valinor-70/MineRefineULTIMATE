using Microsoft.UI.Xaml;
using System;

namespace MineRefine
{
    public partial class App : Application
    {
        private Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                m_window = new MainWindow();
                m_window.Title = "Mine & Refine Ultimate Edition - Alpha v1.0.0";

                // Set window properties for optimal gaming experience
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    // Set minimum size for proper UI display
                    appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

                    // Center window on screen
                    var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId,
                        Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
                    if (displayArea != null)
                    {
                        var centeredPosition = appWindow.Position;
                        centeredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                        centeredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                        appWindow.Move(centeredPosition);
                    }
                }

                m_window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App launch error: {ex.Message}");

                // Emergency fallback
                m_window = new MainWindow();
                m_window.Title = "Mine & Refine Ultimate Edition - Emergency Mode";
                m_window.Activate();
            }
        }
    }
}