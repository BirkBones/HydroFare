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

            foreach (var merd in merdsHandler.Merds)
            {

            }


            await merdsHandler.Merds["Merd1"].brokerClient.Subscribe("rawData", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);
            await merdsHandler.Merds["Merd1"].brokerClient.Subscribe("actualHydrophonePlacement", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);

            GlobalUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            GlobalUpdateTimer.Start();

            merdViewModel = new SingleMerdViewModel(merdsHandler.Merds["Merd1"]);
            firstMerd.DataContext = merdViewModel;

        }
        /* Det som skjer: når man tilegner new viewmodel is view.cs, blir dette den opprinnelige datakonteksten.
         Dersom man prøver å erstatte denne, erstatter man den, men det originale objektet blir ikke ødelagt og lager et krasj.
        Grunnen til at det ikke blir ødelagt er at det subscriber på ting, og de må desubscribes før objektet blir avinstansiert.
        I tillegg blir det nye objektet ikke bindet opp korrekt til viewen, */
    }

    }

