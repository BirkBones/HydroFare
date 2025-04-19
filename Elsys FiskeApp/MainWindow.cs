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
        BrokerClientsHandler brokerClientsHandler;
        SingleMerdViewModel merdViewModel;
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
                brokerClientsHandler = new BrokerClientsHandler();
            });

            await brokerClientsHandler.BrokerClients["Merd1"].Subscribe("rawData", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);
            GlobalUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            GlobalUpdateTimer.Start();
            merdViewModel = (SingleMerdViewModel)firstMerd.DataContext;

        }

    }

    }

