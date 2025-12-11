using AsterixParser.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static AsterixViewer.Tabs.Proyecto3;

namespace AsterixViewer.Projecte3
{
    internal class DistanciasSonometro
    {

        // Clase de distancias minimas de cada vuelo respecto al sonometro
        public class DistanciaMinimaSonometro
        {
            public Vuelo vuelo = new Vuelo();
            public double distMinSonometro;
            public string timeMinSonometro = "";
            public string Lat = "";
            public string Lon = "";
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        public List<DistanciaMinimaSonometro> CalcularDistanciaMinimaSonometro(List<List<string>> datosAsterix, List<Vuelo> vuelosOrdenados, CoordinatesUVH sonometro,
            List<DistanciaMinimaSonometro> listaDistanciasMinimasSonometro)
        {
            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;
            int colAST_time = 3;
            int colAST_lat = 4;
            int colAST_lon = 5;

            Point posSonometro = new Point(sonometro.U, sonometro.V);
            Point posVuelo;

            double dist;
            double distMin;
            string timeMin;

            int index = 0;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                if (vuelo.pistadesp == "LEBL-24L")
                {
                    posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[0][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[0][Ycol]));
                    distMin = CalcularDistanciaEntrePuntos(posVuelo, posSonometro);
                    timeMin = vuelo.mensajesVuelo[0][colAST_time];

                    for (int i = 1; i < vuelo.mensajesVuelo.Count; i++)
                    {
                        posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[i][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[i][Ycol]));
                        dist = CalcularDistanciaEntrePuntos(posVuelo, posSonometro);

                        if (dist < distMin)
                        {
                            distMin = dist;
                            timeMin = vuelo.mensajesVuelo[i][colAST_time];
                            index = i;
                        }
                    }

                    DistanciaMinimaSonometro distanciaMinima = new DistanciaMinimaSonometro();
                    distanciaMinima.vuelo = vuelo;
                    distanciaMinima.distMinSonometro = distMin;
                    distanciaMinima.timeMinSonometro = timeMin;
                    distanciaMinima.Lat = vuelo.mensajesVuelo[index][colAST_lat];
                    distanciaMinima.Lon = vuelo.mensajesVuelo[index][colAST_lon];

                    listaDistanciasMinimasSonometro.Add(distanciaMinima);
                }
            }

            return listaDistanciasMinimasSonometro;
        }

        public void GuardarDistMinSonometro(List<DistanciaMinimaSonometro> listaDistanciasMinimasSonometro)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Distancias Minimas Sonometro",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "MinDistance Sonometro.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        writer1.WriteLine("IdentificacionDESP;Callsign;ATOT;Time DEP 0,5NM THR;SID;Estela;Tipo Aeronave;Distancia Minima al Sonometro;" +
                            "Tiempo de Deteccion de Distancia Minima;Latitud;Longitud");
                        foreach (DistanciaMinimaSonometro distMinSonometro in listaDistanciasMinimasSonometro)
                        {
                            try
                            {
                                writer1.WriteLine(distMinSonometro.vuelo.identificadorDeparture + ";" + distMinSonometro.vuelo.codigoVuelo + ";" + distMinSonometro.vuelo.ATOT + ";" +
                                    distMinSonometro.vuelo.timeDEP_05NM + ";" + distMinSonometro.vuelo.sid + ";" + distMinSonometro.vuelo.estela + ";" +
                                    distMinSonometro.vuelo.tipo_aeronave + ";" + distMinSonometro.distMinSonometro / 1852 + ";" +
                                    distMinSonometro.timeMinSonometro + ";" + distMinSonometro.Lat + ";" + distMinSonometro.Lon);
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
