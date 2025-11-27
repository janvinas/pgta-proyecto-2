using AsterixParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Transactions;

namespace AsterixViewer
{
    public class FiltersViewModel : INotifyPropertyChanged
    {
        private CultureInfo c = CultureInfo.GetCultureInfo("es-ES");

        private bool _showCat21 = true;
        private bool _showCat48 = true;
        private bool _showBlancoPuro = true;
        private bool _showTransponderFijo = false;
        private bool _showOnGround = false;
        private double? _minLatitude = 40.9;
        private double? _maxLatitude = 41.7;
        private double? _minLongitude = 1.5;
        private double? _maxLongitude = 2.6;
        private string? _idFilter = null;

        public bool ShowCat21
        {
            get => _showCat21;
            set { _showCat21 = value; OnChanged(); OnFiltersChanged(); }
        }

        public bool ShowCat48
        {
            get => _showCat48;
            set { _showCat48 = value; OnChanged(); OnFiltersChanged(); }
        }

        public bool ShowBlancoPuro
        {
            get => _showBlancoPuro;
            set { _showBlancoPuro = value; OnChanged(); OnFiltersChanged(); }
        }

        public bool ShowTransponderFijo
        {
            get => _showTransponderFijo;
            set { _showTransponderFijo = value; OnChanged(); OnFiltersChanged(); }
        }

        public bool ShowOnGround
        {
            get => _showOnGround;
            set { _showOnGround = value; OnChanged(); OnFiltersChanged(); }
        }

        public string MinLatitude
        {
            get => _minLatitude?.ToString(c) ?? "";
            set
            {
                if (value == "")
                {
                    _minLatitude = null;
                }
                else if(double.TryParse(value, NumberStyles.Any, c, out double latMin))
                {
                    _minLatitude = latMin;
                }
                else
                {
                    _minLatitude = null;
                }
                OnChanged(); OnFiltersChanged();
            }
        }

        public string MaxLatitude
        {
            get => _maxLatitude?.ToString(c) ?? "";
            set
            {
                if (value == "")
                {
                    _maxLatitude = null;
                }
                else if (double.TryParse(value, NumberStyles.Any, c, out double latMax))
                {
                    _maxLatitude = latMax;
                }
                else
                {
                    _maxLatitude = null;
                }
                OnChanged(); OnFiltersChanged();
            }
        }

        public string MinLongitude
        {
            get => _minLongitude?.ToString(c) ?? "";
            set
            {
                if (value == "")
                {
                    _minLongitude = null;
                }
                else if (double.TryParse(value, NumberStyles.Any, c, out double lonMin))
                {
                    _minLongitude = lonMin;
                }
                else
                {
                    _minLongitude = null;
                }
                OnChanged(); OnFiltersChanged();
            }
        }

        public string MaxLongitude
        {
            get => _maxLongitude?.ToString(c) ?? "";
            set
            {
                if (value == "")
                {
                    _maxLongitude = null;
                }
                else if (double.TryParse(value, NumberStyles.Any, c, out double lonMax))
                {
                    _maxLongitude = lonMax;
                }
                else
                {
                    _maxLongitude = null;
                }
                OnChanged(); OnFiltersChanged();
            }
        }

        public string IdFilter
        {
            get => _idFilter ?? "";
            set
            {
                if (value == "")
                {
                    _idFilter = null;
                }
                else
                {
                    _idFilter = value;
                }
                OnChanged(); OnFiltersChanged();
            }
        }

        public bool FilterMessages(object obj)
        {
            if (obj is not AsterixMessage msg)
                return false;
            if (msg.TimeOfDay == null)
            {
                return false;
            }
            if (msg.Cat == CAT.CAT021 && !_showCat21)
            {
                return false;
            }
            if (msg.Cat == CAT.CAT048 && !_showCat48)
            {
                return false;
            }

            if (!_showTransponderFijo)
            {
                if (msg.Mode3A == 4095)
                {
                    return false;
                }
                if (msg.Identification?.Length >= 4 && msg.Identification.StartsWith("7777", StringComparison.Ordinal))
                {
                    return false;
                }
            }
            if (msg.targetReportDescriptor021?.GBS == "Set" && !_showOnGround)
            {
                return false;
            }
            if (msg.I048230?.OnGround == true && !_showOnGround && msg.Mode3A != 4095)
            {
                return false;
            }


            if (!_showBlancoPuro)
            {
                var first = msg.TargetReportDescriptor048?[0];
                if (first?.IndexOf("ModeS", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false; 
                }
            }

            if (_idFilter is string id)
            {
                if (msg.Identification == null ||
                    !msg.Identification.Contains(id, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }


            var c = CultureInfo.GetCultureInfo("es-ES");

            if (_minLatitude != null &&
                _maxLatitude != null &&
                _minLongitude != null &&
                _maxLongitude != null)
            {
                if (msg.Latitude.HasValue && msg.Longitude.HasValue)
                {
                    if (msg.Latitude < _minLatitude || msg.Latitude > _maxLatitude ||
                        msg.Longitude < _minLongitude || msg.Longitude > _maxLongitude)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool FilterMessagesP3(object obj)
        {
            if (obj is not AsterixMessage msg)
                return false;

            if (msg.Cat == CAT.CAT021)
            {
                return false;
            }
            if (msg.Mode3A == 4095)
            {
                return false;
            }
            if (msg.BDS?.IAS == 0)
            {
                return false;
            }
            if (msg.I048230?.OnGround ?? false)
            {
                return false;
            }
            if (msg.I048230?.STAT == null)
            {
                return false;
            }

            var c = CultureInfo.GetCultureInfo("es-ES");

            float latMin = 40.9f;
            float latMax = 41.7f;
            float lonMin = 1.5f;
            float lonMax = 2.6f;
            if (msg.Latitude.HasValue && msg.Longitude.HasValue)
            {
                if (msg.Latitude < latMin || msg.Latitude > latMax ||
                    msg.Longitude < lonMin || msg.Longitude > lonMax && msg.Latitude != 0 && msg.Longitude != 0)
                {
                    return false;
                }
            }

            return true;
        }

        // Event that main VM can subscribe to
        public event Action? FiltersChanged;

        private void OnFiltersChanged() => FiltersChanged?.Invoke();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
