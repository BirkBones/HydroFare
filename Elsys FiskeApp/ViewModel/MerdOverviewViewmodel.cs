using Elsys_FiskeApp.Model;
using Elsys_FiskeApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Elsys_FiskeApp.ViewModel
{
    public class MerdOverviewViewmodel : ViewModelBase
    {
        private readonly ObservableCollection<SingleMerdViewModel> _MerdViews;
        public ObservableCollection<SingleMerdViewModel> MerdViews => _MerdViews;

        public List<SingleMerdModel> models;

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
        public RelayCommand ChangeWarningNotification { get; set; }
        public MerdOverviewViewmodel(ObservableCollection<SingleMerdViewModel> _merds, float _openValue)
        {
            _MerdViews = _merds;
            ToggleMenuBar = new RelayCommand((sender) => ToggleWindow());
            WindowWidth = _openValue;
            _savedOpenValue = _openValue;
            models = new();
            foreach (var item in MerdViews)
            {
                models.Add(item.merdModel);
                item.merdModel.criticalEvent += (name) => Warn(name);
            }
            ChangeWarningNotification = new RelayCommand((e) => ChangeWarning());
        }

        private void ChangeWarning()
        {
            shouldWarn = !shouldWarn;
        }

        public bool shouldWarn { get; set; } = false;

        async void Warn(string MerdName)
        {
            if (shouldWarn == true)
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
