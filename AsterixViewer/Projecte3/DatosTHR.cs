using AsterixParser.Utils;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Windows.UI.WebUI;
using static AsterixViewer.Projecte3.CalculosEstereograficos;
using static AsterixViewer.Tabs.Proyecto3;

namespace AsterixViewer.Projecte3
{
    public class THRAltitudVelocidad
    {
        public Vuelo vuelo;
        public string IAS;
        public string IAScorrespondance;
        public string altitud;
        public string time;
        public bool pasaPorTHR;
        public string lat;
        public string lon;
        public string distance2THR;
    }

    internal class DatosTHR
    {
        CoordinatesUVH THR_24L;
        CoordinatesUVH THR_24L_1b;
        CoordinatesUVH THR_24L_2c;
        CoordinatesUVH THR_24L_3b;
        CoordinatesUVH THR_24L_4c;

        CoordinatesUVH THR_06R;
        CoordinatesUVH THR_06R_1b;
        CoordinatesUVH THR_06R_2c;
        CoordinatesUVH THR_06R_3b;
        CoordinatesUVH THR_06R_4c;

        private CoordinatesUVH CalcularCoordenadasUVH(string coords_LatLon)
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            CoordinatesWGS84 coords_geodesic = GeoUtils.LatLonStringBoth2Radians(coords_LatLon, 8 * 0.3048);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            CoordinatesUVH coordsUVH = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            return coordsUVH;
        }

        private void CalcularPuntosTHR()
        {

            // Calcular Estereograficas de THR_24L (Hecho con Google Earth)
            string coordsTHR_24L = "41:17:31.99N 02:06:11.81E";
            string coordsTHR_24L_1b = "41:17:30.56N 02:06:04.59E";
            string coordsTHR_24L_2c = "41:17:35.47N 02:06:19.28E";
            string coordsTHR_24L_3b = "41:17:28.82N 02:06:05.64E";
            string coordsTHR_24L_4c = "41:17:33.24N 02:06:20.57E";

            THR_24L = CalcularCoordenadasUVH(coordsTHR_24L);
            THR_24L_1b = CalcularCoordenadasUVH(coordsTHR_24L_1b);
            THR_24L_2c = CalcularCoordenadasUVH(coordsTHR_24L_2c);
            THR_24L_3b = CalcularCoordenadasUVH(coordsTHR_24L_3b);
            THR_24L_4c = CalcularCoordenadasUVH(coordsTHR_24L_4c);

            // Calcular Estereograficas de THR_06R (Hecho con Google Earth)
            string coordsTHR_06R = "41:16:56.32N 02:04:27.66E";
            string coordsTHR_06R_1b = "41:16:55.15N 02:04:19.10E";
            string coordsTHR_06R_2c = "41:16:59.48N 02:04:33.91E";
            string coordsTHR_06R_3b = "41:16:52.33N 02:04:20.89E";
            string coordsTHR_06R_4c = "41:16:57.80N 02:04:35.08E";

            THR_06R = CalcularCoordenadasUVH(coordsTHR_06R);
            THR_06R_1b = CalcularCoordenadasUVH(coordsTHR_06R_1b);
            THR_06R_2c = CalcularCoordenadasUVH(coordsTHR_06R_2c);
            THR_06R_3b = CalcularCoordenadasUVH(coordsTHR_06R_3b);
            THR_06R_4c = CalcularCoordenadasUVH(coordsTHR_06R_4c);
        }

        bool PuntoEnZonaDeteccion(Point punto, List<Point> zona)
        {
            bool inside = false;
            int j = zona.Count - 1;

            for (int i = 0; i < zona.Count; i++)
            {
                if ((zona[i].Y > punto.Y) != (zona[j].Y > punto.Y) &&
                    punto.X < (zona[j].X - zona[i].X) * (punto.Y - zona[i].Y) /
                          (zona[j].Y - zona[i].Y) + zona[i].X)
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        private void Interpolar1segundo(List<Vuelo> vuelosOrdenados)
        {
            // Definimos columnas para mayor flexibilidad
            int colAST_time = 3;
            int colAST_lat = 4;
            int colAST_lon = 5;
            int colAST_altitudft = 7;
            int colAST_altitudm = 6;
            int colAST_IAS = 21;
            int colAST_IVVftpm = 24;
            int colAST_posx = -2; // penúltima posición
            int colAST_posy = -1; // última posición

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                var mensajesVueloInterpolados = new List<List<string>>();

                for (int i = 0; i < vuelo.mensajesVuelo.Count - 1; i++)
                {
                    // Valores iniciales y finales
                    double lat0 = double.Parse(vuelo.mensajesVuelo[i][colAST_lat]);
                    double lat4 = double.Parse(vuelo.mensajesVuelo[i + 1][colAST_lat]);

                    double lon0 = double.Parse(vuelo.mensajesVuelo[i][colAST_lon]);
                    double lon4 = double.Parse(vuelo.mensajesVuelo[i + 1][colAST_lon]);

                    double alt0 = double.Parse(vuelo.mensajesVuelo[i][colAST_altitudft]);
                    double alt1, alt2, alt3;

                    // Usamos IVV si está disponible
                    if (vuelo.mensajesVuelo[i][colAST_IVVftpm] != "N/A" && vuelo.mensajesVuelo[i][colAST_IVVftpm] != "NV")
                    {
                        double ivv = double.Parse(vuelo.mensajesVuelo[i][colAST_IVVftpm]);
                        alt1 = alt0 + ivv / 60.0;
                        alt2 = alt0 + ivv * 2.0 / 60.0;
                        alt3 = alt0 + ivv * 3.0 / 60.0;
                    }
                    else
                    {
                        double alt4 = double.Parse(vuelo.mensajesVuelo[i + 1][colAST_altitudft]);
                        alt1 = alt0 + (alt4 - alt0) * 0.25;
                        alt2 = alt0 + (alt4 - alt0) * 0.5;
                        alt3 = alt0 + (alt4 - alt0) * 0.75;
                    }

                    double ias0 = (vuelo.mensajesVuelo[i][colAST_IAS] != "N/A" && vuelo.mensajesVuelo[i][colAST_IAS] != "NV") ? double.Parse(vuelo.mensajesVuelo[i][colAST_IAS]) : double.NaN;
                    double ias4 = (vuelo.mensajesVuelo[i + 1][colAST_IAS] != "N/A" && vuelo.mensajesVuelo[i + 1][colAST_IAS] != "NV" ) ? double.Parse(vuelo.mensajesVuelo[i + 1][colAST_IAS]) : double.NaN;

                    double posx0 = double.Parse(vuelo.mensajesVuelo[i][vuelo.mensajesVuelo[i].Count + colAST_posx]);
                    double posx4 = double.Parse(vuelo.mensajesVuelo[i + 1][vuelo.mensajesVuelo[i + 1].Count + colAST_posx]);

                    double posy0 = double.Parse(vuelo.mensajesVuelo[i][vuelo.mensajesVuelo[i].Count + colAST_posy]);
                    double posy4 = double.Parse(vuelo.mensajesVuelo[i + 1][vuelo.mensajesVuelo[i + 1].Count + colAST_posy]);

                    // Tiempo inicial en ms
                    int t0 = int.Parse(vuelo.mensajesVuelo[i][colAST_time].Split(':')[0]) * 3600000 +
                             int.Parse(vuelo.mensajesVuelo[i][colAST_time].Split(':')[1]) * 60000 +
                             int.Parse(vuelo.mensajesVuelo[i][colAST_time].Split(':')[2]) * 1000 +
                             int.Parse(vuelo.mensajesVuelo[i][colAST_time].Split(':')[3]);

                    // Creamos los 4 mensajes (t0 + 3 interpolados)
                    double[] altitudesFt = new double[] { alt0, alt1, alt2, alt3 };
                    for (int j = 0; j <= 3; j++)
                    {
                        double factor = j * 0.25;
                        double lat = lat0 + (lat4 - lat0) * factor;
                        double lon = lon0 + (lon4 - lon0) * factor;
                        double altFt = altitudesFt[j];
                        double altM = altFt / 3.28084;
                        double ias = double.IsNaN(ias0) || double.IsNaN(ias4) ? double.NaN : ias0 + (ias4 - ias0) * factor;

                        double posx = posx0 + (posx4 - posx0) * factor;
                        double posy = posy0 + (posy4 - posy0) * factor;

                        // Tiempo
                        int t = t0 + j * 1000;
                        string timeStr = $"{t / 3600000:00}:{(t / 60000) % 60:00}:{(t / 1000) % 60:00}:{t % 1000:000}";

                        var msgTemp = new List<string>
                        {
                            timeStr,                   // Time
                            lat.ToString(),            // LAT
                            lon.ToString(),            // LON
                            altFt.ToString(),          // Altitud ft
                            altM.ToString(),           // Altitud m
                            (vuelo.mensajesVuelo[i][colAST_IAS] == "N/A" || vuelo.mensajesVuelo[i][colAST_IAS] == "NV" || double.IsNaN(ias)) ? "N/A" : ias.ToString(),      // IAS
                            posx.ToString(),           // posX interpolado
                            posy.ToString()            // posY interpolado
                        };

                        mensajesVueloInterpolados.Add(msgTemp);
                    }
                }

                vuelo.mensajesVueloInterpolados = mensajesVueloInterpolados;
            }
        }
        private void RellenarIAS(THRAltitudVelocidad thr, List<List<string>> mensajes, int index, int colIAS)
        {
            string ias = mensajes[index][colIAS];

            if (ias != "N/A")
            {
                thr.IAS = ias;
                thr.IAScorrespondance = "0";
                return;
            }

            int total = mensajes.Count;

            for (int offset = 1; offset < total; offset++)
            {
                int adelante = index + offset;
                if (adelante < total && mensajes[adelante][colIAS] != "N/A")
                {
                    thr.IAS = mensajes[adelante][colIAS];
                    thr.IAScorrespondance = $"+{offset}";
                    return;
                }

                int atras = index - offset;
                if (atras >= 0 && mensajes[atras][colIAS] != "N/A")
                {
                    thr.IAS = mensajes[atras][colIAS];
                    thr.IAScorrespondance = $"-{offset}";
                    return;
                }
            }
        }

        private int BuscarMinimoDistancia(List<List<string>> mensajes, int start, Point thr, int colX, int colY)
        {
            int indexMin = start;
            double minDistance = CalcularDistanciaEntrePuntos(
                new Point(Convert.ToDouble(mensajes[start][colX]), Convert.ToDouble(mensajes[start][colY])),
                thr
            );

            int consecutivosAumento = 0;
            const int maxConsecutivos = 4;

            for (int j = start + 1; j < mensajes.Count; j++)
            {
                Point p = new Point(
                    Convert.ToDouble(mensajes[j][colX]),
                    Convert.ToDouble(mensajes[j][colY])
                );

                double d = CalcularDistanciaEntrePuntos(p, thr);

                if (d < minDistance)
                {
                    minDistance = d;
                    indexMin = j;
                    consecutivosAumento = 0;
                }
                else
                {
                    consecutivosAumento++;
                    if (consecutivosAumento >= maxConsecutivos)
                        break;
                }
            }

            return indexMin;
        }

        public List<THRAltitudVelocidad> CalcularAltitudVelocidadTHR( List<List<string>> datosAsterix, List<Vuelo> vuelosOrdenados, List<THRAltitudVelocidad> listaTHRAltitudVelocidad)
        {
            bool interpol = true;

            CalcularPuntosTHR();

            // Zonas
            List<Point> zona24L = new List<Point>
            {
                new Point(THR_24L_1b.U, THR_24L_1b.V),
                new Point(THR_24L_2c.U, THR_24L_2c.V),
                new Point(THR_24L_4c.U, THR_24L_4c.V),
                new Point(THR_24L_3b.U, THR_24L_3b.V)
            };

            List<Point> zona06R = new List<Point>
            {
                new Point(THR_06R_1b.U, THR_06R_1b.V),
                new Point(THR_06R_2c.U, THR_06R_2c.V),
                new Point(THR_06R_4c.U, THR_06R_4c.V),
                new Point(THR_06R_3b.U, THR_06R_3b.V)
            };

            Point pointTHR_06R = new Point(THR_06R.U, THR_06R.V);
            Point pointTHR_24L = new Point(THR_24L.U, THR_24L.V);

            // Columnas interpolado / no interpolado
            int colTime, colLat, colLon, colAlt, colIAS, colX, colY;

            if (interpol)
            {
                Interpolar1segundo(vuelosOrdenados);

                colTime = 0;
                colLat = 1;
                colLon = 2;
                colAlt = 3;
                colIAS = 5;
                colX = 6;
                colY = 7;
            }
            else
            {
                colTime = 3;
                colLat = 4;
                colLon = 5;
                colAlt = 7;
                colIAS = 21;

                colX = vuelosOrdenados[0].mensajesVuelo[0].Count - 2;
                colY = colX + 1;
            }

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                THRAltitudVelocidad thr = new THRAltitudVelocidad();
                thr.vuelo = vuelo;

                // Zona y THR según pista
                List<Point> zonaDeteccion;
                Point puntoTHR;

                if (vuelo.pistadesp == "LEBL-24L")
                {
                    zonaDeteccion = zona06R;
                    puntoTHR = pointTHR_06R;
                }
                else if (vuelo.pistadesp == "LEBL-06R")
                {
                    zonaDeteccion = zona24L;
                    puntoTHR = pointTHR_24L;
                }
                else
                {
                    thr.pasaPorTHR = false;
                    listaTHRAltitudVelocidad.Add(thr);
                    continue;
                }

                List<List<string>> mensajes = interpol ? vuelo.mensajesVueloInterpolados : vuelo.mensajesVuelo;

                bool encontrado = false;

                // BUSCA ENTRADA EN ZONA
                for (int i = 0; i < mensajes.Count; i++)
                {
                    Point p = new Point(
                        Convert.ToDouble(mensajes[i][colX]),
                        Convert.ToDouble(mensajes[i][colY])
                    );

                    if (PuntoEnZonaDeteccion(p, zonaDeteccion))
                    {
                        // BUSCA MÍNIMO EN ZONA
                        int jmin = BuscarMinimoDistancia(mensajes, i, puntoTHR, colX, colY);

                        // RELLENAR DATOS
                        thr.time = mensajes[jmin][colTime];
                        thr.altitud = mensajes[jmin][colAlt];
                        thr.lat = mensajes[jmin][colLat];
                        thr.lon = mensajes[jmin][colLon];

                        double distanciaMin = CalcularDistanciaEntrePuntos(
                            new Point(Convert.ToDouble(mensajes[jmin][colX]), Convert.ToDouble(mensajes[jmin][colY])),
                            puntoTHR);

                        thr.distance2THR = distanciaMin.ToString();

                        // IAS cercana
                        RellenarIAS(thr, mensajes, jmin, colIAS);

                        encontrado = true;
                        break;
                    }
                }

                thr.pasaPorTHR = encontrado;
                listaTHRAltitudVelocidad.Add(thr);
            }

            // GuardarNoInterpolaciones(vuelosOrdenados);
            // GuardarInterpolaciones(vuelosOrdenados);

            return listaTHRAltitudVelocidad;
        }


        public void GuardarAltitudVelocidadTHR(List<THRAltitudVelocidad> listaTHRAltitudVelocidad)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Datos de THR",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "Datos THR.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        writer1.WriteLine("IdentificacionDESP;Callsign;ATOT;SID;Estela;Tipo Aeronave;Runway;IAS en THR;Altitud en THR;Time en THR;" +
                            "Pasa por la zona definida de THR?;LAT;LON;Distancia a THR;Correspondencia IAS");
                        foreach (THRAltitudVelocidad thr in listaTHRAltitudVelocidad)
                        {
                            try
                            {
                                writer1.WriteLine(thr.vuelo.identificadorDeparture + ";" + thr.vuelo.codigoVuelo + ";" + thr.vuelo.ATOT + ";" + thr.vuelo.sid + ";" + thr.vuelo.estela + ";" +
                                    thr.vuelo.tipo_aeronave + ";" + thr.vuelo.pistadesp + ";" + thr.IAS + ";" + thr.altitud + ";" + thr.time + ";" + 
                                    thr.pasaPorTHR + ";" + thr.lat + ";" + thr.lon + ";" + thr.distance2THR + ";" + thr.IAScorrespondance);
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

        /*
        public void GuardarNoInterpolaciones(List<Vuelo> vuelosOrdenados)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de No Interpolaciones",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "NoInterpolaciones.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        int colAST_time = 3;
                        int colAST_lat = 4;
                        int colAST_lon = 5;
                        int colAST_altitudeft = 7;
                        int colAST_altitudm = 6;
                        int colAST_IAS = 21;

                        int colAST_posx = vuelosOrdenados[0].mensajesVuelo[0].Count - 2;
                        int colAST_posy = vuelosOrdenados[0].mensajesVuelo[0].Count - 1;

                        writer1.WriteLine("Time;Lat;Lon;Altft;Altm;IAS;posX;posY");
                        foreach (Vuelo vuelo in vuelosOrdenados)
                        {
                            foreach (List<string> msg in vuelo.mensajesVuelo)
                            {
                                try
                                {
                                    writer1.WriteLine(vuelo.codigoVuelo + ";" + msg[colAST_time] + ";" + msg[colAST_lat] + ";" + msg[colAST_lon] + ";" + msg[colAST_altitudeft] + ";" + msg[colAST_altitudm] + ";" + 
                                        msg[colAST_IAS] + ";" + msg[colAST_posx] + ";" + msg[colAST_posy]);
                                }
                                catch { }
                            }
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

        public void GuardarInterpolaciones(List<Vuelo> vuelosOrdenados)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Interpolaciones",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "Interpolaciones.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        int colAST_time = 0;
                        int colAST_lat = 1;
                        int colAST_lon = 2;
                        int colAST_altitudeft = 3;
                        int colAST_altitudm = 4;
                        int colAST_IAS = 5;

                        int colAST_posx = 6;
                        int colAST_posy = 7;

                        writer1.WriteLine("Callsign;Time;Lat;Lon;Altft;Altm;IAS;posX;posY");
                        foreach (Vuelo vuelo in vuelosOrdenados)
                        {
                            foreach (List<string> msg in vuelo.mensajesVueloInterpolados)
                            {
                                try
                                {
                                    writer1.WriteLine(vuelo.codigoVuelo + ";" + msg[colAST_time] + ";" + msg[colAST_lat] + ";" + msg[colAST_lon] + ";" + msg[colAST_altitudeft] + ";" + msg[colAST_altitudm] + ";" +
                                        msg[colAST_IAS] + ";" + msg[colAST_posx] + ";" + msg[colAST_posy]);
                                }
                                catch { }
                            }
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
        */

    }
}
