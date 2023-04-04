using Syncfusion.Windows.Shared;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace CryptoControlCenter.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChromelessWindow
    {
        public string LanguageSwitch { get; private set; } = null;

        public MainWindow()
        {
            App.Current.Resources["AppFontSize"] = Properties.Settings.Default.Zoom;
            App.Current.Resources["AppFontSizeHeaders"] = Properties.Settings.Default.Zoom + 4;
            App.Current.Resources["ButtonSize"] = Properties.Settings.Default.Zoom + 9;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            if (Properties.Settings.Default.IsFullscreen)
            {
                this.WindowState = WindowState.Maximized;
            }
            InitializeComponent();
        }

        public void SetLanguage(string languageCode)
        {
            LanguageSwitch = languageCode;
            Close();
        }


        private void ChromelessWindow_Closed(object sender, EventArgs e)
        {
            MainWindow wnd = sender as MainWindow;
            if (wnd.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.IsFullscreen = true;
            }
            else
            {
                Properties.Settings.Default.IsFullscreen = false;
                Properties.Settings.Default.Height = wnd.Height;
                Properties.Settings.Default.Width = wnd.Width;
            }
            Properties.Settings.Default.Save();

            if (!string.IsNullOrEmpty(wnd.LanguageSwitch))
            {
                wnd.Closed -= ChromelessWindow_Closed;

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(wnd.LanguageSwitch);
                wnd.LanguageSwitch = null;
                wnd = new MainWindow();
                wnd.NavigationMenuListBox.SelectedValue = "Settings";
                wnd.Show();
            }
            else
            {
                App.Current.Shutdown();
            }
        }
    }
}