using AsterixViewer.AsterixMap;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AsterixViewer.Tabs
{
    public partial class MapTab : UserControl
    {
        private MapViewModel ViewModel;

        public MapTab()
        {
            InitializeComponent();
            var dataStore = ((App)Application.Current).DataStore;
            var filtersViewModel = ((App)Application.Current).FiltersViewModel;
            ViewModel = new MapViewModel(dataStore, filtersViewModel);
            DataContext = ViewModel;
            MainMapView.GeoViewTapped += OnMapTapped;
        }

        private async void OnMapTapped(object? sender, GeoViewInputEventArgs e)
        {
            if (ViewModel?.GraphicsOverlays == null) return;

            foreach (var overlay in ViewModel.GraphicsOverlays)
            {
                var result = await MainMapView.IdentifyGraphicsOverlayAsync(
                    overlay,
                    e.Position,
                    tolerance: 15,
                    returnPopupsOnly: false,
                    maximumResults: 1
                );

                if (result.Graphics.Count > 0)
                {
                    var g = result.Graphics[0];
                    ViewModel.ShowGraphicDetails(g);
                    return;
                }
            }

            ViewModel.HideGraphicDetails();
        }

        private void Slider_DragEnter(object sender, DragEventArgs e)
        {

        }
    }
}
