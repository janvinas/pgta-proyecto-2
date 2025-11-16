using AsterixParser;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        private readonly DataStore dataStore;
        public TimeSliderViewModel TimeSliderViewModel { get; }
        public string ReplayTimeText => TimeSpan.FromSeconds(dataStore.ReplayTime).ToString(@"hh\:mm\:ss\.fff");
        private DispatcherTimer _replayTimer;
        private bool _isPlaying;
        private int _replaySpeedMultiplier = 1;

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

        private readonly Symbol planeSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 4);

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
        private List<AsterixMessage>? _selectedFlight;
        public List<AsterixMessage>? SelectedFlight
        {
            get => _selectedFlight;
            set
            {
                _selectedFlight = value;
                OnPropertyChanged();
            }
        }
        private CAT? _selectedFlightCat;
        public CAT? SelectedFlightCat
        {
            get => _selectedFlightCat;
            set
            {
                _selectedFlightCat = value;
                OnPropertyChanged();
            }
        }


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

        public string PlayPauseButtonText => IsPlaying ? "Pause" : "Play";
        public string ReplaySpeedText => $"x{_replaySpeedMultiplier}";
        private Dictionary<string, Graphic> mapPoints = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MapViewModel(DataStore dataStore)
        {
            this.dataStore = dataStore;
            TimeSliderViewModel = new TimeSliderViewModel(dataStore);
            dataStore.PropertyChanged += OnDataStoreChanged;

            ChangeTimeCommand = new RelayCommand((object? args) =>
            {
                if (args is string timeString && int.TryParse(timeString, out int time))
                {
                    dataStore.ReplayTime += time;
                    OnPropertyChanged(nameof(ReplayTimeText));
                }
            });
            // Comando para cerrar el panel de detalles (llama al método existente)
            HideDetailsCommand = new RelayCommand((object? _) =>
            {
                HideGraphicDetails();
            });

            PlayPauseCommand = new RelayCommand((object? args) =>
            {
                IsPlaying = !IsPlaying;
                if (IsPlaying) _replayTimer.Start();
                else _replayTimer.Stop();
            });

            ChangeSpeedCommand = new RelayCommand((object? args) =>
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
            selectedOverlay = new GraphicsOverlay();

            var overlays = new GraphicsOverlayCollection { planeGraphics, selectedOverlay };
            GraphicsOverlays = overlays;

            DisplayFlights();
        }
        private bool _isMoreInfoVisible;
        public bool IsMoreInfoVisible
        {
            get => _isMoreInfoVisible;
            set { _isMoreInfoVisible = value; OnPropertyChanged(); }
        }

        public ICommand ShowMoreInfoCommand { get; }
        public ICommand HideMoreInfoCommand { get; }
        private string _extendedGraphicInfo = string.Empty;
        public string ExtendedGraphicInfo
        {
            get => _extendedGraphicInfo;
            set { _extendedGraphicInfo = value; OnPropertyChanged(); }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // incrementa el dataStore; DataStore notificará y OnDataStoreChanged actualizará la UI
            dataStore.ReplayTime += 1;
            // por seguridad forzamos notificación local también:
            TimeSliderViewModel.OnPropertyChanged(nameof(TimeSliderViewModel.ReplayTime));
            OnPropertyChanged(nameof(ReplayTimeText));
        }


        private void OnDataStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStore.ReplayTime))
            {
                OnPropertyChanged(nameof(ReplayTimeText));
                DisplayFlights();

                if (SelectedGraphic != null)
                    UpdateSelectedGraphicInfo();
            }
            else if (e.PropertyName == nameof(DataStore.Flights))
            {
                // recargar gráficos
                mapPoints = new Dictionary<string, Graphic>();
                planeGraphics.Graphics.Clear();
                DisplayFlights();
            }
            else if (e.PropertyName == "FiltersRefreshed") // "FiltersRefreshed" es un ejemplo
            {
                DisplayFlights(); // Llama a DisplayFlights para aplicar el filtro
            }
        }


        private void SetupMap()
        {
            var map = new Map(BasemapStyle.ArcGISTopographic);
            var mapCenterPoint = new MapPoint(2.0670034, 41.2874084, SpatialReferences.Wgs84);
            map.InitialViewpoint = new Viewpoint(mapCenterPoint, 300000);
            Map = map;
        }

        // EN: MapViewModel.cs

        private void DisplayFlights()
        {
            if (dataStore == null || dataStore.Flights == null) return;

            // --- MODIFICACIÓN 1: Obtener el filtro global ---
            // Este predicado es el mismo que usa la tabla (el método FilterMessages).
            var globalFilter = dataStore.GlobalFilter;

            var visibleKeys = new HashSet<string>();

            foreach (var item in dataStore.Flights)
            {
                var flightId = item.Key;
                var messages = item.Value;

                var msg021 = FindMessage(messages.Where(m => m.Cat == CAT.CAT021).ToList(), dataStore.ReplayTime);

                // --- MODIFICACIÓN 2.A: Aplicar el filtro a msg021 ---
                // Comprobamos si el mensaje existe Y (si el filtro no es nulo O si pasa el filtro)
                if (msg021 != null && (globalFilter == null || globalFilter(msg021)))
                {
                    // El mensaje es válido Y ha pasado el filtro.
                    // Ahora solo comprobamos la antigüedad y si tiene coordenadas.
                    // Nota: La comprobación 'GBS != "Set"' se elimina de aquí porque
                    // ya está incluida en el 'globalFilter' (es el filtro 'EliminarSuelo').
                    if (msg021.Latitude.HasValue && msg021.Longitude.HasValue && dataStore.ReplayTime - msg021.TimeOfDay < 10)
                    {
                        string key = $"{flightId}_CAT021";
                        var pos = new MapPoint(msg021.Longitude.Value, msg021.Latitude.Value, SpatialReferences.Wgs84);
                        visibleKeys.Add(key);

                        if (mapPoints.TryGetValue(key, out var g))
                        {
                            g.Geometry = pos;
                        }
                        else
                        {
                            var sym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Chocolate, 5);
                            var graphic = new Graphic(pos, sym);
                            graphic.Attributes["Key"] = key;
                            planeGraphics.Graphics.Add(graphic);
                            mapPoints[key] = graphic;
                        }
                    }
                }
                // --- FIN MODIFICACIÓN 2.A ---

                var msgOther = FindMessage(messages.Where(m => m.Cat != CAT.CAT021).ToList(), dataStore.ReplayTime);

                // --- MODIFICACIÓN 2.B: Aplicar el filtro a msgOther ---
                // Hacemos la misma comprobación para los otros tipos de mensajes
                if (msgOther != null && (globalFilter == null || globalFilter(msgOther)))
                {
                    // Pasa el filtro, ahora comprobar antigüedad y coordenadas
                    if (msgOther.Latitude.HasValue && msgOther.Longitude.HasValue && dataStore.ReplayTime - msgOther.TimeOfDay < 10)
                    {
                        string key = $"{flightId}_OTHER";
                        var pos = new MapPoint(msgOther.Longitude.Value, msgOther.Latitude.Value, SpatialReferences.Wgs84);
                        visibleKeys.Add(key);

                        if (mapPoints.TryGetValue(key, out var g))
                        {
                            g.Geometry = pos;
                        }
                        else
                        {
                            var sym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.BlueViolet, 5);
                            var graphic = new Graphic(pos, sym);
                            graphic.Attributes["Key"] = key;
                            planeGraphics.Graphics.Add(graphic);
                            mapPoints[key] = graphic;
                        }
                    }
                }
                // --- FIN MODIFICACIÓN 2.B ---
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

        public void ShowGraphicDetails(Graphic graphic)
        {
            if (graphic == null)
            {
                HideGraphicDetails();
                return;
            }

            SelectedGraphic = graphic;
            var targetPoint = (MapPoint)SelectedGraphic.Geometry;

            var match = dataStore.Flights
                .SelectMany(f => f.Value.Select(m => new { Flight = f.Key, Match = m }))
                .FirstOrDefault(x =>
                    Math.Abs(x.Match.Latitude.GetValueOrDefault() - targetPoint.Y) < 0.0001 &&
                    Math.Abs(x.Match.Longitude.GetValueOrDefault() - targetPoint.X) < 0.0001);

            if (match == null) return;
            SelectedFlight = dataStore.Flights[match.Flight];
            SelectedFlightCat = match.Match.Cat;

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
            SelectedFlight = null;
            SelectedFlightCat = null;
            SelectedGraphicInfo = string.Empty;
            IsInfoPanelVisible = false;
            IsMoreInfoVisible = false;
        }

        private void UpdateSelectedGraphicInfo()
        {
            if (SelectedGraphic == null || SelectedFlight == null || SelectedFlightCat == null || dataStore == null)
                return;

            // Identificar vuelo

            var msg = FindMessage(SelectedFlight.Where(m => m.Cat == SelectedFlightCat).ToList(), dataStore.ReplayTime);
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


            SelectedGraphicInfo = sb.ToString();
            if (msg.Cat == CAT.CAT048)
            {
                var sbExt = new StringBuilder();
                sbExt.AppendLine($"🔎 Información extendida del vuelo {msg.Identification}");
                sbExt.AppendLine("------------------------------------------");
                sbExt.AppendLine($"Category: {msg.Cat}");
                sbExt.AppendLine($"Asterix SAC/SIC: {msg.SAC}/{msg.SIC}");
                sbExt.AppendLine($"Mode 3/A: {msg.Mode3A?.ToString() ?? "N/A"}");
                sbExt.AppendLine($"{msg.BDS.BDSsTabla ?? "N/A"}");
                sbExt.AppendLine($"Baro: {msg.BDS.BARO.ToString() ?? "N/A"}");
                sbExt.AppendLine($"IAS: {msg.BDS.IAS.ToString() ?? "N/A"}");

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
