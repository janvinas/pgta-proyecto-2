using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static AsterixViewer.Tabs.Proyecto3;

namespace AsterixViewer.Projecte3
{
    internal class VelocidadesDespegue
    {
        public class DatosAltitud
        {
            public string IAS;
            public string IAScorrespondance;
            public string Time;
            public string Altura;

        }

        public class IASaltitudes
        {
            public Vuelo vuelo;
            public DatosAltitud data850ft = new DatosAltitud();
            public DatosAltitud data1500ft = new DatosAltitud();
            public DatosAltitud data3500ft = new DatosAltitud();
        }

        public List<IASaltitudes> CalcularVelocidadIASDespegue(List<Vuelo> vuelosOrdenados, List<IASaltitudes> listaVelocidadesIASDespegue)
        {
            int colHft = 7;
            int colIAS = 21;
            int colTIME = 3;

            int i;
            int j;
            int k;

            int valorIAS;
            double valorHft;
            double valorHft_prev;

            int target_altitude;
            int index;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                IASaltitudes iasAltitudes = new IASaltitudes();
                iasAltitudes.vuelo = vuelo;

                // 850ft
                target_altitude = 850;
                for (i = 1; i < vuelo.mensajesVuelo.Count; i++)
                {
                    valorIAS = int.TryParse(vuelo.mensajesVuelo[i][colIAS], out int tmp) ? tmp : 0;
                    valorHft = Convert.ToDouble(vuelo.mensajesVuelo[i][colHft]);
                    valorHft_prev = Convert.ToDouble(vuelo.mensajesVuelo[i - 1][colHft]);

                    if (valorHft > 1500) break;
                    if (valorHft - target_altitude > 0)
                    {
                        if (Math.Abs(valorHft_prev - target_altitude) < Math.Abs(valorHft - target_altitude)) index = i - 1;
                        else index = i;

                        if (vuelo.mensajesVuelo[index][colIAS] != "N/A")
                        {
                            iasAltitudes.data850ft.IAS = vuelo.mensajesVuelo[index][colIAS];
                            iasAltitudes.data850ft.IAScorrespondance = "0";
                        }
                        else
                        {
                            // Todo esto es para mirar donde queda la IAS más cercana no "N/A"
                            int max = vuelo.mensajesVuelo.Count;
                            int maxOffset = Math.Min(index, max - index);

                            for (int offset = 1; offset <= maxOffset; offset++)
                            {
                                // mirar hacia adelante
                                int adelante = index + offset;
                                if (vuelo.mensajesVuelo[adelante][colIAS] != "N/A")
                                {
                                    iasAltitudes.data850ft.IAS = vuelo.mensajesVuelo[adelante][colIAS];
                                    iasAltitudes.data850ft.IAScorrespondance = $"+{offset}";
                                    break;
                                }

                                // mirar hacia atrás
                                int atras = index - offset;
                                if (vuelo.mensajesVuelo[atras][colIAS] != "N/A")
                                {
                                    iasAltitudes.data850ft.IAS = vuelo.mensajesVuelo[atras][colIAS];
                                    iasAltitudes.data850ft.IAScorrespondance = $"-{offset}";
                                    break;
                                }
                            }
                        }

                        iasAltitudes.data850ft.Time = vuelo.mensajesVuelo[index][colTIME];
                        iasAltitudes.data850ft.Altura = vuelo.mensajesVuelo[index][colHft];

                        break;
                    }
                }

                // 1500ft
                target_altitude = 1500;
                for (j = i; j < vuelo.mensajesVuelo.Count; j++)
                {
                    valorIAS = int.TryParse(vuelo.mensajesVuelo[j][colIAS], out int tmp) ? tmp : 0;
                    valorHft = Convert.ToDouble(vuelo.mensajesVuelo[j][colHft]);
                    valorHft_prev = Convert.ToDouble(vuelo.mensajesVuelo[j - 1][colHft]);

                    if (valorHft > 3500) break;
                    if (valorHft - target_altitude > 0)
                    {
                        if (Math.Abs(valorHft_prev - target_altitude) < Math.Abs(valorHft - target_altitude)) index = j - 1;
                        else index = j;

                        if (vuelo.mensajesVuelo[index][colIAS] != "N/A")
                        {
                            iasAltitudes.data1500ft.IAS = vuelo.mensajesVuelo[index][colIAS];
                            iasAltitudes.data1500ft.IAScorrespondance = "0";
                        }
                        else
                        {
                            // Todo esto es para mirar donde queda la IAS más cercana no "N/A"
                            int max = vuelo.mensajesVuelo.Count;
                            int maxOffset = Math.Min(index, max - index);

                            for (int offset = 1; offset <= maxOffset; offset++)
                            {
                                // mirar hacia adelante
                                int adelante = index + offset;
                                if (vuelo.mensajesVuelo[adelante][colIAS] != "N/A")
                                {
                                    iasAltitudes.data1500ft.IAS = vuelo.mensajesVuelo[adelante][colIAS];
                                    iasAltitudes.data1500ft.IAScorrespondance = $"+{offset}";
                                    break;
                                }

                                // mirar hacia atrás
                                int atras = index - offset;
                                if (vuelo.mensajesVuelo[atras][colIAS] != "N/A")
                                {
                                    iasAltitudes.data1500ft.IAS = vuelo.mensajesVuelo[atras][colIAS];
                                    iasAltitudes.data1500ft.IAScorrespondance = $"-{offset}";
                                    break;
                                }
                            }
                        }

                        iasAltitudes.data1500ft.Time = vuelo.mensajesVuelo[index][colTIME];
                        iasAltitudes.data1500ft.Altura = vuelo.mensajesVuelo[index][colHft];

                        break;
                    }
                }

                // 3500ft
                target_altitude = 3500;
                for (k = j; k < vuelo.mensajesVuelo.Count; k++)
                {
                    valorIAS = int.TryParse(vuelo.mensajesVuelo[k][colIAS], out int tmp) ? tmp : 0;
                    valorHft = Convert.ToDouble(vuelo.mensajesVuelo[k][colHft]);
                    valorHft_prev = Convert.ToDouble(vuelo.mensajesVuelo[k - 1][colHft]);

                    if (valorHft - target_altitude > 0)
                    {
                        if (Math.Abs(valorHft_prev - target_altitude) < Math.Abs(valorHft - target_altitude)) index = k - 1;
                        else index = k;

                        if (vuelo.mensajesVuelo[index][colIAS] != "N/A")
                        {
                            iasAltitudes.data3500ft.IAS = vuelo.mensajesVuelo[index][colIAS];
                            iasAltitudes.data3500ft.IAScorrespondance = "0";
                        }
                        else
                        {
                            // Todo esto es para mirar donde queda la IAS más cercana no "N/A"
                            int max = vuelo.mensajesVuelo.Count;
                            int maxOffset = Math.Min(index, max - index);

                            for (int offset = 1; offset <= maxOffset; offset++)
                            {
                                // mirar hacia adelante
                                int adelante = index + offset;
                                if (vuelo.mensajesVuelo[adelante][colIAS] != "N/A")
                                {
                                    iasAltitudes.data3500ft.IAS = vuelo.mensajesVuelo[adelante][colIAS];
                                    iasAltitudes.data3500ft.IAScorrespondance = $"+{offset}";
                                    break;
                                }

                                // mirar hacia atrás
                                int atras = index - offset;
                                if (vuelo.mensajesVuelo[atras][colIAS] != "N/A")
                                {
                                    iasAltitudes.data3500ft.IAS = vuelo.mensajesVuelo[atras][colIAS];
                                    iasAltitudes.data3500ft.IAScorrespondance = $"-{offset}";
                                    break;
                                }
                            }
                        }

                        iasAltitudes.data3500ft.Time = vuelo.mensajesVuelo[index][colTIME];
                        iasAltitudes.data3500ft.Altura = vuelo.mensajesVuelo[index][colHft];

                        break;
                    }
                }

                listaVelocidadesIASDespegue.Add(iasAltitudes);
            }

            return listaVelocidadesIASDespegue;
        }

        public void GuardarVelocidadIASDespegue(List<IASaltitudes> listaVelocidadesIASDespegue)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Velocidades IAS Despegue",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "IAS TakeOff.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        writer1.WriteLine("IdentificacionDESP;Callsign;ATOT;SID;Estela;Tipo Aeronave;Runway;IAS850ft;time850ft;altitudTomada850ft;IAS1500ft;time1500ft;altitudTomada1500ft;IAS3500ft;time3500ft;altitudTomada3500ft;" +
                            "IAScorrespondance850ft;IAScorrespondance1500ft;IAScorrespondance3500ft");
                        foreach (IASaltitudes iasAltitudes in listaVelocidadesIASDespegue)
                        {
                            try
                            {
                                writer1.WriteLine(iasAltitudes.vuelo.identificadorDeparture + ";" + iasAltitudes.vuelo.codigoVuelo + ";" + iasAltitudes.vuelo.ATOT + ";" + iasAltitudes.vuelo.sid + ";" +
                                    iasAltitudes.vuelo.estela + ";" + iasAltitudes.vuelo.tipo_aeronave + ";" + iasAltitudes.vuelo.pistadesp + ";" +
                                    iasAltitudes.data850ft.IAS + ";" + iasAltitudes.data850ft.Time + ";" + iasAltitudes.data850ft.Altura + ";" +
                                    iasAltitudes.data1500ft.IAS + ";" + iasAltitudes.data1500ft.Time + ";" + iasAltitudes.data1500ft.Altura + ";" +
                                    iasAltitudes.data3500ft.IAS + ";" + iasAltitudes.data3500ft.Time + ";" + iasAltitudes.data3500ft.Altura + ";" + 
                                    iasAltitudes.data850ft.IAScorrespondance + ";" + iasAltitudes.data1500ft.IAScorrespondance + ";" + iasAltitudes.data3500ft.IAScorrespondance);
                            }
                            catch { }
                        }
                    }

                    // ✅ Confirmar al usuario
                    MessageBox.Show(
                        $"Archivo exportado correctamente:\n{filePath1}",
                        "Exportación completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
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
