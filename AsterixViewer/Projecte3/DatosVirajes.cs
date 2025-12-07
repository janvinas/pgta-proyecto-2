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
    internal class DatosVirajes
    {
        public class DatosViraje
        {
            public Vuelo vuelo;
            public string lat;
            public string lon;
            public string altitud;
            public string time;
            public string RA;
            public string HDG;
            public string TTA;
            public bool atraviesaRadial234;
            public double radialDVOR;
        }

        public List<DatosViraje> CalcularPosicionAltitudViraje(List<List<string>> datosAsterix, List<Vuelo> vuelosOrdenados, List<DatosViraje> listaVirajes, 
            CoordinatesUVH DVOR_BCN)
        {
            int colAST_time = 3;
            int colAST_lat = 4;
            int colAST_lon = 5;
            int colAST_altitudeft = 7;
            int colAST_RA = 15;
            int colAST_TTA = 16;
            int colAST_HDG = 20;

            int colAST_posx = datosAsterix[0].Count - 2;
            int colAST_posy = datosAsterix[0].Count - 1;

            double headingIntended_24L = -114;
            double headingInicial;
            double heading;
            double heading_prev;
            double heading_aux;

            int indexInicioViraje = 0;

            double posx_viraje;
            double posy_viraje;
            double posx_DVOR = DVOR_BCN.U;
            double posy_DVOR = DVOR_BCN.V;

            double deltaX;
            double deltaY;
            double deltaAngle;
            double radialDVOR;
            double angle = 234;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                if (vuelo.pistadesp == "LEBL-24L")
                {
                    DatosViraje viraje = new DatosViraje();
                    viraje.vuelo = vuelo;
                    viraje.atraviesaRadial234 = false;
                    indexInicioViraje = 0;

                    // Que el heading inicial no sea N/A
                    if (vuelo.mensajesVuelo[0][colAST_HDG] != "N/A" && vuelo.mensajesVuelo[0][colAST_HDG] != "NV") headingInicial = Convert.ToDouble(vuelo.mensajesVuelo[0][colAST_HDG]);
                    else headingInicial = headingIntended_24L;

                    if (Math.Abs(headingIntended_24L - headingInicial) > 10)    // Si el heading inicial no se parece al heading inicial de la 24L -> algo falla
                    {
                        listaVirajes.Add(viraje);
                        continue;
                    }
                    ;

                    for (int i = 1; i < vuelo.mensajesVuelo.Count; i++)
                    {
                        // Que el heaing no sean N/A
                        if (vuelo.mensajesVuelo[i][colAST_HDG] != "N/A" && vuelo.mensajesVuelo[0][colAST_HDG] != "NV")
                        {
                            heading = Convert.ToDouble(vuelo.mensajesVuelo[i][colAST_HDG]);

                            // Que el heading anterior no sea N/A, si lo es -> heading anterior es heading inicial para comparar
                            if (vuelo.mensajesVuelo[i - 1][colAST_HDG] == "N/A" && vuelo.mensajesVuelo[0][colAST_HDG] != "NV") heading_prev = headingInicial;
                            else heading_prev = Convert.ToDouble(vuelo.mensajesVuelo[i - 1][colAST_HDG]);

                            if (heading < heading_prev && heading < headingInicial && Math.Abs(heading - headingInicial) > 3)     // El heading en la salida de 24L se va a valores mas negativos (mas pequeños)
                            {
                                for (int j = i + 1; j < Math.Min(vuelo.mensajesVuelo.Count, i + 5); j++)
                                {
                                    if (vuelo.mensajesVuelo[j][colAST_HDG] == "N/A" && vuelo.mensajesVuelo[0][colAST_HDG] != "NV") continue;

                                    heading_aux = Convert.ToDouble(vuelo.mensajesVuelo[j][colAST_HDG]);
                                    if (Math.Abs(headingInicial - heading_aux) > 10)           // Diferencia de mas de 10 con la inicial -> encontrado
                                    {
                                        indexInicioViraje = i;
                                        break;
                                    }
                                    else if (heading_aux > heading) break;                      // Si algno de los siguientes headings se acerca mas al inicial -> no es este
                                    else if (Math.Abs(heading - heading_aux) < 3) break;        // Si la diferencia con el siguiente es muy baja, aun no es
                                }

                                if (indexInicioViraje != 0)
                                {
                                    // Comprobar si existe RA y TTA, si existen -> comprobar si es valido
                                    if (vuelo.mensajesVuelo[indexInicioViraje][colAST_RA] != "N/A")
                                    {
                                        if (Math.Abs(Convert.ToDouble(vuelo.mensajesVuelo[indexInicioViraje][colAST_RA])) > 5) break;
                                    }
                                    else break;

                                    if (vuelo.mensajesVuelo[indexInicioViraje][colAST_TTA] != "N/A")
                                    {
                                        if (Math.Abs(Convert.ToDouble(vuelo.mensajesVuelo[indexInicioViraje][colAST_TTA]) - headingIntended_24L) < 5) break;
                                    }

                                }
                            }
                        }
                    }

                    // HACER SI CRUZA EL RADIAL 234:
                    posx_viraje = Convert.ToDouble(vuelo.mensajesVuelo[indexInicioViraje][colAST_posx]);
                    posy_viraje = Convert.ToDouble(vuelo.mensajesVuelo[indexInicioViraje][colAST_posy]);

                    deltaX = posx_viraje - posx_DVOR;
                    deltaY = posy_viraje - posy_DVOR;

                    deltaAngle = Math.Atan2(deltaX, deltaY) * 180.0 / Math.PI;
                    if (deltaAngle < 0) deltaAngle += 360;
                    radialDVOR = deltaAngle;
                    if (radialDVOR > angle) viraje.atraviesaRadial234 = true;

                    viraje.lat = vuelo.mensajesVuelo[indexInicioViraje][colAST_lat];
                    viraje.lon = vuelo.mensajesVuelo[indexInicioViraje][colAST_lon];
                    viraje.altitud = vuelo.mensajesVuelo[indexInicioViraje][colAST_altitudeft];
                    viraje.time = vuelo.mensajesVuelo[indexInicioViraje][colAST_time];
                    viraje.RA = vuelo.mensajesVuelo[indexInicioViraje][colAST_RA];
                    viraje.HDG = vuelo.mensajesVuelo[indexInicioViraje][colAST_HDG];
                    viraje.TTA = vuelo.mensajesVuelo[indexInicioViraje][colAST_TTA];

                    viraje.radialDVOR = radialDVOR;

                    listaVirajes.Add(viraje);
                }
            }

            return listaVirajes;
        }

        public void GuardarPosicionAltitudViraje(List<DatosViraje> listaVirajes)
        {
            // csv como nos piden
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Datos de Viraje",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "Datos Viraje.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    var filePath1 = saveFileDialog1.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer1 = new StreamWriter(filePath1, false, Encoding.UTF8))
                    {
                        writer1.WriteLine("IdentificacionDESP;Callsign;ATOT;Latitud;Longitud;Tiempo Inicio Viraje;RA;HDG;TTA;Altitud;SID;Tipo Aeronave;Estela;Atraviesa Radial 234;Radial DVOR que atraviesa viraje");
                        foreach (DatosViraje viraje in listaVirajes)
                        {
                            try
                            {
                                writer1.WriteLine(viraje.vuelo.identificadorDeparture + ";" + viraje.vuelo.codigoVuelo + ";" + viraje.vuelo.ATOT + ";" + viraje.lat + ";" + viraje.lon + ";" + viraje.time + ";" +
                                    viraje.RA + ";" + viraje.HDG + ";" + viraje.TTA + ";" + viraje.altitud + ";" + viraje.vuelo.sid + ";" +
                                    viraje.vuelo.tipo_aeronave + ";" + viraje.vuelo.estela + ";" + viraje.atraviesaRadial234 + ";" + viraje.radialDVOR);
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
