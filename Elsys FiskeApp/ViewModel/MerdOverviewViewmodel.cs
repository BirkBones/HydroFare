using Elsys_FiskeApp.Model;
using Elsys_FiskeApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elsys_FiskeApp.ViewModel
{
    public class MerdOverviewViewmodel : ViewModelBase
    {
        private readonly ObservableCollection<SingleMerdViewModel> _MerdViews;
        public ObservableCollection<SingleMerdViewModel> MerdViews => _MerdViews;

        private float _windowWidth = 500;
        public float WindowWidth
        {
            get => _windowWidth;
            set { _windowWidth = value; OnPropertyChanged(); } 
        }
        public RelayCommand ToggleMenuBar {  get; set; }

        public bool _isClosed = false;
        public bool IsClosed
        {
            get => _isClosed;
            set { _isClosed = value; OnPropertyChanged(); }
        }

        float closedValue;
        float openValue;
        public MerdOverviewViewmodel(ObservableCollection<SingleMerdViewModel> _merds, float _closedValue, float _openValue)
        {
            _MerdViews = _merds;
            ToggleMenuBar = new RelayCommand((sender) => ToggleWindow());
            closedValue = _closedValue;
            openValue = _openValue;
            WindowWidth = closedValue;


        }
        private void ToggleWindow()
        {
            if (IsClosed)
            {
                WindowWidth = openValue;
            }
            else
            {
                WindowWidth = closedValue;
            }
            IsClosed = !IsClosed;
        }

       
    }
}
