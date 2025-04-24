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

namespace Elsys_FiskeApp.Model
{
    public class SingleMerdModel // HOlds all the data for each merd, including the brokerclient.
    {
        private string _merdName;
		private float _radius;
		private float _height;
        private string _wellbeingStatus = "Unknown";
        public Vector3 _position = new Vector3() { X = 0, Y = 0, Z = 0 };
        public BrokerClient brokerClient;
        public Action statusVariablesChanged;
        public string MerdName
		{
			get { return _merdName; }
			set { _merdName = value; statusVariablesChanged?.Invoke(); }
		}

		public float Radius
		{ get { return _radius; } set { _radius = value; } }

		public float Height { get { return _height; } set {_height = value; } }
  
        public string WellbeingStatus
        {
            get => _wellbeingStatus;
            set { _wellbeingStatus = value; statusVariablesChanged?.Invoke(); }
        }

        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }
    
        public SingleMerdModel(MerdSettings settings)
        {
            Radius = settings.Radius;
            Height = settings.Height;
            MerdName = settings.MerdName;
            brokerClient = new BrokerClient(settings);
        }

    

    }
}
