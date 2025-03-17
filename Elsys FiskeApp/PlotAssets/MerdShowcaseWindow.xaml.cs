using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Elsys_FiskeApp.PlotAssets
{
   
    public partial class MerdShowcaseWindow : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        SinglePlot SingleplotInstance {  get; set; }

        public SinglePlot singlePlot;

        private double plotHeight = 200;
        public double PlotHeight
        {
            get { return plotHeight; }
            set { OnPropertyChanged(); plotHeight = value;
            }
        }

        double plotWidth = 500;
        public double PlotWidth
        {
            get { return plotWidth; }
            set { OnPropertyChanged(); plotWidth = value; }
        }
        private string title;
        public string PlotTitle
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();



            }
        }
        public MerdShowcaseWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        }

    }
}
