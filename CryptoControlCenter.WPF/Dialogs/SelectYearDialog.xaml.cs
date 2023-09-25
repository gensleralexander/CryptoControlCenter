using Syncfusion.Windows.Shared;
using System;
using System.Windows;

namespace CryptoControlCenter.WPF.Dialogs
{
    public partial class SelectYearDialog : ChromelessWindow
    {
        public SelectYearDialog()
        {
            InitializeComponent();
        }

        public SelectYearDialog(int year)
        {
            InitializeComponent();
            Picker.Value = new DateTime(year, 1,1);
            Picker.MaxDate = new DateTime(year, 1, 1);
            Picker.MinDate = new DateTime(2010, 1, 1);
        }
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public int Year
        {
            get
            {
                if (Picker.Value != null)
                {
                    return ((DateTime)Picker.Value).Year;
                }
                else return DateTime.UtcNow.Year;
            }
        }
    }
}
