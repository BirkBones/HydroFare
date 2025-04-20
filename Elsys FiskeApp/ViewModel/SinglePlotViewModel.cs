using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace Elsys_FiskeApp.ViewModel
{
    public class SinglePlotViewModel : ViewModelBase // Tror det beste i grunnen er å bare blotte _plottingmodel for omverdenen og endre på den fra utsiden direkte, ingen poeng i å skrive nye hjelpefunksjoner som gjør nøyaktig det samme som allerede er definert i oxyplot.
    {
        private PlotModel _plottingModel;

        public PlotModel plottingModel
        {
            get { return _plottingModel; }
            set { _plottingModel = value; OnPropertyChanged(); }
        }

        private LineSeries _plottedData;

        public LineSeries plottedData
        {
            get { return _plottedData; }
            set { _plottedData = value; OnPropertyChanged(); }
        }
        private double _Height = 200;

        public double Height
        {
            get { return _Height; }
            set { _Height = value; OnPropertyChanged(); }
        }

        private double _Width = 400;

        public double Width
        {
            get { return _Width; }
            set { _Width = value; OnPropertyChanged(); }
        }

        private Button _ResetButton;
        public Button ResetButton
        {
            get { return _ResetButton; }
            set { _ResetButton = value; OnPropertyChanged(); }

        }


        // How much of the graph will be shown at a time. For the sake of showing this at a stand, assume it to be one minute.
        double expectedRelevantTimeArea = 60;

        public void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            plottingModel.ResetAllAxes();
            plottingModel.InvalidatePlot(true);

        }
        public SinglePlotViewModel()
        {
            plottingModel = new PlotModel() { DefaultFont = "Times New Roman", TitleFont = "Times New Roman" };
            plottedData = new LineSeries { XAxisKey = "xAxis", YAxisKey = "yAxis" };
            plottingModel.Series.Add(plottedData);
        }
        public SinglePlotViewModel(string plotTitle, Axis xAxisType, string xAxesName, string yAxesName)
        {
            plottingModel = new PlotModel();
            // Initializing things for the first time

            setOptions(plotTitle, xAxisType, xAxesName, yAxesName);
            plottedData = new LineSeries { XAxisKey = "xAxis", YAxisKey = "yAxis" };
            plottingModel.Series.Add(plottedData);

        }
        public void setOptions(string title, Axis xAxisType, string xAxesName, string yAxesName)
        {
            plottingModel.Title = title;
            plottingModel.Axes.Clear();

            xAxisType.Title = xAxesName;
            plottingModel.Axes.Add(xAxisType);

            var yAxis = new LinearAxis { Position = AxisPosition.Left, Key = "yAxis", Title = yAxesName };
            plottingModel.Axes.Add(yAxis);
 
        }

        public void addPoints(List<DataPoint> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                plottedData.Points.Add(points.ElementAt(i));

            }
            plottingModel.InvalidatePlot(true);
            updateAxesLimits();
        }
        public void addPoints(Queue<DataPoint> points)
        {
            while (points.Count > 0) 
            {
                plottedData.Points.Add(points.Dequeue());

            }
            plottingModel.InvalidatePlot(true);
            updateAxesLimits();
        }
        public void SetPoints(List<DataPoint> points)
        {
            plottedData.Points.Clear();
            addPoints(points);

        }

        private void updateAxesLimits()
        {
            if (plottedData.Points.Count > 0)
            {
                double Ymax = plottedData.Points.Max(point => point.Y); // Check which dimensions the plot at least has to be within
                double Ymin = plottedData.Points.Min(point => point.Y);
                double Xmax = plottedData.Points.Max(point => point.X);
                double Xmin = plottedData.Points.Min(point => point.X);
                plottingModel.Axes[1].Minimum = Ymin;
                plottingModel.Axes[1].Maximum = Ymax;
                if (plottingModel.Axes[0] is LinearAxis || plottingModel.Axes[0] is TimeSpanAxis) // the only difference between how the plot should be scaled, is whether or not we are dealing in real-time or the frequency domain on the x-axes
                {
                    // For the moment this scales up gradually when the program is started, then has constant size when it has achieved the correct size for the xAxes.
                    plottingModel.Axes[0].Minimum = Math.Max(Xmax - expectedRelevantTimeArea * 1.05, Xmin);
                    plottingModel.Axes[0].Maximum = Xmax + expectedRelevantTimeArea * 0.05;


                }
                if (plottingModel.Axes[0] is LogarithmicAxis)
                {
                    plottingModel.Axes[0].Minimum = Xmin - (Xmax - Xmin) * 0.1;
                    plottingModel.Axes[0].Maximum = Xmax + (Xmax - Xmin) * 0.1;

                }
                plottingModel.InvalidatePlot(true);
            }
        }

    }
}
