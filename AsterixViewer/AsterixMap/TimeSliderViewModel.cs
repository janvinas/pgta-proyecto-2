using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AsterixViewer.AsterixMap
{
    public class TimeSliderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly DataStore dataStore;

        public double MinReplayTime => dataStore.MinReplayTime;
        public double MaxReplayTime => dataStore.MaxReplayTime;

        public TimeSpan MinReplayTimeSpan => TimeSpan.FromSeconds(MinReplayTime);
        public TimeSpan MaxReplayTimeSpan => TimeSpan.FromSeconds(MaxReplayTime);


        private readonly DispatcherTimer _replayTimeThrottleTimer;
        private double _uiReplayTime;
        private double _latestValue;
        private bool _hasPendingValue;
        public double ReplayTime
        {
            get => _uiReplayTime;
            set
            {
                // Update local slider value instantly
                if (Math.Abs(_uiReplayTime - value) > 0.0001)
                {
                    _uiReplayTime = value;
                    OnPropertyChanged(nameof(ReplayTime));
                }

                // Schedule throttled datastore update
                _latestValue = Math.Max(dataStore.MinReplayTime, Math.Min(dataStore.MaxReplayTime, value));
                _hasPendingValue = true;
            }
        }


        private void FlushReplayTime()
        {
            if (!_hasPendingValue)
                return;

            ApplyReplayTime(_latestValue);
            _hasPendingValue = false;
        }

        private void ApplyReplayTime(double value)
        {
            var newVal = Math.Max(dataStore.MinReplayTime, Math.Min(dataStore.MaxReplayTime, value));
            if (Math.Abs(dataStore.ReplayTime - newVal) > 0.0001)
            {
                dataStore.ReplayTime = newVal;
                // Optionally sync back UI in case of corrections
                _uiReplayTime = newVal;
                OnPropertyChanged(nameof(ReplayTime));
            }
        }

        public void SetUiTime(double value)
        {
            _uiReplayTime = value;
        }

        public TimeSliderViewModel(DataStore dataStore)
        {
            this.dataStore = dataStore;
            dataStore.PropertyChanged += OnDataStoreChanged;
            _replayTimeThrottleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // update frequency
            };
            _replayTimeThrottleTimer.Tick += (s, e) => FlushReplayTime();
            _replayTimeThrottleTimer.Start();
        }

        private void OnDataStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStore.ReplayTime))
            {
                // El DataStore cambió el tiempo (timer o slider)
                OnPropertyChanged(nameof(ReplayTime));
            }
            else if (e.PropertyName == nameof(DataStore.MinReplayTime))
            {
                OnPropertyChanged(nameof(MinReplayTime));
                OnPropertyChanged(nameof(MinReplayTimeSpan));
            }
            else if (e.PropertyName == nameof(DataStore.MaxReplayTime))
            {
                OnPropertyChanged(nameof(MaxReplayTime));
                OnPropertyChanged(nameof(MaxReplayTimeSpan));
            }
            else if (e.PropertyName == nameof(DataStore.Flights))
            {
                // Asegúrate de notificar min/max si Flights cambió
                OnPropertyChanged(nameof(MinReplayTime));
                OnPropertyChanged(nameof(MaxReplayTime));
                OnPropertyChanged(nameof(MinReplayTimeSpan));
                OnPropertyChanged(nameof(MaxReplayTimeSpan));
            }
        }
    }
}
