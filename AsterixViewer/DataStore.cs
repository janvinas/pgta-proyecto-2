using AsterixParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsterixViewer
{
    public class DataStore : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private List<AsterixMessage> _messages = [];
        public List<AsterixMessage> Messages
        {
            get => _messages;
            set { _messages = value;  OnPropertyChanged(); }
        }

        private Dictionary<uint, List<AsterixMessage>> _flights = [];
        public Dictionary<uint, List<AsterixMessage>> Flights
        {
            get => _flights;
            set { _flights = value; OnPropertyChanged(); }
        }

        private double _replayTime = 14400;
        public double ReplayTime
        {
            get => _replayTime;
            set { _replayTime = value; OnPropertyChanged(); }
        }
    }
}
