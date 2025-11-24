using AsterixParser;
using AsterixParser.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsterixViewer.Tabs
{
    public partial class TableTab : UserControl
    {
        private DataStore? dataStore;
        private ICollectionView? view;

        public TableTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            dataStore = ((App)Application.Current).DataStore;
            if (dataStore == null || dataStore.Messages == null)
                return; // Evita el crash si aún no está inicializado

            view = CollectionViewSource.GetDefaultView(dataStore.Messages);
            if (view != null)
            {
                dataStore.GlobalFilter = FilterMessages;
                view.Filter = dataStore.GlobalFilter;
                DataGrid.ItemsSource = view;
            }
        }

        private bool FilterMessages(object obj)
        {
            if (obj is not AsterixMessage msg)
                return false;
            if (msg.TimeOfDay == null)
            {
                return false;
            }
            if (msg.Cat == CAT.CAT021 && !(Cat021Filter?.IsChecked ?? true))
            {
                return false;
            }
            if (msg.Cat == CAT.CAT048 && !(Cat048Filter?.IsChecked ?? true))
            {
                return false;
            }
            if (msg.Mode3A == 4095 && !(TransponderFijoFilter?.IsChecked ?? true))
            {
                return false;
            }
            if (msg.targetReportDescriptor021?.GBS == "Set" && (EliminarSuelo?.IsChecked ?? false))
            {
                return false;
            }
            if(msg.I048230?.OnGround == true && (EliminarSuelo?.IsChecked ?? false))
            {
                return false;
            }
            

            if (BlancoPuroFilter?.IsChecked ?? false)
            {
                if (msg.TargetReportDescriptor048 == null || msg.TargetReportDescriptor048.Count == 0)
                {
                    return false;
                }

                var first = msg.TargetReportDescriptor048[0];
                if (string.IsNullOrEmpty(first) ||
                    (first.IndexOf("PSR", StringComparison.OrdinalIgnoreCase) < 0 &&
                     first.IndexOf("SSR", StringComparison.OrdinalIgnoreCase) < 0))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(IdentFilterBox.Text))
            {
                if (msg.Identification == null ||
                    !msg.Identification.Contains(IdentFilterBox.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }


            var c = CultureInfo.GetCultureInfo("es-ES");

            if (!string.IsNullOrWhiteSpace(LatMinBox.Text) &&
                !string.IsNullOrWhiteSpace(LatMaxBox.Text) &&
                !string.IsNullOrWhiteSpace(LonMinBox.Text) &&
                !string.IsNullOrWhiteSpace(LonMaxBox.Text) &&
                double.TryParse(LatMinBox.Text, NumberStyles.Any, c, out double latMin) &&
                double.TryParse(LatMaxBox.Text, NumberStyles.Any, c, out double latMax) &&
                double.TryParse(LonMinBox.Text, NumberStyles.Any, c, out double lonMin) &&
                double.TryParse(LonMaxBox.Text, NumberStyles.Any, c, out double lonMax))
            {
                if (msg.Latitude.HasValue && msg.Longitude.HasValue)
                {
                    if (msg.Latitude < latMin || msg.Latitude > latMax ||
                        msg.Longitude < lonMin || msg.Longitude > lonMax)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void OnClearCoordFilter(object sender, RoutedEventArgs e)
        {
            LatMinBox.Text = "";
            LatMaxBox.Text = "";
            LonMinBox.Text = "";
            LonMaxBox.Text = "";
            DataGrid.Items.Filter = FilterMessages;
            if (view != null)
            {
                view.Refresh();
            }
        }


        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (view != null)
            {
                view.Refresh();
            }
        }



        private void OnTRDClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var item = btn.DataContext;
                if (item is AsterixMessage message)
                {
                    if (message.TargetReportDescriptor048 == null)
                    {
                        MessageBox.Show("No data", "Target Report Descriptor");
                    }
                    else
                    {
                        MessageBox.Show(string.Join("\n", message.TargetReportDescriptor048), "Target Report Descriptor");
                    }
                }
            }
        }

        private void Exportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1️⃣ Obtener los mensajes filtrados
                IEnumerable<AsterixMessage> messages = dataStore.Messages.Where(FilterMessages);

                // 2️⃣ Crear el cuadro de diálogo para elegir ruta y nombre de archivo
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de mensajes ASTERIX",
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = "AsterixExport.csv",
                    DefaultExt = ".csv"
                };

                // 3️⃣ Mostrar el cuadro de diálogo
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // 4️⃣ Escribir el CSV
                    using (var writer = new StreamWriter(filePath))
                    {
                        // Encabezado CSV
                        writer.WriteLine(
                            "Cat;SIC;SAC;TimeOfDay;" +  // Initial info
                            "LAT;LON;FL/Altitude;" +    // LLA COORDS
                            "TargetReportDescriptor;Distance;Azimuth;Mode3A;" +
                            "FL GarbledCode;FL CodeNotValidated;" +
                            "RPC SRL;RPC SSR;RPC SAM;RPC PRL;RPC PAM;RPC RPD;RPC APD;RPC SCO;RPC SCR;RPC RW;RPC AR;" +
                            "Address;Identification;TrackNum;GS;Heading;" +
                            "TS CNF;TS RAD;TS DOU;TS MAH;TS CDM;TS TRE;TS GHO;TS SUP;TS TCC;" +
                            "I048230 COM;I048230 STAT;I048230 SI;I048230 MSSC;I048230 ARC;I048230 AIC;I048230 B1A;I048230 B1B;" +
                            "BPS;" +
                            "BDS BDSs;BDS statusMCP;BDS MCP;BDS statusFMS;BDS FMS;BDS statusBARO;BDS BARO;BDS infoMCP;BDS VNAV;BDS ALTflag;" +
                            "BDS APPR;BDS statusTarget;BDS TargetALT;BDS statusROLL;BDS ROLL;BDS statusTTA;BDS TTA;BDS statusGS;BDS GS;" +
                            "BDS statusTAR;BDS TAR;BDS statusTAS;BDS TAS;BDS statusMH;BDS MH;BDS statusIAS;BDS IAS;BDS statusMACH;BDS MACH;" +
                            "BDS statusBAROV;BDS BAROV;BDS statusIVV;BDS IVV"
                        );

                        // 5️⃣ Escribir cada fila (aquí solo de ejemplo)
                        foreach (AsterixMessage message in messages)
                        {
                            writer.WriteLine($"{message.Cat};" +
                                $"{message.SIC};" +
                                $"{message.SAC};" +
                                $"{TimeSpan.FromSeconds(message.TimeOfDay ?? 0).ToString(@"hh\:mm\:ss\:fff")};" +

                                // LLA COORDINATES positions: [4;5;6]=[LAT;LON;Alt]
                                $"{message.Latitude};" +
                                $"{message.Longitude};" +
                                $"{message.FlightLevel?.flightLevel};" +

                                // Report, Distance, Azimuth and Mode3A
                                $"{(message.Cat == CAT.CAT021
                                        ? (message.TargetReportDescriptor021 != null ? string.Join(",", message.TargetReportDescriptor021): "")
                                        : (message.TargetReportDescriptor048 != null ? string.Join(",", message.TargetReportDescriptor048): "")
                                )};" +
                                $"{message.Distance};" +
                                $"{message.Azimuth};" +
                                $"{message.Mode3A};" +

                                // FlightLevel other info
                                $"{message.FlightLevel?.garbledCode};" +
                                $"{message.FlightLevel?.codeNotValidated};" +

                                // RadarPlotCharacteristics
                                $"{message.RadarPlotCharacteristics?.SRL};" +
                                $"{message.RadarPlotCharacteristics?.SSR};" +
                                $"{message.RadarPlotCharacteristics?.SAM};" +
                                $"{message.RadarPlotCharacteristics?.PRL};" +
                                $"{message.RadarPlotCharacteristics?.PAM};" +
                                $"{message.RadarPlotCharacteristics?.RPD};" +
                                $"{message.RadarPlotCharacteristics?.APD};" +
                                $"{message.RadarPlotCharacteristics?.SCO};" +
                                $"{message.RadarPlotCharacteristics?.SCR};" +
                                $"{message.RadarPlotCharacteristics?.RW};" +
                                $"{message.RadarPlotCharacteristics?.AR};" +

                                // Campos principales
                                $"{message.Address};" +
                                $"{message.Identification};" +
                                $"{message.TrackNum};" +
                                $"{message.GS};" +
                                $"{message.Heading};" +

                                // TrackStatus
                                $"{message.TrackStatus?.CNF};" +
                                $"{message.TrackStatus?.RAD};" +
                                $"{message.TrackStatus?.DOU};" +
                                $"{message.TrackStatus?.MAH};" +
                                $"{message.TrackStatus?.CDM};" +
                                $"{message.TrackStatus?.TRE};" +
                                $"{message.TrackStatus?.GHO};" +
                                $"{message.TrackStatus?.SUP};" +
                                $"{message.TrackStatus?.TCC};" +

                                // I048230
                                $"{message.I048230?.COM ?? ""};" +
                                $"{message.I048230?.STAT ?? ""};" +
                                $"{message.I048230?.SI};" +
                                $"{message.I048230?.MSSC};" +
                                $"{message.I048230?.ARC};" +
                                $"{message.I048230?.AIC};" +
                                $"{message.I048230?.B1A};" +
                                $"{message.I048230?.B1B};" +

                                // Velocidad
                                $"{message.BPS};" +

                                // BDS
                                $"{message.BDS?.BDSsCSV};" +
                                $"{message.BDS?.statusMCP};" +
                                $"{message.BDS?.MCP};" +
                                $"{message.BDS?.statusFMS};" +
                                $"{message.BDS?.FMS};" +
                                $"{message.BDS?.statusBARO};" +
                                $"{message.BDS?.BARO};" +
                                $"{message.BDS?.infoMCP};" +
                                $"{message.BDS?.VNAV};" +
                                $"{message.BDS?.ALTflag};" +
                                $"{message.BDS?.APPR};" +
                                $"{message.BDS?.statusTarget};" +
                                $"{message.BDS?.TargetALT};" +
                                $"{message.BDS?.statusROLL};" +
                                $"{message.BDS?.ROLL};" +
                                $"{message.BDS?.statusTTA};" +
                                $"{message.BDS?.TTA};" +
                                $"{message.BDS?.statusGS};" +
                                $"{message.BDS?.GS};" +
                                $"{message.BDS?.statusTAR};" +
                                $"{message.BDS?.TAR};" +
                                $"{message.BDS?.statusTAS};" +
                                $"{message.BDS?.TAS};" +
                                $"{message.BDS?.statusMH};" +
                                $"{message.BDS?.MH};" +
                                $"{message.BDS?.statusIAS};" +
                                $"{message.BDS?.IAS};" +
                                $"{message.BDS?.statusMACH};" +
                                $"{message.BDS?.MACH};" +
                                $"{message.BDS?.statusBAROV};" +
                                $"{message.BDS?.BAROV};" +
                                $"{message.BDS?.statusIVV};" +
                                $"{message.BDS?.IVV};"
                            );
                        }
                    }

                    // 6️⃣ Confirmar al usuario
                    MessageBox.Show(
                        $"Archivo exportado correctamente:\n{filePath}",
                        "Exportación completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show("Exportación cancelada por el usuario.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

        }
        private bool FilterMessagesP3(object obj)
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
            if (msg.I048230?.OnGround ?? false && (EliminarSuelo?.IsChecked ?? false))
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
        private void ExportarP3_Click(object sender, RoutedEventArgs e)
        {
            if (dataStore == null) return;

            try
            {
                var messages = dataStore.Messages.Where(FilterMessagesP3);

                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de mensajes ASTERIX",
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = "AsterixExportP3.csv",
                    DefaultExt = ".csv"
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result == null || !result.Value) return;

                using (var writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine("CAT;SAC;SIC;Time;LAT;LON;H(m);H(ft);RHO;THETA;Mode3/A;FL;TA;TI;BP;RA;TTA;GS;TAR;TAS;HDG;IAS;MACH;BAR;IVV;TN;GS(kt);HDG;STAT");

                    var c = CultureInfo.GetCultureInfo("es-ES");

                    foreach (AsterixMessage message in messages)
                    {
                        if (message.FlightLevel?.flightLevel * 100 <= 6000f)
                        {
                            string line =
                                $"{message.Cat};" +
                                $"{message.SAC};" +
                                $"{message.SIC};" +
                                $"{TimeSpan.FromSeconds(message.TimeOfDay ?? 0):hh\\:mm\\:ss\\:fff};" +
                                $"{message.Latitude?.ToString(c) ?? "N/A"};" +
                                $"{message.Longitude?.ToString(c) ?? "N/A"};" +
                                $"{(message.FlightLevel != null ? (message.FlightLevel.flightLevel * 100 * GeoUtils.FEET2METERS)?.ToString(c) : "N/A")};" +
                                $"{(message.FlightLevel != null ? (message.FlightLevel.flightLevel * 100)?.ToString(c) : "N/A")};" +
                                $"{message.Distance?.ToString() ?? "N/A"};" +
                                $"{message.Azimuth?.ToString() ?? "N/A"};" +
                                $"{(message.Mode3A != null ? Convert.ToString(message.Mode3A.Value, 8) : "N/A")};" +
                                $"{message.FlightLevel?.flightLevel?.ToString(c) ?? "N/A"};" +
                                $"{message.Address?.ToString("X6") ?? "N/A"};" +
                                $"{message.Identification ?? "N/A"};" +
                                $"{message.BDS?.BARO?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.ROLL?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TTA?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.GS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TAR?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.MH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.IAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.MACH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.BAROV?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.IVV?.ToString(c) ?? "N/A"};" +
                                $"{message.TrackNum?.ToString(c) ?? "N/A"};" +
                                $"{message.GS?.ToString(c) ?? "N/A"};" +
                                $"{message.Heading?.ToString(c) ?? "N/A"};" +
                                $"{message.I048230?.STAT?.ToString() ?? "N/A"}";

                            writer.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
