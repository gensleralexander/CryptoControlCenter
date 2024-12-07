using CryptoControlCenter.Common;
using CryptoControlCenter.WPF.ViewModels;
using Syncfusion.UI.Xaml.SunburstChart;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CryptoControlCenter.WPF.Views
{
    /// <summary>
    /// Interaktionslogik für WalletView.xaml
    /// </summary>
    public partial class WalletView : UserControl
    {
        bool switched;

        public WalletView()
        {
            InitializeComponent();
            CryptoCenter.Instance.IsBusy = false;
            switched = false;
            Task.Run(CryptoCenter.Instance.RefreshBalanceValues);
        }

        private void SwitchLevels_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switched = !switched;
            Sunburst.Levels.Clear();
            if (switched)
            {
                Sunburst.Levels.Add(new SunburstHierarchicalLevel() { GroupMemberPath = "TaxfreeString" });
                Sunburst.Levels.Add(new SunburstHierarchicalLevel() { GroupMemberPath = "Asset" });
                try
                {
                    (Sunburst.Behaviors.First(x => x.GetType() == typeof(SunburstSelectionBehavior)) as SunburstSelectionBehavior).SelectionType = SelectionType.Single;
                }
                catch { }
            }
            else
            {
                Sunburst.Levels.Add(new SunburstHierarchicalLevel() { GroupMemberPath = "Asset" });
                Sunburst.Levels.Add(new SunburstHierarchicalLevel() { GroupMemberPath = "TaxfreeString" });
                try
                {
                    (Sunburst.Behaviors.First(x => x.GetType() == typeof(SunburstSelectionBehavior)) as SunburstSelectionBehavior).SelectionType = SelectionType.Group;
                }
                catch { }
            }
        }

        private void Sunburst_SelectionChanged(object sender, SunburstSelectionChangedEventArgs e)
        {
            var vm = (this.DataContext as WalletViewModel);
            if (vm != null)
            {
                if (e.IsSelected)
                {
                    switch (e.SelectedSegment.Category)
                    {
                        case "Ja":
                        case "Nein":
                        case "Yes":
                        case "No":
                            if (e.SelectedSegment.HasParent)
                            {
                                vm.SelectedCoin = e.SelectedSegment.Parent.Category.ToString();
                            }
                            break;
                        default:
                            vm.SelectedCoin = e.SelectedSegment.Category.ToString();
                            break;
                    }
                }
                else
                {
                    vm.SelectedCoin = null;
                }
            }
        }
    }
}