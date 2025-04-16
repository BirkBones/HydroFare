using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elsys_FiskeApp.ViewModel
{
    public class SingleMerdViewModel : ViewModelBase
    {
        public SinglePlotViewModel TrendPlotViewModel { get; set; }

        public SinglePlotViewModel RawPlotViewModel { get; set; }

        public SinglePlotViewModel FourierPlotViewModel { get; set; }

        public SingleMerdViewModel() {
            TrendPlotViewModel = new SinglePlotViewModel();
            RawPlotViewModel = new SinglePlotViewModel();
            FourierPlotViewModel = new SinglePlotViewModel();
            TrendPlotViewModel.setOptions("Trend", new LinearAxis(), "Time / s", "Events per second");
            RawPlotViewModel.setOptions("Raw Signal", new LinearAxis(), "Time / s", "Volume ");
            FourierPlotViewModel.setOptions("Fourier Transform", new LogarithmicAxis(), "Frequency / f", "Fourier");
            
        }




    }
}
