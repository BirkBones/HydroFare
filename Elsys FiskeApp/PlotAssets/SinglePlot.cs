using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Wpf;
namespace Elsys_FiskeApp.PlotAssets
{
    public enum State
    {
        Decent, Bad, Critical
    }
    public partial class SinglePlot : UserControl, INotifyPropertyChanged
    {

        PlottingModule _PlottingModule;
        public PlotModel CurrentPlotModel { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private double plotHeight = 200;
        public double PlotHeight
        {
            get { return plotHeight; }
            set { OnPropertyChanged();  plotHeight = value; }
        }

        double plotWidth = 500;
        public double PlotWidth
        {
            get { return plotWidth; }
            set { OnPropertyChanged();  plotWidth = value; }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                if (CurrentPlotModel != null)
                {
                    CurrentPlotModel.Title = title;
                    CurrentPlotModel.InvalidatePlot(true);
                }
              
            }
            }

        
        public SinglePlot()
        {
            InitializeComponent();
            DataContext = this;
            _PlottingModule = new PlottingModule();
            CurrentPlotModel = new PlotModel { Title = this.Title};
            CurrentPlotModel.Series.Add(_PlottingModule.InitializeSoundPlotLineSeries(2));
            if (MainWindow.GlobalUpdateTimer!=null)
            MainWindow.GlobalUpdateTimer.Tick += (sender, e) => _PlottingModule.UpdateInRealTime(CurrentPlotModel, 2);

           
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        }
    }
}