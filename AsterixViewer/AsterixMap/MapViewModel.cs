using Accord.IO;
using AsterixParser;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;

namespace AsterixViewer.AsterixMap
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public ICommand ChangeTimeCommand { get; }

        private readonly DataStore dataStore;

        private Map? _map;
        public Map? Map
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        private GraphicsOverlayCollection? _graphicsOverlays;
        public GraphicsOverlayCollection? GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set
            {
                _graphicsOverlays = value;
                OnPropertyChanged();
            }
        }
        private GraphicsOverlay planeGraphics;
        private readonly Symbol planeSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 4);

        public MapViewModel(DataStore dataStore)
        {
            this.dataStore = dataStore;
            dataStore.PropertyChanged += OnDataStoreChanged;

            ChangeTimeCommand = new RelayCommand((object? args) =>
            {
                if (args is string timeString && int.TryParse(timeString, out int time))
                {
                    dataStore.ReplayTime += time;
                }
            });

            SetupMap();
            planeGraphics = new GraphicsOverlay();
            GraphicsOverlays = [planeGraphics];
            DisplayFlights();
        }

        private SortedDictionary<uint, Graphic> mapPoints = [];

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // fired when anything in the data store changes. Here we are interested in the flight list
        private void OnDataStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStore.ReplayTime))
            {
                DisplayFlights();
            }
            if (e.PropertyName == nameof(DataStore.Flights))
            {
                mapPoints = [];
                planeGraphics.Graphics.Clear();
                DisplayFlights();
            }
        }

        private void SetupMap()
        {
            var map = new Map(BasemapStyle.ArcGISTopographic);
            var mapCenterPoint = new MapPoint(5.840724825400484, 44.645513412977614, SpatialReferences.Wgs84);
            map.InitialViewpoint = new Viewpoint(mapCenterPoint, 30000000);
            Map = map;
        }

        private void DisplayFlights()
        {
            if (dataStore == null) return;
            foreach (var item in dataStore.Flights)
            {
                var msg = FindMessage(item.Value, dataStore.ReplayTime);
                if (msg == null) continue;

                var position = new MapPoint(msg.Longitude.Value, msg.Latitude.Value, SpatialReferences.Wgs84);

                if (mapPoints.TryGetValue(item.Key, out var graphic))
                {
                    graphic.Geometry = position;
                    if (msg.Cat == CAT.CAT021)
                    {
                        graphic.Symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Chocolate, 4);
                    }
                    else
                    {
                        graphic.Symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.BlueViolet, 4);
                    }
                }
                else
                {
                    var g = new Graphic(position, planeSymbol.Clone());
                    planeGraphics.Graphics.Add(g);
                    mapPoints[item.Key] = g;
                }
            }
        }

        private static AsterixMessage? FindMessage(List<AsterixMessage> messages, double time)
        {
            int index = messages.BinarySearch(
                new AsterixMessage { TimeOfDay = time },
                Comparer<AsterixMessage>.Create((a, b) => {
                    if (a.TimeOfDay == null && b.TimeOfDay == null) return 0;
                    if (a.TimeOfDay == null) return -1;  // treat null as smaller
                    if (b.TimeOfDay == null) return 1;   // treat null as smaller
                    return a.TimeOfDay.Value.CompareTo(b.TimeOfDay.Value);
                })
            );

            AsterixMessage msg;
            if (index >= 0)
            {
                // exact match
                msg = messages[index];
            }
            else
            {
                // get the message immediately before
                int nextIndex = ~index;
                int prevIndex = nextIndex - 1;
                if (prevIndex < 0) return null;
                msg = messages[prevIndex];
            }
            if (msg.Longitude == null || msg.Latitude == null) return null;
            return msg;
        }

    }
}
