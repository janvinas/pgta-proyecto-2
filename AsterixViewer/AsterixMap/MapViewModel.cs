using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public ICommand AddPointCommand { get; }
        public ICommand MovePointCommand { get; }

        private Graphic? _graphic;

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

        public MapViewModel()
        {
            SetupMap();
            planeGraphics = new GraphicsOverlay();
            GraphicsOverlays = [planeGraphics];

            AddPointCommand = new RelayCommand(() => 
            {
                var point = new MapPoint(-118.805, 34.027, SpatialReferences.Wgs84);
                var symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);
                _graphic = new Graphic(point, symbol);

                // Add to overlay
                planeGraphics.Graphics.Add(_graphic);
            });

            MovePointCommand = new RelayCommand(() =>
            {
                _graphic.Geometry = new MapPoint(-120.805, 34.027, SpatialReferences.Wgs84);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetupMap()
        {
            var map = new Map(BasemapStyle.ArcGISTopographic);
            var mapCenterPoint = new MapPoint(5.840724825400484, 44.645513412977614, SpatialReferences.Wgs84);
            map.InitialViewpoint = new Viewpoint(mapCenterPoint, 30000000);
            Map = map;
        }

    }
}
