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
using Microsoft.VisualBasic;
using MQTTnet;
using System.Data;
using MQTTnet.Protocol;
namespace Elsys_FiskeApp.ViewModel
{
    public class SingleMerdViewModel : ViewModelBase
    {

        public SinglePlotViewModel TrendPlotViewModel { get; set; }

        public SinglePlotViewModel RawPlotViewModel { get; set; }

        public SinglePlotViewModel FourierPlotViewModel { get; set; }

        public SingleMerdModel merdModel { set; get; }
        public string ConnectionStatus
        {
            get {
               
                if (merdModel.brokerClient.ConnectionStatus == MQTTnet.MqttClientConnectionStatus.Connected) return "Connected";
                else
                {
                    if (merdModel.brokerClient.ConnectionStatus == MQTTnet.MqttClientConnectionStatus.Disconnecting) return "Attempting Connection";
                }
                
                return "Disconnected";

            }

        }

        public string WellbeingStatus
        {
            get
            {
                return merdModel.WellbeingStatus;
            }
        }
        #region hydroPos
        public string MerdName
        {
            get
            {
                return merdModel.MerdName;
            }
        }

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
        
        public RelayCommand ReconnectCommand { get; set; }
        public RelayCommand ApplyPositionCommand { get; set; }
        public SingleMerdViewModel(SingleMerdModel model)
        {
            merdModel = model;
            merdModel.brokerClient.ConnectionStateChanged += () => { OnPropertyChanged(nameof(ConnectionStatus)); CommandManager.InvalidateRequerySuggested(); };
            merdModel.statusVariablesChanged += () =>
            {
                OnPropertyChanged(nameof(MerdName));
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(WellbeingStatus));
            };

                TrendPlotViewModel = new SinglePlotViewModel("Trend", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Events per second");
            RawPlotViewModel = new SinglePlotViewModel("Raw Signal", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Volume");
            FourierPlotViewModel = new SinglePlotViewModel("Fourier Transform", new LogarithmicAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Frequency / f", "Fourier");
            
            ReconnectCommand = new RelayCommand(execute => AttemptConnection(), canexecute => shouldConnect() );
            ApplyPositionCommand = new RelayCommand(execute => updatePosition(), canExecute => shouldSendPosition());


            DataHolder.Instance.GlobalUpdateTimer.Tick += (sender, e) => updatePlots();
        }
        async Task AttemptConnection()
        {
            await merdModel.brokerClient.ConnectToBroker();
            await merdModel.brokerClient.Subscribe("rawData", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);

        }
        void updatePosition() // if position cannot be sent, it will be sent the next time the brokerclient is connected.
        {
            
            merdModel.position = new Vector3 { X = float.Parse(HydrophoneX), Y = float.Parse(HydrophoneY), Z = float.Parse(HydrophoneZ) };
            string message = "(" + HydrophoneX + ", " + HydrophoneY + ", " + HydrophoneZ + ")";
            merdModel.brokerClient.PublishMessage("wantedHydrophonePlacement", message, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
            
        }

        bool shouldConnect()
        {
             if (merdModel.brokerClient.ConnectionStatus == MQTTnet.MqttClientConnectionStatus.Disconnected) return true;
             else return false; 
            
        }
        bool shouldSendPosition() // returns true if position is within bounds and the brokerclient is connected.
        {

            if (HydrophoneX != "" && HydrophoneY != "" && HydrophoneZ != "" && merdModel.brokerClient.ConnectionStatus == MqttClientConnectionStatus.Connected)
            {
                if (float.Parse(HydrophoneX) == merdModel.position.X && float.Parse(HydrophoneY) == merdModel.position.Y && float.Parse(HydrophoneZ) == merdModel.position.Z) return false;
                var distance = Math.Pow(Math.Pow(float.Parse(HydrophoneX), 2) + Math.Pow(float.Parse(HydrophoneY), 2), 0.5);
                var height = float.Parse(HydrophoneZ);
                if (distance <= merdModel.Radius && height <= merdModel.Height) return true; // if input is within bounds
            }
            
            return false;
        }

        void updatePlots()
        {
            var plottingData = DataHolder.Instance.newProcessedData[merdModel.MerdName].ToList();

            updateSpecificPlot(plottingData, TrendPlotViewModel, data => data.TreatedSignal);
            updateSpecificPlot(plottingData, FourierPlotViewModel, data => data.FourierData);
            updateSpecificPlot(plottingData, RawPlotViewModel, data => data.RawData);

            DataHolder.Instance.newProcessedData[merdModel.MerdName].Clear();
        }

        void updateSpecificPlot(List<updateData> updateDataList, SinglePlotViewModel plotModel, Func<updateData, float> valueSelector)
        {
            List<DataPoint> points = updateDataList.Select(data => new DataPoint(data.Time, valueSelector(data))).ToList();
            plotModel.addPoints(points);
        }


    }

}
