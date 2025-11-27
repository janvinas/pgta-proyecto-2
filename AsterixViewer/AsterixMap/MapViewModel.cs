using AsterixParser;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AsterixViewer.AsterixMap
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public ICommand ChangeTimeCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand ChangeSpeedCommand { get; }
        public ICommand HideDetailsCommand { get; }

        public DataStore DataStore { get; set; }
        private readonly FiltersViewModel filtersViewModel;
        public TimeSliderViewModel TimeSliderViewModel { get; }

        private Map? _map;
        public Map? Map
        {
            get => _map;
            set { _map = value; OnPropertyChanged(); }
        }

        private GraphicsOverlayCollection? _graphicsOverlays;
        public GraphicsOverlayCollection? GraphicsOverlays
        {
            get => _graphicsOverlays;
            set { _graphicsOverlays = value; OnPropertyChanged(); }
        }

        private GraphicsOverlay planeGraphics;
        private GraphicsOverlay selectedOverlay;
        private Graphic? selectedHighlightGraphic;

        // 🟡 Símbolo de halo amarillo para selección
        private readonly Symbol highlightSymbol = new CompositeSymbol
        {
            Symbols =
            {
                new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.FromArgb(160, 255, 255, 0), 12),
                new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Transparent, 8)
            }
        };

        private bool _isInfoPanelVisible;
        public bool IsInfoPanelVisible
        {
            get => _isInfoPanelVisible;
            set { _isInfoPanelVisible = value; OnPropertyChanged(); }
        }

        private string _selectedGraphicInfo = string.Empty;
        public string SelectedGraphicInfo
        {
            get => _selectedGraphicInfo;
            set { _selectedGraphicInfo = value; OnPropertyChanged(); }
        }

        private Graphic? _selectedGraphic;
        public Graphic? SelectedGraphic
        {
            get => _selectedGraphic;
            set
            {
                _selectedGraphic = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, Graphic> mapPoints = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private const string PlaneIconUri = "pack://application:,,,/AsterixViewer;component/Resources/avion.png";
        private const string AntennaIconUri = "pack://application:,,,/AsterixViewer;component/Resources/antenna.png";
        public MapViewModel(DataStore dataStore, FiltersViewModel filtersViewModel, TimeSliderViewModel timeSliderViewModel)
        {
            this.DataStore = dataStore;
            this.filtersViewModel = filtersViewModel;
            TimeSliderViewModel = timeSliderViewModel;
            dataStore.PropertyChanged += OnDataStoreChanged;
            filtersViewModel.PropertyChanged += OnFiltersChanged;

            // Comando para cerrar el panel de detalles (llama al método existente)
            HideDetailsCommand = new RelayCommand((object? _) =>
            {
                HideGraphicDetails();
            });

            ShowMoreInfoCommand = new RelayCommand((_) =>
            {
                IsMoreInfoVisible = true;
            });

            HideMoreInfoCommand = new RelayCommand((_) =>
            {
                IsMoreInfoVisible = false;
            });



            SetupMap();

            planeGraphics = new GraphicsOverlay();

            ConfigurePlaneRenderer();

            selectedOverlay = new GraphicsOverlay();

            var overlays = new GraphicsOverlayCollection { planeGraphics, selectedOverlay };
            GraphicsOverlays = overlays;

            DisplayFlights();
        }

        private async void ConfigurePlaneRenderer()
        {
            try
            {
                var dotSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.BlueViolet, 8);

                var msAvion = new MemoryStream();
                Properties.Resources.Avion.Save(msAvion, System.Drawing.Imaging.ImageFormat.Png);
                msAvion.Position = 0;
                var planeSymbol = await PictureMarkerSymbol.CreateAsync(msAvion);

                planeSymbol.Width = 25;
                planeSymbol.Height = 25;


                var msAntena = new MemoryStream();
                Properties.Resources.Antena.Save(msAntena, System.Drawing.Imaging.ImageFormat.Png);
                msAntena.Position = 0;
                var antennaSymbol = await PictureMarkerSymbol.CreateAsync(msAntena);

                antennaSymbol.Width = 25;
                antennaSymbol.Height = 25;

                var renderer = new UniqueValueRenderer();
                renderer.FieldNames.Add("RenderType");

                // CAT021 punto
                renderer.UniqueValues.Add(new UniqueValue("Punto", "CAT021", dotSymbol, "CAT021"));

                // CAT048 avión
                renderer.UniqueValues.Add(new UniqueValue("Avion", "CAT048", planeSymbol, "CAT048"));

                // Nuevo: Modo A 7777
                renderer.UniqueValues.Add(new UniqueValue("Antenna", "MODO3A7777", antennaSymbol, "MODO3A7777"));

                renderer.RotationExpression = "[Heading]";
                renderer.RotationType = RotationType.Geographic;

                planeGraphics.Renderer = renderer;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renderer: {ex.Message}");
            }
        }


        private bool _isMoreInfoVisible;
        public bool IsMoreInfoVisible
        {
            get => _isMoreInfoVisible;
            set { _isMoreInfoVisible = value; OnPropertyChanged(); }
        }

        private Visibility _botonVisibilidad = Visibility.Collapsed;

        public Visibility BotonVisibilidad
        {
            get { return _botonVisibilidad; }
            set
            {
                _botonVisibilidad = value;
                OnPropertyChanged(nameof(BotonVisibilidad));
            }
        }

        public ICommand ShowMoreInfoCommand { get; set; }

        public ICommand HideMoreInfoCommand { get; }
        private string _extendedGraphicInfo = string.Empty;
        public string ExtendedGraphicInfo
        {
            get => _extendedGraphicInfo;
            set { _extendedGraphicInfo = value; OnPropertyChanged(); }
        }


        private void OnDataStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AsterixViewer.DataStore.ReplayTime))
            {
                DisplayFlights();

                if (SelectedGraphic != null)
                    UpdateSelectedGraphicInfo();
            }
            else if (e.PropertyName == nameof(AsterixViewer.DataStore.Flights))
            {
                // recargar gráficos
                mapPoints = new Dictionary<string, Graphic>();
                planeGraphics.Graphics.Clear();
                DisplayFlights();
            }
        }

        private void OnFiltersChanged(object? sender, PropertyChangedEventArgs e)
        {
            DisplayFlights();
        }


        private void SetupMap()
        {
            var map = new Map(BasemapStyle.ArcGISDarkGray);
            var mapCenterPoint = new MapPoint(2.0670034, 41.2874084, SpatialReferences.Wgs84);
            map.InitialViewpoint = new Viewpoint(mapCenterPoint, 300000);
            Map = map;
        }


        private void DisplayFlights()
        {
            if (DataStore == null || DataStore.Flights == null) return;
            var visibleKeys = new HashSet<string>();

            foreach (var item in DataStore.Flights)
            {
                var flightId = item.Key;
                var flight = item.Value;

                // ---------------- PROCESAR CAT021 ----------------
                var msg021 = FindMessage(flight.cat21Messages, DataStore.ReplayTime);

                if (msg021 != null && filtersViewModel.FilterMessages(msg021))
                {
                    ProcessFlightGraphic(msg021, flightId.ToString(), "CAT021", visibleKeys);
                }

                // ---------------- PROCESAR CAT048 ----------------
                var msg048 = FindMessage(flight.cat48Messages, DataStore.ReplayTime);

                if (msg048 != null && filtersViewModel.FilterMessages(msg048))
                {
                    ProcessFlightGraphic(msg048, flightId.ToString(), "CAT048", visibleKeys);
                }
            }
            var toRemove = mapPoints.Keys
                .Where(k => !visibleKeys.Contains(k))
                .ToList();

            foreach (var key in toRemove)
            {
                if (mapPoints.TryGetValue(key, out var g))
                {
                    planeGraphics.Graphics.Remove(g);
                    mapPoints.Remove(key);
                }
            }

            if (SelectedGraphic != null && selectedHighlightGraphic != null)
                selectedHighlightGraphic.Geometry = SelectedGraphic.Geometry;
        }
        private void ProcessFlightGraphic(AsterixMessage msg, string flightId, string suffix, HashSet<string> visibleKeys)
        {
            if (msg == null) return;

            if (msg.Latitude.HasValue && msg.Longitude.HasValue && DataStore.ReplayTime - msg.TimeOfDay < 10)
            {
                string key = $"{flightId}_{suffix}";
                var pos = new MapPoint(msg.Longitude.Value, msg.Latitude.Value, SpatialReferences.Wgs84);
                visibleKeys.Add(key);

                double heading = msg.Heading ?? 0;
                int zIndex = (suffix == "CAT021") ? 100 : 0;

                string renderType = null;

                if (msg.Identification?.Length > 4)
                {
                    renderType = (msg.Mode3A == 4095 || msg.Identification.StartsWith("7777", StringComparison.Ordinal)) ? "MODO3A7777" : suffix;
                }

                if (mapPoints.TryGetValue(key, out var g))
                {
                    g.Geometry = pos;
                    g.Attributes["message"] = JsonSerializer.Serialize(msg);
                    g.Attributes["Heading"] = heading;
                    g.Attributes["RenderType"] = renderType;

                    g.ZIndex = zIndex;
                }
                else
                {
                    var graphic = new Graphic(pos);
                    graphic.Attributes["Key"] = key;
                    graphic.Attributes["message"] = JsonSerializer.Serialize(msg);
                    graphic.Attributes["Heading"] = heading;
                    graphic.Attributes["RenderType"] = renderType;

                    graphic.ZIndex = zIndex;

                    planeGraphics.Graphics.Add(graphic);
                    mapPoints[key] = graphic;
                }
            }
        }


        public void ShowGraphicDetails(Graphic graphic)
        {
            if (graphic == null)
            {
                HideGraphicDetails();
                return;
            }
            SelectedGraphic = graphic;
            // Crear o mover highlight
            if (selectedHighlightGraphic == null)
            {
                selectedHighlightGraphic = new Graphic(graphic.Geometry, highlightSymbol);
                selectedOverlay.Graphics.Add(selectedHighlightGraphic);
            }
            else
            {
                selectedHighlightGraphic.Geometry = graphic.Geometry;
            }

            UpdateSelectedGraphicInfo();
            IsInfoPanelVisible = true;
        }

        public void HideGraphicDetails()
        {
            if (selectedHighlightGraphic != null)
            {
                selectedOverlay.Graphics.Remove(selectedHighlightGraphic);
                selectedHighlightGraphic = null;
            }

            SelectedGraphic = null;
            SelectedGraphicInfo = string.Empty;
            IsInfoPanelVisible = false;
            IsMoreInfoVisible = false;
        }

        private void UpdateSelectedGraphicInfo()
        {
            if (SelectedGraphic == null || DataStore == null)
                return;


            if (SelectedGraphic.Attributes["message"] is not string msgString) return;
            var msg = JsonSerializer.Deserialize<AsterixMessage>(msgString);
            if (msg == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"Category: {msg.Cat}");
            sb.AppendLine($"Identification: {msg.Identification}");
            sb.AppendLine($"Time: {TimeSpan.FromSeconds(msg.TimeOfDay ?? 0):hh\\:mm\\:ss\\.fff}");
            sb.AppendLine($"Lat: {msg.Latitude:F6}");
            sb.AppendLine($"Lon: {msg.Longitude:F6}");
            string fl = "N/A";
            try
            {
                if (msg.FlightLevel != null && msg.FlightLevel.flightLevel.HasValue)
                    fl = (string) new FLConverter().Convert(msg.FlightLevel, typeof(String), new object(), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }

            string speed = msg.GS.HasValue ? msg.GS.Value.ToString("F1") : "N/A";
            string heading = msg.Heading.HasValue ? msg.Heading.Value.ToString("F1") : "N/A";

            sb.AppendLine($"FL: {fl}");
            sb.AppendLine($"Speed: {speed} kt");
            sb.AppendLine($"Heading: {heading} º");

            if (msg.Cat == CAT.CAT021)
            {
                BotonVisibilidad = Visibility.Collapsed;
            }
            else
            {
                BotonVisibilidad = Visibility.Visible;

                ShowMoreInfoCommand = new RelayCommand((_) =>
                {
                    IsMoreInfoVisible = true;
                });
            }
            SelectedGraphicInfo = sb.ToString();
            if (msg.Cat == CAT.CAT048)
            {
                var sbExt = new StringBuilder();
                sbExt.AppendLine($"🔎 Información extendida del vuelo {msg.Identification}");
                sbExt.AppendLine("------------------------------------------");
                sbExt.AppendLine($"Category: {msg.Cat}");
                sbExt.AppendLine($"Asterix SAC/SIC: {msg.SAC}/{msg.SIC}");
                sbExt.AppendLine($"Mode 3/A: {(msg.Mode3A.HasValue ? Convert.ToString(msg.Mode3A.Value, 8).PadLeft(4, '0') : "N/A")}");
                sbExt.AppendLine($"{msg.BDS?.BDSsTabla ?? "N/A"}");
                sbExt.AppendLine($"Baro: {msg.BDS?.BARO.ToString() ?? "N/A"}");
                sbExt.AppendLine($"IAS: {msg.BDS?.IAS?.ToString() ?? "N/A"}");

                ExtendedGraphicInfo = sbExt.ToString();
            }
        }

        private static AsterixMessage? FindMessage(List<AsterixMessage> messages, double time)
        {
            int index = messages.BinarySearch(
                new AsterixMessage { TimeOfDay = time },
                Comparer<AsterixMessage>.Create((a, b) =>
                {
                    if (a.TimeOfDay == null && b.TimeOfDay == null) return 0;
                    if (a.TimeOfDay == null) return -1;
                    if (b.TimeOfDay == null) return 1;
                    return a.TimeOfDay.Value.CompareTo(b.TimeOfDay.Value);
                })
            );

            AsterixMessage msg;
            if (index >= 0)
                msg = messages[index];
            else
            {
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
