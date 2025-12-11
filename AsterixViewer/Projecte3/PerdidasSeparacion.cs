using AsterixParser.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static AsterixViewer.Projecte3.PerdidasSeparacion;
using static AsterixViewer.Tabs.Proyecto3;

namespace AsterixViewer.Projecte3
{
    internal class PerdidasSeparacion
    {
        // Clase que incluye el vuelo1 (precursor) y vuelo2 (posterior) y el listado de distancias en cada actualización radar
        public class ConjuntoDespeguesConsecutivos
        {
            public Vuelo vuelo1 = new Vuelo();
            public Vuelo vuelo2 = new Vuelo();
            public List<string> listaDistancias = new List<string>();
            public List<string> listaTiemposVuelo1 = new List<string>();
            public List<string> listaTiemposVuelo2 = new List<string>();
        }

        // Tabla que devuelve la minima separación entre dos vuelos según LoA
        public class SeparacionesLoA
        {
            // Declaración válida fuera de métodos
            private static readonly Dictionary<(string, string, string), int> LoA = new Dictionary<(string, string, string), int>
            {
                { ("HP", "HP", "misma"),    5 },  { ("HP", "R", "misma"),    5 },  { ("HP", "LP", "misma"),    5 },  { ("HP", "NR+", "misma"),    3 },  { ("HP", "NR-", "misma"),    3 },  { ("HP", "NR", "misma"),    3 },
                { ("HP", "HP", "distinta"), 3 },  { ("HP", "R", "distinta"), 3 },  { ("HP", "LP", "distinta"), 3 },  { ("HP", "NR+", "distinta"), 3 },  { ("HP", "NR-", "distinta"), 3 },  { ("HP", "NR", "distinta"), 3 },

                { ("R", "HP", "misma"),    7 },   { ("R", "R", "misma"),    5 },   { ("R", "LP", "misma"),    5 },   { ("R", "NR+", "misma"),    3 },   { ("R", "NR-", "misma"),    3 },   { ("R", "NR", "misma"),    3 },
                { ("R", "HP", "distinta"), 5 },   { ("R", "R", "distinta"), 3 },   { ("R", "LP", "distinta"), 3 },   { ("R", "NR+", "distinta"), 3 },   { ("R", "NR-", "distinta"), 3 },   { ("R", "NR", "distinta"), 3 },

                { ("LP", "HP", "misma"),    8 },  { ("LP", "R", "misma"),    6 },  { ("LP", "LP", "misma"),    5 },  { ("LP", "NR+", "misma"),    3 },  { ("LP", "NR-", "misma"),    3 },  { ("LP", "NR", "misma"),    3 },
                { ("LP", "HP", "distinta"), 6 },  { ("LP", "R", "distinta"), 4 },  { ("LP", "LP", "distinta"), 3 },  { ("LP", "NR+", "distinta"), 3 },  { ("LP", "NR-", "distinta"), 3 },  { ("LP", "NR", "distinta"), 3 },

                { ("NR+", "HP", "misma"),   11 }, { ("NR+", "R", "misma"),    9 }, { ("NR+", "LP", "misma"),    9 }, { ("NR+", "NR+", "misma"),    5 }, { ("NR+", "NR-", "misma"),    3 }, { ("NR+", "NR", "misma"),    3 },
                { ("NR+", "HP", "distinta"), 8 }, { ("NR+", "R", "distinta"), 6 }, { ("NR+", "LP", "distinta"), 6 }, { ("NR+", "NR+", "distinta"), 3 }, { ("NR+", "NR-", "distinta"), 3 }, { ("NR+", "NR", "distinta"), 3 },

                { ("NR-", "HP", "misma"),    9 }, { ("NR-", "R", "misma"),    9 }, { ("NR-", "LP", "misma"),    9 }, { ("NR-", "NR+", "misma"),    9 }, { ("NR-", "NR-", "misma"),    5 }, { ("NR-", "NR", "misma"),    3 },
                { ("NR-", "HP", "distinta"), 9 }, { ("NR-", "R", "distinta"), 9 }, { ("NR-", "LP", "distinta"), 9 }, { ("NR-", "NR+", "distinta"), 6 }, { ("NR-", "NR-", "distinta"), 3 }, { ("NR-", "NR", "distinta"), 3 },

                { ("NR", "HP", "misma"),    9 },  { ("NR", "R", "misma"),    9 },  { ("NR", "LP", "misma"),    9 },  { ("NR", "NR+", "misma"),    9 },  { ("NR", "NR-", "misma"),    9 },  { ("NR", "NR", "misma"),    5 },
                { ("NR", "HP", "distinta"), 9 },  { ("NR", "R", "distinta"), 9 },  { ("NR", "LP", "distinta"), 9 },  { ("NR", "NR+", "distinta"), 9 },  { ("NR", "NR-", "distinta"), 9 },  { ("NR", "NR", "distinta"), 3 }
            };

            public int ObtenerSeparacionLoA(string perf1, string perf2, string sid)
            {
                int separacion_NM = LoA[(perf1, perf2, sid)];
                int separacion_m = separacion_NM * 1852;

                return separacion_m;
            }
        }

        // Clasificacion Aeronaves segun performance de LoA
        public class ClasificacionAeronavesLoA
        {
            public List<string> HP = new List<string>();
            public List<string> NR = new List<string>();
            public List<string> NRplus = new List<string>();
            public List<string> NRminus = new List<string>();
            public List<string> LP = new List<string>();
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        public List<ConjuntoDespeguesConsecutivos> CalcularDistanciasDespeguesConsecutivos(List<Vuelo> vuelosOrdenados, List<List<string>> datosAsterix,
            CoordinatesUVH THR_24L, CoordinatesUVH THR_06R, List<ConjuntoDespeguesConsecutivos> listaConjuntosDistanciasDespeguesConsecutivos)
        {
            int TIcol = 13;     // Posición de columna en que se encuentra la variable TI en csv datosAsterix
            int TIMEcol = 3;
            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;

            int tiempo_ms_vuelo1;
            int tiempo_ms_vuelo2;
            int tiempo_ms_vuelo1_05NM;
            int tiempo_ms_vuelo2_05NM;

            Point posTHR_24L = new Point(THR_24L.U, THR_24L.V);
            Point posTHR_06R = new Point(THR_06R.U, THR_06R.V);
            Point posVuelo1;
            Point posVuelo2;
            bool condicion05NMvuelo1 = false;
            bool condicion05NMvuelo2 = false;

            double distance;

            int N = 20;

            // Itero cada vuelo
            for (int i = 0; i < vuelosOrdenados.Count - 1; i++)
            {
                Vuelo vuelo1 = vuelosOrdenados[i];
                Vuelo vuelo2 = vuelosOrdenados[i + 1];

                ConjuntoDespeguesConsecutivos distanciasDespeguesConsecutivos = new ConjuntoDespeguesConsecutivos();
                distanciasDespeguesConsecutivos.vuelo1 = vuelo1;
                distanciasDespeguesConsecutivos.vuelo2 = vuelo2;

                int numberOfIteratedMSGvuelo1 = 0;

                // Iterar sobre todos los mensajes Asterix para encontrar las posiciones del vuelo1
                for (int j = 0; j < datosAsterix.Count - 1; j++)
                {
                    if (datosAsterix[j][TIcol] == vuelo1.codigoVuelo)
                    {
                        condicion05NMvuelo1 = false;
                        numberOfIteratedMSGvuelo1++;

                        posVuelo1 = new Point(Convert.ToDouble(datosAsterix[j][Xcol]), Convert.ToDouble(datosAsterix[j][Ycol]));
                        tiempo_ms_vuelo1 = int.Parse(datosAsterix[j][TIMEcol].Split(':')[0]) * 3600 * 1000 + int.Parse(datosAsterix[j][TIMEcol].Split(':')[1]) * 60000 + int.Parse(datosAsterix[j][TIMEcol].Split(':')[2]) * 1000 + int.Parse(datosAsterix[j][TIMEcol].Split(':')[3]);
                        tiempo_ms_vuelo1_05NM = int.Parse(vuelo1.timeDEP_05NM.Split(':')[0]) * 3600 * 1000 + int.Parse(vuelo1.timeDEP_05NM.Split(':')[1]) * 60000 + int.Parse(vuelo1.timeDEP_05NM.Split(':')[2]) * 1000 + int.Parse(vuelo1.timeDEP_05NM.Split(':')[3]);

                        condicion05NMvuelo1 = tiempo_ms_vuelo1 >= tiempo_ms_vuelo1_05NM;

                        if (condicion05NMvuelo1)
                        {
                            // Iterar sobre los siguientes N vuelos para encontrar la detección simultanea del vuelo2 (N = 15 arbitrario)
                            for (int j2 = j + 1; j2 < Math.Min(j + N, datosAsterix.Count); j2++)
                            {
                                if (datosAsterix[j2][TIcol] == vuelo2.codigoVuelo && vuelo1.pistadesp == vuelo2.pistadesp)
                                {
                                    posVuelo2 = new Point(Convert.ToDouble(datosAsterix[j2][Xcol]), Convert.ToDouble(datosAsterix[j2][Ycol]));
                                    tiempo_ms_vuelo2 = int.Parse(datosAsterix[j2][TIMEcol].Split(':')[0]) * 3600 * 1000 + int.Parse(datosAsterix[j2][TIMEcol].Split(':')[1]) * 60000 + int.Parse(datosAsterix[j2][TIMEcol].Split(':')[2]) * 1000 + int.Parse(datosAsterix[j2][TIMEcol].Split(':')[3]);
                                    tiempo_ms_vuelo2_05NM = int.Parse(vuelo2.timeDEP_05NM.Split(':')[0]) * 3600 * 1000 + int.Parse(vuelo2.timeDEP_05NM.Split(':')[1]) * 60000 + int.Parse(vuelo2.timeDEP_05NM.Split(':')[2]) * 1000 + int.Parse(vuelo2.timeDEP_05NM.Split(':')[3]);

                                    condicion05NMvuelo2 = tiempo_ms_vuelo2 >= tiempo_ms_vuelo2_05NM;

                                    if (Math.Abs(tiempo_ms_vuelo2 - tiempo_ms_vuelo1) < 3000 && condicion05NMvuelo2)
                                    {
                                        distance = CalcularDistanciaEntrePuntos(posVuelo1, posVuelo2);

                                        distanciasDespeguesConsecutivos.listaDistancias.Add(distance.ToString());
                                        distanciasDespeguesConsecutivos.listaTiemposVuelo1.Add(datosAsterix[j][3]);
                                        distanciasDespeguesConsecutivos.listaTiemposVuelo2.Add(datosAsterix[j2][3]);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (numberOfIteratedMSGvuelo1 >= vuelo1.mensajesVuelo.Count) break;
                }

                listaConjuntosDistanciasDespeguesConsecutivos.Add(distanciasDespeguesConsecutivos);
            }

            return listaConjuntosDistanciasDespeguesConsecutivos;
        }
        private bool IncumplimientoRadar(string distancia)
        {
            if (Convert.ToDouble(distancia) < 3 * 1852) { return true; }
            else return false;
        }

        private List<string> IncumplimientoEstela(ConjuntoDespeguesConsecutivos conjunto, string distancia)
        {
            if (conjunto.vuelo1.estela == "Pesada" && conjunto.vuelo2.estela == "Pesada")
            {
                if (Convert.ToDouble(distancia) < 4 * 1852) return ["True", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
                else return ["False", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
            }
            else if ((conjunto.vuelo1.estela == "Pesada" && conjunto.vuelo2.estela == "Media") || (conjunto.vuelo1.estela == "Media" && conjunto.vuelo2.estela == "Ligera"))
            {
                if (Convert.ToDouble(distancia) < 5 * 1852) return ["True", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
                else return ["False", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
            }
            else if ((conjunto.vuelo1.estela == "Super Pesada" && conjunto.vuelo2.estela == "Pesada") || (conjunto.vuelo1.estela == "Pesada" && conjunto.vuelo2.estela == "Ligera"))
            {
                if (Convert.ToDouble(distancia) < 6 * 1852) return ["True", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
                else return ["False", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
            }
            else if (conjunto.vuelo1.estela == "Super pesada" && conjunto.vuelo2.estela == "Media")
            {
                if (Convert.ToDouble(distancia) < 7 * 1852) return ["True", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
                else return ["False", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
            }
            else if (conjunto.vuelo1.estela == "Super pesada" && conjunto.vuelo2.estela == "Ligera")
            {
                if (Convert.ToDouble(distancia) < 8 * 1852) return ["True", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
                else return ["False", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
            }
            else return ["N/A", conjunto.vuelo1.estela, conjunto.vuelo2.estela];
        }

        private void AñadirMotorizacion(List<Vuelo> vuelosOrdenados, ClasificacionAeronavesLoA clasificacionAeronavesLoA)
        {
            for (int i = 0; i < vuelosOrdenados.Count; i++)
            {
                if (clasificacionAeronavesLoA.HP.Contains(vuelosOrdenados[i].tipo_aeronave)) vuelosOrdenados[i].motorizacion = "HP";
                else if (clasificacionAeronavesLoA.NR.Contains(vuelosOrdenados[i].tipo_aeronave)) vuelosOrdenados[i].motorizacion = "NR";
                else if (clasificacionAeronavesLoA.LP.Contains(vuelosOrdenados[i].tipo_aeronave)) vuelosOrdenados[i].motorizacion = "LP";
                else if (clasificacionAeronavesLoA.NRminus.Contains(vuelosOrdenados[i].tipo_aeronave)) vuelosOrdenados[i].motorizacion = "NR-";
                else if (clasificacionAeronavesLoA.NRplus.Contains(vuelosOrdenados[i].tipo_aeronave)) vuelosOrdenados[i].motorizacion = "NR+";
                else vuelosOrdenados[i].motorizacion = "R";
            }
        }

        private List<string> IncumplimientoLoA(ConjuntoDespeguesConsecutivos conjunto, string distancia)
        {
            SeparacionesLoA LoA = new SeparacionesLoA();
            List<string> g1 = new List<string>(["OLOXO", "NATPI", "MOPAS", "GRAUS", "LOBAR", "MAMUK", "REBUL", "VIBOK", "DUQQI"]);
            List<string> g2 = new List<string>(["DUNES", "LARPA", "LOTOS", "SENIA"]);
            List<string> g3 = new List<string>(["DALIN", "AGENA", "DIPES"]);
            string misma = "distinta";

            string sid1 = conjunto.vuelo1.sid[..^2];
            string sid2 = conjunto.vuelo2.sid[..^2];

            if (g1.Contains(sid1))
            {
                if (g1.Contains(sid2)) misma = "misma";
                else if (g2.Contains(sid2)) misma = "distinta";
                else if (g3.Contains(sid2)) misma = "distinta";
                else misma = "misma";
            }
            else if (g2.Contains(sid1))
            {
                if (g2.Contains(sid2)) misma = "misma";
                else if (g1.Contains(sid2)) misma = "distinta";
                else if (g3.Contains(sid2)) misma = "distinta";
                else misma = "misma";
            }
            else if (g3.Contains(sid1))
            {
                if (g3.Contains(sid2)) misma = "misma";
                else if (g1.Contains(sid2)) misma = "distinta";
                else if (g2.Contains(sid2)) misma = "distinta";
                else misma = "misma";
            }
            else misma = "misma";

            if (Convert.ToDouble(distancia) < LoA.ObtenerSeparacionLoA(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma)) return ["True", misma, Convert.ToString(LoA.ObtenerSeparacionLoA(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma) / 1852)];
            else return ["False", misma, Convert.ToString(LoA.ObtenerSeparacionLoA(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma) / 1852)];
        }

        public void GuardarDistDESPConsecutivos(List<Vuelo> vuelosOrdenados, ClasificacionAeronavesLoA clasificacionAeronavesLoA,
            List<ConjuntoDespeguesConsecutivos> listaConjuntosDistanciasDespeguesConsecutivos)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog2 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Distancias Vuelos Consecutivos",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "Consecutive Distances.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog2.ShowDialog() == true)
                {
                    var filePath2 = saveFileDialog2.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer2 = new StreamWriter(filePath2, false, Encoding.UTF8))
                    {
                        writer2.WriteLine("Avion precedente;Avion posterior;Hora de activación PV;ToD zona TWR;Distancia zona TWR;ToD mínima distancia zona TMA;Mínima distancia zona TMA;Inc. Radar TMA;Inc. Radar TWR;Inc Estela TMA;Inc. Estela TWR;Inc LoA;Min distancia según LoA;Misma SID/Distinta SID;Estela precedente;Estela posterior;Motor precedente;Motor posterior;SID precedente;SID posterior;Modelo precedente;Modelo posterior;Runway DEP");
                        foreach (var conjunto in listaConjuntosDistanciasDespeguesConsecutivos)
                        {
                            int minimadistanciaTMA = 1;
                            AñadirMotorizacion(vuelosOrdenados, clasificacionAeronavesLoA);
                            for (int i = 1; i < conjunto.listaDistancias.Count; i++)
                            {
                                if (Convert.ToDouble(conjunto.listaDistancias[i]) < Convert.ToDouble(conjunto.listaDistancias[i - 1])) minimadistanciaTMA = i;
                            }
                            try
                            {
                                writer2.WriteLine(conjunto.vuelo1.codigoVuelo + ";" + conjunto.vuelo2.codigoVuelo + ";" + conjunto.vuelo2.horaPV + ";" +
                                    conjunto.listaTiemposVuelo2[0] + ";" + Convert.ToString(Convert.ToDouble(conjunto.listaDistancias[0]) / 1852) + ";" + conjunto.listaTiemposVuelo2[minimadistanciaTMA] + ";" +
                                    Convert.ToString(Convert.ToDouble(conjunto.listaDistancias[minimadistanciaTMA]) / 1852) + ";" + IncumplimientoRadar(conjunto.listaDistancias[minimadistanciaTMA]) + ";" +
                                    IncumplimientoRadar(conjunto.listaDistancias[0]) + ";" + IncumplimientoEstela(conjunto, conjunto.listaDistancias[minimadistanciaTMA])[0] + ";" +
                                    IncumplimientoEstela(conjunto, conjunto.listaDistancias[0])[0] + ";" + IncumplimientoLoA(conjunto, conjunto.listaDistancias[0])[0] + ";" +
                                    IncumplimientoLoA(conjunto, conjunto.listaDistancias[0])[2] + ";" + IncumplimientoLoA(conjunto, conjunto.listaDistancias[0])[1] + ";" +
                                    IncumplimientoEstela(conjunto, conjunto.listaDistancias[minimadistanciaTMA])[1] + ";" + IncumplimientoEstela(conjunto, conjunto.listaDistancias[minimadistanciaTMA])[2] + ";" +
                                    conjunto.vuelo1.motorizacion + ";" + conjunto.vuelo2.motorizacion + ";" + conjunto.vuelo1.sid + ";" + conjunto.vuelo2.sid + ";" +
                                    conjunto.vuelo1.tipo_aeronave + ";" + conjunto.vuelo2.tipo_aeronave + ";" + conjunto.vuelo2.pistadesp);
                            }
                            catch
                            {

                            }
                        }
                    }

                    // ✅ Confirmar al usuario
                    MessageBox.Show(
                        $"Archivo exportado correctamente:\n{filePath2}",
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

        // ESTE ES EL ANTIGUO, PARA DEBUGGEAR, QUITARLO
        /*
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Guardar CSV de Distancias Vuelos Consecutivos",
                Filter = "Archivos CSV (*.xlsx)|*.csv",
                FileName = "Consecutive Distances Raw.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // ✍️ Escribir el archivo (CSV con extensión XLSX)
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    foreach (var conjunto in listaConjuntosDistanciasDespeguesConsecutivos)
                    {
                        writer.WriteLine("Vuelos: " + conjunto.vuelo1.codigoVuelo + ";" + conjunto.vuelo2.codigoVuelo);
                        writer.WriteLine("Distancias: ;" + string.Join(";", conjunto.listaDistancias));
                        writer.WriteLine("Tiempos detección vuelo1: ;" + string.Join(";", conjunto.listaTiemposVuelo1));
                        writer.WriteLine("Tiempos detección vuelo2: ;" + string.Join(";", conjunto.listaTiemposVuelo2));
                        writer.WriteLine();
                    }
                }

                // ✅ Confirmar al usuario
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
        */

    }
}
