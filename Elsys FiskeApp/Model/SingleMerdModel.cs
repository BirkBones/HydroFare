using Elsys_FiskeApp.ViewModel;
using Microsoft.VisualBasic;
using OxyPlot.Axes;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MQTTnet;
using MQTTnet.Protocol;
using System.Diagnostics;
using System.IO;

namespace Elsys_FiskeApp.Model
{
    public class SingleMerdModel // HOlds all the data for each merd, including the brokerclient.
    {
        private string _merdName;
		private float _radius;
		private float _height;
        private Wellfare _wellbeingStatus = Wellfare.UnKnown;
        public Vector3 _position = new Vector3() { X = 0, Y = 0, Z = 0 };
        public BrokerClient brokerClient;
        public Action statusVariablesChanged;
        public Action<string> criticalEvent;
        public static Dictionary<string, Wellfare> MerdStates;
        public string MerdName
		{
			get { return _merdName; }
			set { _merdName = value; statusVariablesChanged?.Invoke(); }
		}

		public float Radius
		{ get { return _radius; } set { _radius = value; } }

		public float Height { get { return _height; } set {_height = value; } }
  
        public Wellfare WellbeingStatus
        {
            get => _wellbeingStatus;
            set { 
                if (_wellbeingStatus != value)
                {
                    _wellbeingStatus = value; statusVariablesChanged?.Invoke();
                    if (value == Wellfare.Bad) criticalEvent?.Invoke(MerdName);
                }
            }
        }

        public Vector3 position
        {
            get { return _position; }
            set { _position = value; statusVariablesChanged?.Invoke(); }
        }

        bool lastState;
        public SingleMerdModel(MerdSettings settings)
        {
            Radius = settings.Radius;
            Height = settings.Height;
            MerdName = settings.MerdName;
            brokerClient = new BrokerClient(settings);
            //brokerClient.positionChanged += (newPos) => { position = newPos; };


            brokerClient.handleMessages += handleMessagesRecieved;
            AttemptConnection();
            if (MerdStates == null) MerdStates = new Dictionary<string, Wellfare>();
            StartShowcase();
        }

        void StartShowcase()
        {
            if (MerdName == "Merd1")
            {
                string fourierPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\fft_data\" + "feeding_fft_no_stress.csv");
                string signalpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\signal_data\" + "signal_Feeding_no_stress.csv");
                string wellbeingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\true_false_dat\" + "bool_feeding_no_stress.csv");
                var fakeClient = new FakeSender(signalpath, fourierPath, wellbeingPath);
                fakeClient.currentUpdateChanged += () =>
                {
                    brokerClient.inputData.Enqueue(fakeClient.currentUpdate);
                    WellbeingStatus = fakeClient.currentUpdate.IsHealthGood ? Wellfare.Good : Wellfare.Bad;
                    //if (lastState != fakeClient.currentUpdate.IsHealthGood && lastState!=null) System.Diagnostics.Debugger.Break();
                    lastState = fakeClient.currentUpdate.IsHealthGood;

                };


            }
            if (MerdName =="Merd2")
            {
                string fourierPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\fft_data\" + "fft_noFeeding_no_stress.csv");
                string signalpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\signal_data\" + "signal_noFeeding_no_stress.csv");
                string wellbeingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\true_false_dat\" + "bool_noFeeding_no_stress.csv");
                var fakeClient = new FakeSender(signalpath, fourierPath, wellbeingPath);
                fakeClient.currentUpdateChanged += () =>
                {
                    brokerClient.inputData.Enqueue(fakeClient.currentUpdate);
                    WellbeingStatus = fakeClient.currentUpdate.IsHealthGood ? Wellfare.Good : Wellfare.Bad;
                    //if (lastState != fakeClient.currentUpdate.IsHealthGood && lastState!=null) System.Diagnostics.Debugger.Break();
                    lastState = fakeClient.currentUpdate.IsHealthGood;

                };
            }
            if (MerdName == "Merd3")
            {
                string fourierPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\fft_data\" + "fft_noFeeding_with_stress.csv");
                string signalpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\signal_data\" + "signal_noFeeding_with_stress.csv");
                string wellbeingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\true_false_dat\" + "bool_noFeeding_with_stress.csv");
                var fakeClient = new FakeSender(signalpath, fourierPath, wellbeingPath);
                fakeClient.currentUpdateChanged += () =>
                {
                    brokerClient.inputData.Enqueue(fakeClient.currentUpdate);
                    WellbeingStatus = fakeClient.currentUpdate.IsHealthGood ? Wellfare.Good : Wellfare.Bad;
                    //if (lastState != fakeClient.currentUpdate.IsHealthGood && lastState!=null) System.Diagnostics.Debugger.Break();
                    lastState = fakeClient.currentUpdate.IsHealthGood;

                };
            }
        }

        public void handleMessagesRecieved (MqttApplicationMessageReceivedEventArgs e)
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            {
                if (e.ApplicationMessage.Topic == MerdName + "/rawData") // Uploads the data to the inputdata queue.
                {
                    var inputList = brokerClient.interpretRawData(payload);
                    inputList.ForEach(input => brokerClient.inputData.Enqueue(input));
                }
                else if (e.ApplicationMessage.Topic == MerdName + "/actualHydrophonePlacement") // when recieved the actual position.
                {
                    var pos = brokerClient.interpretHydrophonePosition(payload);
                    position = pos;
                }
            };
        }

        public async void PublishWantedPosition(Vector3 position)
        {
            string message = "(" + position.X.ToString() + ", " + position.Y.ToString() + ", " + position.Z.ToString() + ")";
            await brokerClient.PublishMessage(MerdName + "/wantedHydrophonePlacement", message, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

        }
        public async void AttemptConnection()
        {
            await brokerClient.ConnectToBroker();
            await brokerClient.Subscribe(MerdName + "/rawData", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);
            await brokerClient.Subscribe(MerdName + "/actualHydrophonePlacement", MqttQualityOfServiceLevel.ExactlyOnce, CancellationToken.None);
        }


    }
}
