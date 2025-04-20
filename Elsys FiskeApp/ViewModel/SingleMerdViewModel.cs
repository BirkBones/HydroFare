using Elsys_FiskeApp.Model;
using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
namespace Elsys_FiskeApp.ViewModel
{
    public class SingleMerdViewModel : ViewModelBase
    {
        public SinglePlotViewModel TrendPlotViewModel { get; set; }

        public SinglePlotViewModel RawPlotViewModel { get; set; }

        public SinglePlotViewModel FourierPlotViewModel { get; set; }

        public MQTTnet.BrokerClient brokerClient { get { return BrokerClientsHandler.Instance.BrokerClients[merdName]; } }

        private string merdName;
        public string MerdName
        {
            get { return merdName; }
            set { merdName = value; OnPropertyChanged(); }
        }

        float radius = 5;

        float height = 50;

        private string connectionStatus;
        public string ConnectionStatus
        {
            get {
                if (brokerClient != null)
                { if (brokerClient.IsConnected) return "Connected";
                    else
                    {
                        if (brokerClient.IsConnecting)
                        {
                            return "Attempting Connection";
                        }
                        else
                        {
                            return "Disconnected";
                        }
                    }
                }
                else
                {
                    return "Disconnected";
                }
            }

        }
        private string wellbeingStatus;
        public string WellbeingStatus
        {
            get => wellbeingStatus;
            set { wellbeingStatus = value; OnPropertyChanged(); }
        }
        #region hydroPos

        private string _hydrophoneX = "0";
        public string HydrophoneX
        {
            get => _hydrophoneX;
            set { _hydrophoneX = value; OnPropertyChanged();}
        }

        private string _hydrophoneY = "0";
        public string HydrophoneY
        {
            get => _hydrophoneY;
            set { _hydrophoneY = value; OnPropertyChanged(); }
        }

        private string _hydrophoneZ = "0";
        public string HydrophoneZ
        {
            get => _hydrophoneZ;
            set { _hydrophoneZ = value; OnPropertyChanged(); }
        }
        #endregion hydroPos
        public Vector3 _position = new Vector3 () { X = 0, Y = 0, Z = 0 };
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }
        string lastUnSentPosition;
        string lastPositionSent;
        public RelayCommand ReconnectCommand { get; set; }
        public ICommand EditConnectionCommand { get; set; }
        public RelayCommand ApplyPositionCommand { get; set; }
        public SingleMerdViewModel()
        {
            merdName = "Merd1";
            TrendPlotViewModel = new SinglePlotViewModel("Trend", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Events per second");
            RawPlotViewModel = new SinglePlotViewModel("Raw Signal", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Volume");
            FourierPlotViewModel = new SinglePlotViewModel("Fourier Transform", new LogarithmicAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Frequency / f", "Fourier");

            ReconnectCommand = new RelayCommand(execute => AttemptConnection(), canexecute => { if (ConnectionStatus == "Disconnected") return true; else return false; });

            EditConnectionCommand = new RelayCommand(EditConnection);

            ApplyPositionCommand = new RelayCommand(execute => updatePosition(), canExecute => shouldSendPosition());
    

            WellbeingStatus = "Unknown";




            DataHolder.Instance.GlobalUpdateTimer.Tick += (sender, e) => updatePlots();
        }

      
        void updatePosition() // if position cannot be sent, it will be sent the next time the brokerclient is connected.
        {

            position = new Vector3 { X = float.Parse(HydrophoneX), Y = float.Parse(HydrophoneY), Z = float.Parse(HydrophoneZ) };
            string message = "(" + HydrophoneX + ", " + HydrophoneY + ", " + HydrophoneZ + ")";
            if (message == lastPositionSent) return;
            else
            {
                if (brokerClient.IsConnected)
                {
                    brokerClient.PublishMessage("wantedHydrophonePlacement", message, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    lastUnSentPosition = null;
                }
                else
                {
                    lastUnSentPosition = message;
                }
            }
        }

        bool shouldSendPosition() // returns true if position is within bounds and the brokerclient is connected.
        {
            if (HydrophoneX != "" && HydrophoneY != "" && HydrophoneZ != "")
            {
                var distance = Math.Pow(Math.Pow(float.Parse(HydrophoneX), 2) + Math.Pow(float.Parse(HydrophoneY), 2), 0.5);
                var height = float.Parse(HydrophoneZ);
                if (distance <= radius && height <= height && brokerClient.IsConnected) return true;
            }
            return false;
        }
        public void Test()
        {
            brokerClient.ConnectionStateChanged += () => { OnPropertyChanged(nameof(ConnectionStatus)); CommandManager.InvalidateRequerySuggested(); };
        }

        public void whenBrokerClientsHolderIsDone()
        {
           //ReconnectCommand = new RelayCommand(execute => { AttemptConnection(); }, canexecute => { return true;  } );

        }

        void AttemptConnection()
        {
            var task = brokerClient.ConnectToBroker(CancellationToken.None);
            if (brokerClient.IsConnected && lastUnSentPosition != null)
            {
                brokerClient.PublishMessage("wantedHydrophonePlacement", lastUnSentPosition, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
            }
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
            List<DataPoint> points = updateDataList.Select(data => new DataPoint(data.Time, valueSelector(data))).ToList();
            plotModel.addPoints(points);
        }

        void Reconnect(object obj)
        {
            // Assume some connection logic here
        }

        void EditConnection(object obj)
        {
            
        }

        void ApplyPosition(object obj)
        {
            string result = $"Hydrophone position set to X: {HydrophoneX}, Y: {HydrophoneY}, Z: {HydrophoneZ}";
            MessageBox.Show(result);
        }
    }

}
