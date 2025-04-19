using Elsys_FiskeApp.Model;
using OxyPlot;
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

        public string merdName { get; private set; }

        //private string placeHolderText;

        //public string PlaceholderText
        //{
        //    get { return placeHolderText; }
        //    set { placeHolderText = value; }
        //}


        public SingleMerdViewModel() {
            merdName = "Merd1";
            // Note to self; feilen ligger i tilegningen av axis, siden logarithmic color axis kun blir lagt til som en ekstra akse hvis den inputtes.
            TrendPlotViewModel = new SinglePlotViewModel("Trend", new LogarithmicAxis() { Position = AxisPosition.Bottom, Key = "xAxis" }
            , "Time / s", "Events per second");
            RawPlotViewModel = new SinglePlotViewModel("Raw Signal", new LinearAxis() { Position = AxisPosition.Bottom, Key = "xAxis" }
            , "Time / s", "Volume");
            FourierPlotViewModel = new SinglePlotViewModel("Fourier Transform", 
                new LogarithmicAxis() { Position = AxisPosition.Bottom, Key = "xAxis" }
                , "Frequency / f", "Fourier");

            DataHolder.Instance.GlobalUpdateTimer.Tick += (sender, e) => updatePlots();

        }

        void updatePlots()
        {
            var plottingData = DataHolder.Instance.newProcessedData[merdName].ToList();

            updateSpecificPlot(plottingData, TrendPlotViewModel, data => data.TreatedSignal);
            updateSpecificPlot(plottingData, FourierPlotViewModel, data => data.FourierData);
            updateSpecificPlot(plottingData, RawPlotViewModel, data => data.RawData);

            DataHolder.Instance.newProcessedData[merdName].Clear();

        }
        void updateSpecificPlot(List<updateData> updateDataList, SinglePlotViewModel plotModel, Func<updateData, float> valueSelector)
        {
            List<DataPoint> points = updateDataList
                .Select(data => new DataPoint(data.Time, valueSelector(data)))
                .ToList();

            plotModel.addPoints(points);
        }




    }
}
