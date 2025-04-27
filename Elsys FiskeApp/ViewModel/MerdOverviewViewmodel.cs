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

        public bool _isOpen = true;
        public bool IsOpen
        {
            get => _isOpen;
            set { _isOpen = value; OnPropertyChanged(); }
        }
        public float _stackPanelWidth;

        private float _savedOpenValue;
        public MerdOverviewViewmodel(ObservableCollection<SingleMerdViewModel> _merds, float _openValue)
        {
            _MerdViews = _merds;
            ToggleMenuBar = new RelayCommand((sender) => ToggleWindow());
            WindowWidth = _openValue;
            _savedOpenValue = _openValue;
        }
        private void ToggleWindow()
        {
            if (IsOpen)
            {
                WindowWidth = 0;

            }
            else 
            {
                WindowWidth = _savedOpenValue;
            }
            IsOpen = !IsOpen;
        }

       
    }
}
