using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;
namespace Elsys_FiskeApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

   
    public partial class MainWindow : Window
    {
        
        private DispatcherTimer updateTimer;
        public int updateFrequency = 500;
        public static Random rand = new Random(0);
        double deltaTime = 0.1f; // Time between each update.
        public static DispatcherTimer GlobalUpdateTimer { get; private set; } = 
            new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(0.1 * 1000) };// Global timer

        
        public MainWindow()
        {
            InitializeComponent();
            GlobalUpdateTimer.Interval = TimeSpan.FromMilliseconds(deltaTime * 1000);
            GlobalUpdateTimer.Start();


            //skibi

            this.Closed += (s, e) => Application.Current.Shutdown();

            return;



        }



        void Testing(string test)
        {
            //Showcase1.PlotTitle = "";

            //Showcase1.PlotTitle = "Test";

        }

        PlotModel InitializeSoundPlotModel(double maxVariation)
        {
            double startTimeSize = 2f;
            int startSize = (int)(startTimeSize/deltaTime);
            LineSeries SoundPlot = new LineSeries
            {
                Title = "SoundPlot",
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None
            };
            DataPoint nextDataPoint = new DataPoint(0, 1);
            
            for (int i = 0; i < startSize; i++)
            {
                SoundPlot.Points.Add(nextDataPoint);
                nextDataPoint = GenerateNextPoint(nextDataPoint, maxVariation);
            }
            PlotModel SoundModel = new PlotModel();
            SoundModel.Series.Add(SoundPlot);

            return SoundModel;
        }

        
        DataPoint GenerateNextPoint(DataPoint lastPoint, double VariationFactor)
        {
            double NextSoundVal = lastPoint.Y + rand.NextDouble()*VariationFactor;
            return new DataPoint(lastPoint.X + deltaTime, NextSoundVal);
        }


        void UpdateInRealTime(PlotModel model, double VariationFactor)
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