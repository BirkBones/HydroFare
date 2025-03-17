
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Series;
using OxyPlot;

namespace Elsys_FiskeApp.PlotAssets
{
    public partial class PlottingModule() : System.Windows.Window
    {
        public double deltaTime = 0.1f;

        protected Random rand = new Random(0);
        public LineSeries InitializeSoundPlotLineSeries(double maxVariation)
        {
            double startTimeSize = 2f;
            int startSize = (int)(startTimeSize / deltaTime);
            LineSeries SoundLineSeries = new LineSeries
            {
                Title = "RawData",
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None
            };
            DataPoint nextDataPoint = new DataPoint(0, MainWindow.rand.NextDouble());

            for (int i = 0; i < startSize; i++)
            {
                SoundLineSeries.Points.Add(nextDataPoint);
                nextDataPoint = GenerateNextPoint(nextDataPoint, maxVariation);
            }
           

            return SoundLineSeries;
        }


        protected DataPoint GenerateNextPoint(DataPoint lastPoint, double VariationFactor)
        {
            double NextSoundVal = lastPoint.Y + (MainWindow.rand.NextDouble()-0.5) * VariationFactor;
            return new DataPoint(lastPoint.X + deltaTime, NextSoundVal);
        }


        public void UpdateInRealTime(PlotModel model, double VariationFactor)
        {
            LineSeries lineSeries = model.Series.First() as LineSeries;
            DataPoint nextPoint = lineSeries.Points.Last();
            for (int i = 0; i < 10; i++)
            {
                nextPoint = GenerateNextPoint(nextPoint, VariationFactor);
                lineSeries.Points.Add(nextPoint);
            }
            model.InvalidatePlot(true);
        }
    }
}
