using CryptoControlCenter.Common.Helper;
using CryptoControlCenter.WPF.Helper;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Input;

namespace CryptoControlCenter.WPF.ViewModels
{
    public class SettingsViewModel : AbstractPropertyChanged
    {
        #region Commands
        private ICommand englishCommand;
        private ICommand germanCommand;
        private ICommand dollarCommand;
        private ICommand euroCommand;
        private ICommand exportLogsCommand;

        public ICommand EnglishCommand
        {
            get
            {
                return englishCommand ?? (englishCommand = new RelayCommand(EnglishExecute));
            }
        }
        public ICommand GermanCommand
        {
            get
            {
                return germanCommand ?? (germanCommand = new RelayCommand(GermanExecute));
            }
        }
        public ICommand DollarCommand
        {
            get
            {
                return dollarCommand ?? (dollarCommand = new RelayCommand(DollarExecute));
            }
        }
        public ICommand EuroCommand
        {
            get
            {
                return euroCommand ?? (euroCommand = new RelayCommand(EuroExecute));
            }
        }
        public ICommand ExportLogsCommand
        {
            get
            {
                return exportLogsCommand ?? (exportLogsCommand = new RelayCommand(ExportLogsExecute));
            }
        }
        #endregion
        #region CanExecute
        private bool englishCanExecute = true;
        private bool germanCanExecute = true;
        private bool dollarCanExecute = true;
        private bool euroCanExecute = true;

        public bool EnglishCanExecute
        {
            get
            {
                return englishCanExecute;
            }
            set
            {
                englishCanExecute = value;
                OnPropertyChanged();
            }
        }
        public bool GermanCanExecute
        {
            get
            {
                return germanCanExecute;
            }
            set
            {
                germanCanExecute = value;
                OnPropertyChanged();
            }
        }
        public bool DollarCanExecute
        {
            get
            {
                return dollarCanExecute;
            }
            set
            {
                dollarCanExecute = value;
                OnPropertyChanged();
            }
        }
        public bool EuroCanExecute
        {
            get
            {
                return euroCanExecute;
            }
            set
            {
                euroCanExecute = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public double Zoom
        {
            get
            {
                return Properties.Settings.Default.Zoom;
            }
            set
            {
                Properties.Settings.Default.Zoom = value;
                Properties.Settings.Default.Save();
                App.Current.Resources["AppFontSize"] = value;
                App.Current.Resources["AppFontSizeHeaders"] = value + 4;
                App.Current.Resources["ButtonSize"] = value + 9;
            }
        }

        public SettingsViewModel()
        {
            if (Properties.Settings.Default.CurrencyIsDollar)
            {
                DollarCanExecute = false;
            }
            else EuroCanExecute = false;
            switch (Properties.Settings.Default.LanguageCode)
            {
                case "de":
                    GermanCanExecute = false;
                    break;
                case "en":
                default:
                    EnglishCanExecute = false;
                    break;
            }
        }

        private void EnglishExecute()
        {
            Properties.Settings.Default.LanguageCode = "en";
            Properties.Settings.Default.Save();
            (App.Current.MainWindow as MainWindow).SetLanguage("en");
            EnglishCanExecute = false;
            GermanCanExecute = true;
        }

        private void GermanExecute()
        {
            Properties.Settings.Default.LanguageCode = "de";
            Properties.Settings.Default.Save();
            (App.Current.MainWindow as MainWindow).SetLanguage("de");
            GermanCanExecute = false;
            EnglishCanExecute = true;
        }

        private void DollarExecute()
        {
            Properties.Settings.Default.CurrencyIsDollar = true;
            Properties.Settings.Default.Save();
            DollarCanExecute = false;
            EuroCanExecute = true;
            Common.CryptoCenter.SetCurrency(Common.Enums.Currency.USDollar);
        }

        private void EuroExecute()
        {
            Properties.Settings.Default.CurrencyIsDollar = false;
            Properties.Settings.Default.Save();
            EuroCanExecute = false;
            DollarCanExecute = true;
            Common.CryptoCenter.SetCurrency(Common.Enums.Currency.Euro);
        }

        private async void ExportLogsExecute()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Logs (*.log)|*.log";
            string lastPath = Properties.Settings.Default.LastAccessFilePath;
            if (!string.IsNullOrWhiteSpace(lastPath))
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                saveFileDialog.InitialDirectory = lastPath;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LastAccessFilePath = Path.GetDirectoryName(saveFileDialog.FileName);
                await Common.DocumentGenerator.GenerateLogs(saveFileDialog.FileName);
            }
        }
    }
}
