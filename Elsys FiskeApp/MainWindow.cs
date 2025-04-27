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
using Elsys_FiskeApp.Model;
using MQTTnet.Protocol;
using System.Windows.Controls;
using CsvHelper;

namespace Elsys_FiskeApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        public static DispatcherTimer GlobalUpdateTimer = new DispatcherTimer();
        DataHolder dataHolder = new DataHolder();
        MerdsHandler merdsHandler;
        public SingleMerdViewModel merdViewModel;
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();

            return;
        }
        async void InitializeAsync()
        {
            await Task.Run(() =>
            {
                merdsHandler = new MerdsHandler();
            });

            var merds = new ObservableCollection<SingleMerdViewModel>();
            foreach (var merd in merdsHandler.Merds)
            {
                // Initialize main views (horizontal)
                var merdViewModel = new SingleMerdViewModel(merd.Value);
                var merdView = new SingleMerdView();
                merdView.DataContext = merdViewModel;
                MerdsWindows.Children.Add(merdView);
                //Initialize summary (vertical)
                //var simplifiedMerdView = new SimplifiedSingleMerdView();
                //simplifiedMerdView.DataContext = merdViewModel;

                merds.Add(merdViewModel);

            }
            var merdOverviewViewmodel = new MerdOverviewViewmodel(merds, 300);
            merdOverviewView.DataContext = merdOverviewViewmodel;


            GlobalUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            GlobalUpdateTimer.Start();

            //string fourierPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\fft_data\" + "feeding_fft_no_stress.csv");
            //string signalpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\signal_data\" + "signal_Feeding_no_stress.csv");
            //string wellbeingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\true_false_dat\" + "bool_feeding_no_stress.csv");
            //var fakeClient = new FakeSender(signalpath, fourierPath, wellbeingPath);



        }


    }
}

