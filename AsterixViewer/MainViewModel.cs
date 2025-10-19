using AsterixViewer.AsterixMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsterixViewer
{
    internal class MainViewModel
    {
        public DataStore DataStore { get; }
        public MapViewModel MapViewModel { get; }

        public MainViewModel()
        {
            DataStore = new DataStore();
            MapViewModel = new MapViewModel(DataStore);
        }
    }
}
