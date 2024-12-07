using CryptoControlCenter.Common;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows.Controls;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für WhatIfView.xaml
    /// </summary>
    public partial class WhatIfView : UserControl
    {

        public WhatIfView()
        {
            InitializeComponent();
            CryptoCenter.Instance.IsBusy = false;
        }

        private void WhatIfList_CurrentCellBeginEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs e)
        {
            
        }

        private void WhatIfList_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {

        }
    }
}
