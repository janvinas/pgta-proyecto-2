using AsterixParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

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
            dataStore = DataContext as DataStore;
            if (dataStore == null || dataStore.Messages == null)
                return; // Evita el crash si aún no está inicializado

            view = CollectionViewSource.GetDefaultView(dataStore.Messages);
            if (view != null)
            {
                view.Filter = FilterMessages;
                DataGrid.ItemsSource = view;
            }
        }


        private bool FilterMessages(object obj)
        {
            if (obj is not AsterixMessage msg)
                return false;

            // --- FILTROS EXISTENTES ---
            if (msg.Cat == CAT.CAT021 && !(Cat021Filter?.IsChecked ?? true))
                return false;
            if (msg.Cat == CAT.CAT048 && !(Cat048Filter?.IsChecked ?? true))
                return false;
            if (msg.Mode3A == 4095 && !(TransponderFijoFilter?.IsChecked ?? true))
                return false;

            if (BlancoPuroFilter?.IsChecked ?? false)
            {
                if (msg.TargetReportDescriptor048 == null || msg.TargetReportDescriptor048.Count == 0)
                    return false;

                var first = msg.TargetReportDescriptor048[0];
                if (string.IsNullOrEmpty(first) ||
                    (first.IndexOf("PSR", StringComparison.OrdinalIgnoreCase) < 0 &&
                     first.IndexOf("SSR", StringComparison.OrdinalIgnoreCase) < 0))
                {
                    return false;
                }
            }

            // --- Filtro de coordenadas ---
            if (!string.IsNullOrWhiteSpace(LatMinBox.Text) &&
                !string.IsNullOrWhiteSpace(LatMaxBox.Text) &&
                !string.IsNullOrWhiteSpace(LonMinBox.Text) &&
                !string.IsNullOrWhiteSpace(LonMaxBox.Text) &&
                double.TryParse(LatMinBox.Text, out double latMin) &&
                double.TryParse(LatMaxBox.Text, out double latMax) &&
                double.TryParse(LonMinBox.Text, out double lonMin) &&
                double.TryParse(LonMaxBox.Text, out double lonMax))
            {
                if (msg.Latitude.HasValue && msg.Longitude.HasValue)
                {
                    if (msg.Latitude < latMin || msg.Latitude > latMax ||
                        msg.Longitude < lonMin || msg.Longitude > lonMax)
                        return false;
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
        }


        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (DataGrid.ItemsSource is ICollectionView view)
                view.Refresh();
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
                            "Cat,SIC,SAC,TimeOfDay,TargetReportDescriptor,Distance,Azimuth,Mode3A," +
                            "FL FlightLevel,FL GarbledCode,FL CodeNotValidated," +
                            "RPC SRL,RPC SSR,RPC SAM,RPC PRL,RPC PAM,RPC RPD,RPC APD,RPC SCO,RPC SCR,RPC RW,RPC AR," +
                            "Address,Identification,TrackNum,GS,Heading," +
                            "TS CNF,TS RAD,TS DOU,TS MAH,TS CDM,TS TRE,TS GHO,TS SUP,TS TCC," +
                            "I048230 COM,I048230 STAT,I048230 SI,I048230 MSSC,I048230 ARC,I048230 AIC,I048230 B1A,I048230 B1B," +
                            "Latitude,Longitude,BPS," +
                            "BDS BDSs,BDS statusMCP,BDS MCP,BDS statusFMS,BDS FMS,BDS statusBARO,BDS BARO,BDS infoMCP,BDS VNAV,BDS ALTflag," +
                            "BDS APPR,BDS statusTarget,BDS TargetALT,BDS statusROLL,BDS ROLL,BDS statusTTA,BDS TTA,BDS statusGS,BDS GS," +
                            "BDS statusTAR,BDS TAR,BDS statusTAS,BDS TAS,BDS statusMH,BDS MH,BDS statusIAS,BDS IAS,BDS statusMACH,BDS MACH," +
                            "BDS statusBAROV,BDS BAROV,BDS statusIVV,BDS IVV"
                        );

                        // 5️⃣ Escribir cada fila (aquí solo de ejemplo)
                        foreach (AsterixMessage message in messages)
                        {
                            writer.WriteLine($"{message.Cat}," +
                                $"{message.SIC}," +
                                $"{message.SAC}," +
                                $"{TimeSpan.FromSeconds(message.TimeOfDay ?? 0).ToString(@"hh\:mm\:ss\:fff")}," +
                                $"{(message.Cat == CAT.CAT021
                                        ? (message.TargetReportDescriptor021 != null ? string.Join(";", message.TargetReportDescriptor021).Replace(",", "+") : "")
                                        : (message.TargetReportDescriptor048 != null ? string.Join(";", message.TargetReportDescriptor048).Replace(",", "+") : "")
                                )}," +
                                $"{message.Distance}," +
                                $"{message.Azimuth}," +
                                $"{message.Mode3A}," +

                                // FlightLevel
                                $"{message.FlightLevel?.flightLevel}," +
                                $"{message.FlightLevel?.garbledCode}," +
                                $"{message.FlightLevel?.codeNotValidated}," +

                                // RadarPlotCharacteristics
                                $"{message.RadarPlotCharacteristics?.SRL}," +
                                $"{message.RadarPlotCharacteristics?.SSR}," +
                                $"{message.RadarPlotCharacteristics?.SAM}," +
                                $"{message.RadarPlotCharacteristics?.PRL}," +
                                $"{message.RadarPlotCharacteristics?.PAM}," +
                                $"{message.RadarPlotCharacteristics?.RPD}," +
                                $"{message.RadarPlotCharacteristics?.APD}," +
                                $"{message.RadarPlotCharacteristics?.SCO}," +
                                $"{message.RadarPlotCharacteristics?.SCR}," +
                                $"{message.RadarPlotCharacteristics?.RW}," +
                                $"{message.RadarPlotCharacteristics?.AR}," +

                                // Campos principales
                                $"{message.Address}," +
                                $"{message.Identification}," +
                                $"{message.TrackNum}," +
                                $"{message.GS}," +
                                $"{message.Heading}," +

                                // TrackStatus
                                $"{message.TrackStatus?.CNF}," +
                                $"{message.TrackStatus?.RAD}," +
                                $"{message.TrackStatus?.DOU}," +
                                $"{message.TrackStatus?.MAH}," +
                                $"{message.TrackStatus?.CDM}," +
                                $"{message.TrackStatus?.TRE}," +
                                $"{message.TrackStatus?.GHO}," +
                                $"{message.TrackStatus?.SUP}," +
                                $"{message.TrackStatus?.TCC}," +

                                // I048230
                                $"{(message.I048230?.COM ?? "").Replace(",", "+")}," +
                                $"{(message.I048230?.STAT ?? "").Replace(", ", " + ")}," +
                                $"{message.I048230?.SI}," +
                                $"{message.I048230?.MSSC}," +
                                $"{message.I048230?.ARC}," +
                                $"{message.I048230?.AIC}," +
                                $"{message.I048230?.B1A}," +
                                $"{message.I048230?.B1B}," +

                                // Coordenadas y velocidad
                                $"{message.Latitude}," +
                                $"{message.Longitude}," +
                                $"{message.BPS}," +

                                // BDS
                                $"{message.BDS?.BDSsCSV}," +
                                $"{message.BDS?.statusMCP}," +
                                $"{message.BDS?.MCP}," +
                                $"{message.BDS?.statusFMS}," +
                                $"{message.BDS?.FMS}," +
                                $"{message.BDS?.statusBARO}," +
                                $"{message.BDS?.BARO}," +
                                $"{message.BDS?.infoMCP}," +
                                $"{message.BDS?.VNAV}," +
                                $"{message.BDS?.ALTflag}," +
                                $"{message.BDS?.APPR}," +
                                $"{message.BDS?.statusTarget}," +
                                $"{message.BDS?.TargetALT}," +
                                $"{message.BDS?.statusROLL}," +
                                $"{message.BDS?.ROLL}," +
                                $"{message.BDS?.statusTTA}," +
                                $"{message.BDS?.TTA}," +
                                $"{message.BDS?.statusGS}," +
                                $"{message.BDS?.GS}," +
                                $"{message.BDS?.statusTAR}," +
                                $"{message.BDS?.TAR}," +
                                $"{message.BDS?.statusTAS}," +
                                $"{message.BDS?.TAS}," +
                                $"{message.BDS?.statusMH}," +
                                $"{message.BDS?.MH}," +
                                $"{message.BDS?.statusIAS}," +
                                $"{message.BDS?.IAS}," +
                                $"{message.BDS?.statusMACH}," +
                                $"{message.BDS?.MACH}," +
                                $"{message.BDS?.statusBAROV}," +
                                $"{message.BDS?.BAROV}," +
                                $"{message.BDS?.statusIVV}," +
                                $"{message.BDS?.IVV},"
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


    }
}
