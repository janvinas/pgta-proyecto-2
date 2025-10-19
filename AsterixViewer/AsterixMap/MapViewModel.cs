using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
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

namespace AsterixViewer.AsterixMap
{
    public class MapViewModel : INotifyPropertyChanged
    {
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

        public MapViewModel(DataStore dataStore)
        {
            this.dataStore = dataStore;
            dataStore.PropertyChanged += OnDataStoreChanged;
            SetupMap();
            planeGraphics = new GraphicsOverlay();
            GraphicsOverlays = [planeGraphics];
            DisplayFlights();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // fired when anything in the data store changes. Here we are interested in the flight list
        private void OnDataStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(dataStore.Flights))
            {
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
            planeGraphics.Graphics.Clear();
            foreach (var item in dataStore.Flights)
            {
                var flight = item.Value[0];
                if (flight.Latitude == null || flight.Longitude == null) continue;

                var point = new MapPoint(flight.Longitude.Value, flight.Latitude.Value, SpatialReferences.Wgs84);
                var symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 4);
                var graphic = new Graphic(point, symbol);
                planeGraphics.Graphics.Add(graphic);
            }

        }

    }
}
