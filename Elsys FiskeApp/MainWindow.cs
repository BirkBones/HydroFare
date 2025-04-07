using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;
using Elsys_FiskeApp.View;
using Elsys_FiskeApp.ViewModel;
using OxyPlot.Axes;
namespace Elsys_FiskeApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

   
    public partial class MainWindow : Window
    {
        
        private DispatcherTimer updateTimer;
        public static Random rand = new Random(0);
        double deltaTime = 0.1f; // Time between each update in seconds.
        double patchSize = 1;
        double timePassingFactor = 100000;
        public static DispatcherTimer GlobalUpdateTimer = new DispatcherTimer();
        List<DataPoint> points = new List<DataPoint>();

        public MainWindow()
        {
            InitializeComponent();
            GlobalUpdateTimer.Interval = TimeSpan.FromMilliseconds(patchSize * deltaTime / timePassingFactor * 1000);
            GlobalUpdateTimer.Start();
            var vm = (SinglePlotViewModel)skratta.DataContext;
            DataPoint startpoint = new DataPoint(3, 100) { };
            vm.setOptions("test", new LinearAxis() {Position = AxisPosition.Bottom  }, "x", "y");
            points.Add(startpoint);

            GlobalUpdateTimer.Tick += (sender, e) =>
            {
                var temp = GenerateRandomPoints(points.Last(), 2);
                points = Enumerable.Concat(points, temp).ToList();
                vm.addPoints(temp);
            };
            return;
        }



        
        DataPoint GenerateNextPoint(DataPoint lastPoint, double VariationFactor)
        {
            double NextSoundVal = lastPoint.Y + (rand.NextDouble() - 0.5)*VariationFactor;
            return new DataPoint(lastPoint.X + deltaTime, NextSoundVal);
        }

        List<DataPoint> GenerateRandomPoints(DataPoint LastPoint, double variationFactor)
        {
            List<DataPoint> random = new List<DataPoint>();
            for (int i = 0; i< patchSize; i++)
            {
                random.Add(GenerateNextPoint(LastPoint, variationFactor));
                LastPoint = random.Last();
            }
            return random;
        }

        }

    }

