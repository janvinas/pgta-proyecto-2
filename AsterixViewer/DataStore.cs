using AsterixParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace AsterixViewer
{
    public class DataStore : INotifyPropertyChanged
    {
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
