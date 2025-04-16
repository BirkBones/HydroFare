using Elsys_FiskeApp.ViewModel;
using System.Windows;
using System.Windows.Controls;
namespace Elsys_FiskeApp.View
{
    /// <summary>
    /// Interaction logic for SingleMerdView.xaml
    /// </summary>
    public partial class SingleMerdView : UserControl
    {
        public SingleMerdView()
        {

            InitializeComponent();
            DataContext = new SingleMerdViewModel();
        }
    }
}
