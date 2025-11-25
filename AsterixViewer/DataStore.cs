using AsterixParser;
using AsterixViewer.AsterixMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace AsterixViewer
{
    public class DataStore : INotifyPropertyChanged
    {
        public DataStore() {
            PlayPauseCommand = new(args =>
            {
                IsPlaying = !IsPlaying;
                if (IsPlaying) _replayTimer.Start();
                else _replayTimer.Stop();
            });

            ChangeSpeedCommand = new((object? args) =>
            {
                switch (_replaySpeedMultiplier)
                {
                    case 1: _replaySpeedMultiplier = 2; break;
                    case 2: _replaySpeedMultiplier = 4; break;
                    case 4: _replaySpeedMultiplier = 8; break;
                    case 8: _replaySpeedMultiplier = 16; break;
                    case 16: _replaySpeedMultiplier = 32; break;
                    default: _replaySpeedMultiplier = 1; break;
                }

                int newIntervalMs = 1000 / _replaySpeedMultiplier;
                _replayTimer.Interval = TimeSpan.FromMilliseconds(newIntervalMs);
                OnPropertyChanged(nameof(ReplaySpeedText));
            });

            _replayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _replayTimer.Tick += OnTimerTick;

            ChangeTimeCommand = new RelayCommand((object? args) =>
            {
                if (args is string timeString && int.TryParse(timeString, out int time))
                {
                    ReplayTime += time;
                    OnPropertyChanged(nameof(ReplayTimeText));

                    //update slider
                    var timeSliderViewModel = ((App)Application.Current).TimeSliderViewModel;
                    timeSliderViewModel.SetUiTime(ReplayTime);
                }
            });
        }

        public ICollectionView FilteredMessages { get; private set; }
        public void RefreshFilters()
        {
            if (FilteredMessages != null)
                FilteredMessages.Refresh();
        }

        public void InitializeFiltering()
        {
            FilteredMessages = CollectionViewSource.GetDefaultView(Messages);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private List<AsterixMessage> _messages = [];
        public List<AsterixMessage> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged();
                UpdateReplayTimeBounds();
            }
        }

        private Dictionary<uint, Flight> _flights = [];
        public Dictionary<uint, Flight> Flights
        {
            get => _flights;
            set
            {
                _flights = value;
                OnPropertyChanged();
                UpdateReplayTimeBounds();
            }
        }

        private double _replayTime;
        public double ReplayTime
        {
            get => _replayTime;
            set
            {
                if (_replayTime != value)
                {
                    _replayTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _minReplayTime;
        public double MinReplayTime
        {
            get => _minReplayTime;
            set
            {
                if (_minReplayTime != value)
                {
                    _minReplayTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _maxReplayTime;
        public double MaxReplayTime
        {
            get => _maxReplayTime;
            set
            {
                if (_maxReplayTime != value)
                {
                    _maxReplayTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayPauseButtonText));
            }
        }

        private int _replaySpeedMultiplier = 1;


        public string PlayPauseButtonText => IsPlaying ? "Pause" : "Play";
        public string ReplaySpeedText => $"x{_replaySpeedMultiplier}";

        public RelayCommand PlayPauseCommand { get; }
        public RelayCommand ChangeSpeedCommand { get; }
        public RelayCommand ChangeTimeCommand { get; }
        private DispatcherTimer _replayTimer;

        public string ReplayTimeText => TimeSpan.FromSeconds(ReplayTime).ToString(@"hh\:mm\:ss\.fff");

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // incrementa el dataStore; DataStore notificará y OnDataStoreChanged actualizará la UI
            ReplayTime += 1;
            // por seguridad forzamos notificación local también:
            //TimeSliderViewModel.OnPropertyChanged(nameof(TimeSliderViewModel.ReplayTime));
            OnPropertyChanged(nameof(ReplayTimeText));

            //update slider
            var timeSliderViewModel = ((App)Application.Current).TimeSliderViewModel;
            timeSliderViewModel.SetUiTime(ReplayTime);
        }


        /// <summary>
        /// Calcula el primer y último tiempo en los mensajes cargados.
        /// </summary>
        private void UpdateReplayTimeBounds()
        {
            var allTimes = Messages?
                .Where(m => m.TimeOfDay.HasValue)
                .Select(m => m.TimeOfDay!.Value)
                .ToList();

            if (allTimes != null && allTimes.Count > 0)
            {
                MinReplayTime = allTimes.Min();
                MaxReplayTime = allTimes.Max();
                ReplayTime = MinReplayTime; // empieza en el primer punto (por ejemplo 14400 = 4:00:00)
            }
            else
            {
                MinReplayTime = 0;
                MaxReplayTime = 0;
                ReplayTime = 0;
            }
        }
    }
}
