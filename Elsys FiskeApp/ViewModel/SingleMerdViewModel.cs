using Elsys_FiskeApp.Model;
using MQTTnet;
using OxyPlot;
using OxyPlot.Axes;
using System.Data;
using System.IO;
using System.Media;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
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
               
                return merdModel.brokerClient.ConnectionStatus.ToString();

            }

        }

        public string WellbeingStatus
        {
            get
            {
                return merdModel.WellbeingStatus.ToString();
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
        public string ActualHydrophonePosition
        {
            get 
            { 
                var result = "(" + merdModel.position.X.ToString() + ", " +merdModel.position.Y.ToString() 
                    + ", " + merdModel.position.Z.ToString() + ")";
                return result;
                    
            }
        }
        
        public RelayCommand ReconnectCommand { get; set; }
        public RelayCommand ApplyPositionCommand { get; set; }
        public RelayCommand SendWarning {  get; set; }
        public SingleMerdViewModel(SingleMerdModel model)
        {
            merdModel = model;
            merdModel.brokerClient.ConnectionStateChanged += () => { OnPropertyChanged(nameof(ConnectionStatus)); CommandManager.InvalidateRequerySuggested(); };
            merdModel.statusVariablesChanged += () =>
            {
                OnPropertyChanged(nameof(MerdName));
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(WellbeingStatus));
                OnPropertyChanged(nameof(ActualHydrophonePosition));

            };

            TrendPlotViewModel = new SinglePlotViewModel("Trend", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Events per second");
            RawPlotViewModel = new SinglePlotViewModel("Raw Signal", new LinearAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Time / s", "Volume");
            FourierPlotViewModel = new SinglePlotViewModel("Fourier Transform", new LogarithmicAxis { Position = AxisPosition.Bottom, Key = "xAxis" }, "Frequency / f", "Fourier");
            
            ReconnectCommand = new RelayCommand(execute => merdModel.AttemptConnection(), canexecute => shouldConnect() );
            ApplyPositionCommand = new RelayCommand(execute => merdModel.PublishWantedPosition(new Vector3 { X = float.Parse(HydrophoneX), 
                Y = float.Parse(HydrophoneY), Z = float.Parse(HydrophoneZ) }), canExecute => shouldSendPosition());
            SendWarning = new RelayCommand(execute => Warn());

            DataHolder.Instance.GlobalUpdateTimer.Tick += (sender, e) => updatePlots();
        }

        bool shouldConnect()
        {
             if (merdModel.brokerClient.ConnectionStatus == MQTTnet.MqttClientConnectionStatus.Disconnected) return true;
             else return false; 
            
        }

        async void Warn()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BadWelfareSound.wav");
            SoundPlayer player = new SoundPlayer(filePath);

            if (File.Exists(filePath))
            {
                player.PlayLooping();
            }
            MessageBox.Show("Bad stress detected in merd: " + MerdName + "!");
            player.Stop();

        }

        bool shouldWarn()
        {
            if (WellbeingStatus == "Bad") return true;
            else return false;
        }
        bool shouldSendPosition() // returns true if position is within bounds and the brokerclient is connected.
        {
            if (HydrophoneX != "" && HydrophoneY != "" && HydrophoneZ != "" && merdModel.brokerClient.ConnectionStatus == MqttClientConnectionStatus.Connected)
            {
                if (float.Parse(HydrophoneX) == merdModel.position.X && float.Parse(HydrophoneY) == merdModel.position.Y && float.Parse(HydrophoneZ) == merdModel.position.Z) return false;
                //var distance = Math.Pow(Math.Pow(float.Parse(HydrophoneX), 2) + Math.Pow(float.Parse(HydrophoneY), 2), 0.5);
                //var height = float.Parse(HydrophoneZ);
                //if (distance <= merdModel.Radius && height <= merdModel.Height) return true; // if input is within bounds
                if (float.Parse(HydrophoneX) <= 400 && float.Parse(HydrophoneY) <= 360 && float.Parse(HydrophoneZ) <= 800) return true;
            }
            
            return false;
        }

        void updatePlots()
        {
            var plottingData = DataHolder.Instance.newProcessedData[merdModel.MerdName].ToList();

            updateSpecificPlot(plottingData, TrendPlotViewModel, data => data.TreatedSignal);
            updateFourierPlot(plottingData, FourierPlotViewModel);
            updateSpecificPlot(plottingData, RawPlotViewModel, data => data.RawData);

            DataHolder.Instance.newProcessedData[merdModel.MerdName].Clear();
        }

        void updateSpecificPlot(List<updateData> updateDataList, SinglePlotViewModel plotModel, Func<updateData, float> valueSelector)
        {
            List<DataPoint> points = updateDataList.Select(data => new DataPoint(data.Time, valueSelector(data))).ToList();
            plotModel.addPoints(points);
        }
        void updateFourierPlot(List<updateData> updateDataList, SinglePlotViewModel plotModel)
        {
            if (updateDataList.Count > 0)
            {
                plotModel.SetPoints(updateDataList[0].FourierData);
            }
        }


    }

}
