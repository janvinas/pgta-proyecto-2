using AsterixParser;
using AsterixParser.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsterixViewer
{
    /// <summary>
    /// Interaction logic for filters.xaml
    /// </summary>
    public partial class Filters : UserControl
    {

        private DataStore? dataStore;
        private FiltersViewModel? FiltersViewModel;

        public Filters()
        {
            InitializeComponent();
            dataStore = ((App)Application.Current).DataStore;
            FiltersViewModel = ((App)Application.Current).FiltersViewModel;
            DataContext = FiltersViewModel;
        }

        private void OnClearCoordFilter(object sender, RoutedEventArgs e)
        {
            LatMinBox.Text = "";
            LatMaxBox.Text = "";
            LonMinBox.Text = "";
            LonMaxBox.Text = "";

            var expression = LonMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LonMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
        }

        private void OnSetCoordsLebl(object sender, RoutedEventArgs e)
        {
            LatMinBox.Text = "40,9";
            LatMaxBox.Text = "41,7";
            LonMinBox.Text = "1,5";
            LonMaxBox.Text = "2,6";

            var expression = LonMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LonMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
        }

        private void OnIdFilterChanged(object sender, RoutedEventArgs e)
        {
            var expression = IdentFilterBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
        }

        private void OnCoordsFilterChanged(object sender, RoutedEventArgs e)
        {
            var expression = LonMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LonMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMaxBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            expression = LatMinBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
        }

        private void Exportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<AsterixMessage> messages = dataStore.Messages.Where(FiltersViewModel.FilterMessages);

                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de mensajes ASTERIX",
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = "AsterixExport.csv",
                    DefaultExt = ".csv"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var writer = new StreamWriter(filePath))
                    {
                        // Encabezado CSV
                        writer.WriteLine(
                            "Cat;SAC;SIC;TimeOfDay;" +  // Initial info
                            "LAT;LON;FL/Altitude;Altitude Corrected;" +    // LLA COORDS
                            "TargetReportDescriptor;RHO;THETA;Mode3A;" +
                            "FL GarbledCode;FL CodeNotValidated;" +
                            "RPC SRL;RPC SSR;RPC SAM;RPC PRL;RPC PAM;RPC RPD;RPC APD;RPC SCO;RPC SCR;RPC RW;RPC AR;" +
                            "TA;TI;TrackNum;GS;Heading;" +
                            "TS CNF;TS RAD;TS DOU;TS MAH;TS CDM;TS TRE;TS GHO;TS SUP;TS TCC;" +
                            "I048230 COM;I048230 STAT;I048230 SI;I048230 MSSC;I048230 ARC;I048230 AIC;I048230 B1A;I048230 B1B;" +
                            "BPS;" +
                            "BDS BDSs;BDS statusMCP;BDS MCP;BDS statusFMS;BDS FMS;BDS statusBARO;BP;BDS infoMCP;BDS VNAV;BDS ALTflag;" +
                            "BDS APPR;BDS statusTarget;BDS TargetALT;BDS statusROLL;RA;BDS statusTTA;TTA;BDS statusGS;GS;" +
                            "BDS statusTAR;TAR;BDS statusTAS;TAS;BDS statusMH;HDG;BDS statusIAS;IAS;BDS statusMACH;MACH;" +
                            "BDS statusBAROV;BAR;BDS statusIVV;IVV"
                        );

                        var c = CultureInfo.GetCultureInfo("es-ES");

                        // 5️⃣ Escribir cada fila (aquí solo de ejemplo)
                        foreach (AsterixMessage message in messages)
                        {
                            writer.WriteLine($"{message.Cat};" +
                                $"{message.SAC};" +
                                $"{message.SIC};" +
                                $"{TimeSpan.FromSeconds(message.TimeOfDay ?? 0).ToString(@"hh\:mm\:ss\:fff")};" +

                                // LLA COORDINATES positions: [4;5;6]=[LAT;LON;Alt]
                                $"{message.Latitude?.ToString(c) ?? "N/A"};" +
                                $"{message.Longitude?.ToString(c) ?? "N/A"};" +
                                $"{message.FlightLevel?.flightLevel?.ToString(c) ?? "N/A"};" +
                                $"{message.QNHcorrection?.ToString(c) ?? "N/A"};" +

                                // Report, Distance, Azimuth and Mode3A
                                $"{(message.Cat == CAT.CAT021
                                        ? (message.TargetReportDescriptor021 != null ? string.Join(",", message.TargetReportDescriptor021) : "")
                                        : (message.TargetReportDescriptor048 != null ? string.Join(",", message.TargetReportDescriptor048) : "")
                                )};" +
                                $"{message.Distance?.ToString(c) ?? "N/A"};" +
                                $"{message.Azimuth?.ToString(c) ?? "N/A"};" +
                                $"{(message.Mode3A != null ? Convert.ToString(message.Mode3A.Value, 8) : "N/A")};" +

                                // FlightLevel other info
                                $"{message.FlightLevel?.garbledCode};" +
                                $"{message.FlightLevel?.codeNotValidated};" +

                                // RadarPlotCharacteristics
                                $"{message.RadarPlotCharacteristics?.SRL?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.SSR?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.SAM?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.PRL?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.PAM?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.RPD?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.APD?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.SCO?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.SCR?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.RW?.ToString(c) ?? "N/A"};" +
                                $"{message.RadarPlotCharacteristics?.AR?.ToString(c) ?? "N/A"};" +

                                // Campos principales
                                $"{message.Address?.ToString("X6") ?? "N/A"};" +
                                $"{message.Identification ?? "N/A"};" +
                                $"{message.TrackNum?.ToString(c) ?? "N/A"};" +
                                $"{message.GS?.ToString(c) ?? "N/A"};" +
                                $"{message.Heading?.ToString(c) ?? "N/A"};" +

                                // TrackStatus
                                $"{message.TrackStatus?.CNF ?? "N/A"};" +
                                $"{message.TrackStatus?.RAD ?? "N/A"};" +
                                $"{message.TrackStatus?.DOU ?? "N/A"};" +
                                $"{message.TrackStatus?.MAH ?? "N/A"};" +
                                $"{message.TrackStatus?.CDM ?? "N/A"};" +
                                $"{message.TrackStatus?.TRE ?? "N/A"};" +
                                $"{message.TrackStatus?.GHO ?? "N/A"};" +
                                $"{message.TrackStatus?.SUP ?? "N/A"};" +
                                $"{message.TrackStatus?.TCC ?? "N/A"};" +

                                // I048230
                                $"{message.I048230?.COM ?? ""};" +
                                $"{message.I048230?.STAT ?? ""};" +
                                $"{message.I048230?.SI ?? "N/A"};" +
                                $"{message.I048230?.MSSC ?? "N/A"};" +
                                $"{message.I048230?.ARC ?? "N/A"};" +
                                $"{message.I048230?.AIC ?? "N/A"};" +
                                $"{message.I048230?.B1A?.ToString(c) ?? "N/A"};" +
                                $"{message.I048230?.B1B?.ToString(c) ?? "N/A"};" +

                                // Velocidad
                                $"{message.BPS?.ToString(c) ?? "N/A"};" +

                                // BDS
                                $"{message.BDS?.BDSsCSV ?? "N/A"};" +
                                $"{message.BDS?.statusMCP?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.MCP?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusFMS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.FMS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusBARO?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.BARO?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.infoMCP?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.VNAV?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.ALTflag?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.APPR?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusTarget?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TargetALT ?? "N/A"};" +
                                $"{message.BDS?.statusROLL?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.ROLL?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusTTA?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TTA?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusGS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.GS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusTAR?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TAR?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusTAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.TAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusMH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.MH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusIAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.IAS?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusMACH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.MACH?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusBAROV?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.BAROV?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.statusIVV?.ToString(c) ?? "N/A"};" +
                                $"{message.BDS?.IVV?.ToString(c) ?? "N/A"};"
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
        private void ExportarP3_Click(object sender, RoutedEventArgs e)
        {
            if (dataStore == null) return;

            try
            {
                var messages = dataStore.Messages.Where(FiltersViewModel.FilterMessagesP3);

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
                        // Verificamos condición de FlightLevel
                        if (message.FlightLevel?.flightLevel * 100 <= 6000f)
                        {
                            // He optimizado ligeramente la concatenación para que sea más legible y segura
                            string line =
                                $"{message.Cat};" +
                                $"{message.SAC};" +
                                $"{message.SIC};" +
                                $"{TimeSpan.FromSeconds(message.TimeOfDay ?? 0):hh\\:mm\\:ss\\:fff};" +
                                $"{message.Latitude?.ToString(c) ?? "N/A"};" +
                                $"{message.Longitude?.ToString(c) ?? "N/A"};" +
                                $"{(message.QNHcorrection != null ? (message.QNHcorrection * GeoUtils.FEET2METERS)?.ToString(c) : (message.FlightLevel?.flightLevel * 100 * GeoUtils.FEET2METERS)?.ToString(c))};" +
                                $"{(message.QNHcorrection != null ? (message.QNHcorrection?.ToString(c))?.ToString(c) : (message.FlightLevel?.flightLevel * 100)?.ToString(c))};" +
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


                // 6️⃣ Confirmar al usuario
                MessageBox.Show(
                    $"Archivo exportado correctamente:\n{saveFileDialog.FileName}",
                    "Exportación completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

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
