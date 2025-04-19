using Elsys_FiskeApp.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Elsys_FiskeApp.View
{
    /// <summary>
    /// Interaction logic for SinglePlotView.xaml
    /// </summary>
    public partial class SinglePlotView : UserControl
    {

        public SinglePlotView()
        {
            InitializeComponent();
            DataContext = new SinglePlotViewModel();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Example: detect if Escape is pressed
            if (e.Key == Key.Escape)
            {
                if (DataContext is SinglePlotViewModel vm)
                {
                    vm.HandlePreviewKeyDown(sender, e);
                }
            }
        }
    }
}
