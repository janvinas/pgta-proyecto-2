using AsterixParser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AsterixViewer.Projecte3.PerdidasSeparacion;
using static AsterixViewer.Tabs.Proyecto3;

namespace AsterixViewer.Projecte3
{
    internal class CalculosPreliminares
    {
        public CoordinatesUVH DefinirPosicionEstereograficaPuntoFijo(string LatLon, double height)
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            // Calcular Estereograficas del punto fijo
            CoordinatesWGS84 coords_geodesic = GeoUtils.LatLonStringBoth2Radians(LatLon, height + rt);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            CoordinatesUVH fixPoint = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            return fixPoint;
        }

        public List<List<string>> AcondicionarPV(List<List<string>> listaPV)
        {
            int pistadesp = -1;
            int procdesp = -1;
            int rutasacta = -1;
            for (int i = 0; i < listaPV[0].Count; i++) // Buscamos columnas clave
            {
                if (listaPV[0][i] == "PistaDesp") pistadesp = i;
                if (listaPV[0][i] == "ProcDesp") procdesp = i;
                if (listaPV[0][i] == "RutaSACTA") rutasacta = i;
            }

            for (int i = 1; i < listaPV.Count - 1; i++) // Solo dejamos las de 24L y 06R
            {
                if (listaPV[i][pistadesp] != "LEBL-24L" && listaPV[i][pistadesp] != "LEBL-06R")
                {
                    listaPV.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 1; i < listaPV.Count - 1; i++)
            {
                if (listaPV[i][procdesp] == "-")
                {
                    var puntos = listaPV[i][rutasacta].Split(' ');
                    bool encontrado1 = false;
                    bool encontrado2 = false;
                    int j = 0;
                    int m = 0;

                    // ------------ QUE HACER CON NITBA ------------
                    var puntosdeseados = new HashSet<string> { "OLOXO", "NATPI", "MOPAS", "GRAUS", "LOBAR", "MAMUK", "REBUL", "VIBOK", "DUQQI", "DUNES", "LARPA", "LOTOS", "SENIA", "DALIN", "AGENA", "DIPES" };

                    // Cuando el texto de la SID sale tal cual
                    while (!encontrado1)
                    {
                        if (puntosdeseados.Contains(puntos[puntos.Length - j - 1]))
                        {
                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1C");
                            }
                            else if (listaPV[i][pistadesp] == "LEBL-06R")
                            {
                                if (puntos[puntos.Length - j - 1] == "OLOXO") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "NATPI") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}2R");
                                else if (puntos[puntos.Length - j - 1] == "MOPAS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "GRAUS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "LOBAR") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "MAMUK") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}I1R");
                                else if (puntos[puntos.Length - j - 1] == "REBUL") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "VIBOK") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "DUQQI") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                                else if (puntos[puntos.Length - j - 1] == "DUNES") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "LARPA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "LOTOS") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}3R");
                                else if (puntos[puntos.Length - j - 1] == "SENIA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}5R");
                                else if (puntos[puntos.Length - j - 1] == "DALIN") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "AGENA") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}4R");
                                else if (puntos[puntos.Length - j - 1] == "DIPES") listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1R");
                            }

                            encontrado1 = true;
                            break;
                        }
                        j++;
                        if (j == puntos.Length)
                        {
                            encontrado1 = true;
                            break;
                        }
                    }

                    // Cuando la SID sale entre paréntesis
                    while (!encontrado2)
                    {
                        List<string> puntos2 = new List<string>();

                        int k = 0;
                        while (k < listaPV[i][rutasacta].Length)
                        {
                            List<char> elementos = new List<char>();
                            if (listaPV[i][rutasacta][k] == '(')
                            {
                                int l = 0;
                                while (listaPV[i][rutasacta][k + l] != ')')
                                {
                                    if (listaPV[i][rutasacta][k + l] != '(') elementos.Add(listaPV[i][rutasacta][k + l]);
                                    l++;
                                }
                                puntos2.Add(new string([.. elementos]));
                                elementos.Clear();
                            }
                            k++;
                        }
                        if (puntos2.Count == 0)
                        {
                            encontrado2 = true;
                            break;
                        }
                        if (puntosdeseados.Contains(puntos2[puntos2.Count - m - 1]))
                        {
                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1C");
                            }
                            else if (listaPV[i][pistadesp] == "LEBL-06R")
                            {
                                if (puntos2[puntos2.Count - m - 1] == "OLOXO") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "NATPI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}2R");
                                else if (puntos2[puntos2.Count - m - 1] == "MOPAS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "GRAUS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "LOBAR") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "MAMUK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}I1R");
                                else if (puntos2[puntos2.Count - m - 1] == "REBUL") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "VIBOK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "DUQQI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                                else if (puntos2[puntos2.Count - m - 1] == "DUNES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "LARPA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "LOTOS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}3R");
                                else if (puntos2[puntos2.Count - m - 1] == "SENIA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}5R");
                                else if (puntos2[puntos2.Count - m - 1] == "DALIN") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "AGENA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}4R");
                                else if (puntos2[puntos2.Count - m - 1] == "DIPES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - m - 1]}1R");
                            }

                            encontrado2 = true;
                            break;
                        }
                        m++;
                        if (m == puntos2.Count)
                        {
                            encontrado2 = true;
                            break;
                        }
                    }
                }
            }
            return listaPV;
        }

        public List<List<string>> FiltroDeparturesLEBL(List<List<string>> datosAsterix, List<List<string>> listaPV)
        {
            int ti = -1;
            int indicative = -1;
            int rmv = 0;

            for (int i = 0; i < datosAsterix[0].Count; i++)
            {
                if (datosAsterix[0][i] == "TI")
                {
                    ti = i;
                    break;
                }
            }

            for (int i = 0; i < listaPV[0].Count; i++)
            {
                if (listaPV[0][i] == "Indicativo")
                {
                    indicative = i;
                    break;
                }
            }

            for (int i = 0; i < datosAsterix.Count; i++)
            {
                bool pertenece = false;
                for (int j = 0; j < listaPV.Count; j++)
                {
                    if (datosAsterix[i][ti] == listaPV[j][indicative])
                    {
                        pertenece = true;
                        break;
                    }
                }
                if (!pertenece)
                {
                    datosAsterix.RemoveAt(i);
                    rmv++;
                    i--;
                }
            }

            return datosAsterix;
        }

        public List<Vuelo> ClasificarDistintosVuelos(List<List<string>> datosAsterix, List<List<string>> listaPV)
        {
            List<Vuelo> vuelosOrdenados = new List<Vuelo>();

            int colPV_callsign = 1;
            int colPV_ATOT = 10;

            int colAST_time = 3;
            int colAST_callsign = 13;

            int ATOT_ms;
            int ASTtime_ms;

            int counter = 0;

            listaPV.RemoveAt(0);
            foreach (List<string> planVuelo in listaPV)
            {
                Vuelo vuelo = new Vuelo();
                vuelo.codigoVuelo = planVuelo[colPV_callsign];

                vuelo.identificadorDeparture = planVuelo[0];
                vuelo.horaPV = planVuelo[4];
                vuelo.tipo_aeronave = planVuelo[6];
                vuelo.estela = planVuelo[7];
                vuelo.sid = planVuelo[9];
                vuelo.ATOT = planVuelo[10];
                vuelo.pistadesp = planVuelo[11];

                for (int i = 0; i < datosAsterix.Count; i++)
                {
                    if (datosAsterix[i][colAST_callsign] == planVuelo[colPV_callsign])
                    {
                        ATOT_ms = (int)TimeSpan.Parse(planVuelo[colPV_ATOT]).TotalMilliseconds;
                        ASTtime_ms = int.Parse(datosAsterix[i][colAST_time].Split(':')[0]) * 3600 * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[1]) * 60 * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[2]) * 1000
                            + int.Parse(datosAsterix[i][colAST_time].Split(':')[3]);

                        if (Math.Abs(ATOT_ms - ASTtime_ms) < 0.5 * 3600 * 1000)
                        {
                            for (int j = i; j < datosAsterix.Count; j++)
                            {
                                if (datosAsterix[j][colAST_callsign] == planVuelo[colPV_callsign])
                                {
                                    vuelo.mensajesVuelo.Add(datosAsterix[j]);
                                    counter = 0;
                                }
                                else counter++;

                                if (counter >= 100) break;
                            }
                        }
                    }
                }

                // TAL VEZ ES MEJOR NO ELIMINAR LOS PV SI NO SALEN EN ASTERIX
                if (vuelo.mensajesVuelo.Count > 0) vuelosOrdenados.Add(vuelo);
            }

            return vuelosOrdenados;
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        public List<Vuelo> CalcularTiempo05NMfromTHR(List<List<string>> datosAsterix, List<Vuelo> vuelosOrdenados, CoordinatesUVH THR_24L, CoordinatesUVH THR_06R)
        {
            int TIMEcol = 3;
            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;

            Point posTHR_24L = new Point(THR_24L.U, THR_24L.V);
            Point posTHR_06R = new Point(THR_06R.U, THR_06R.V);
            Point posVuelo;
            Point posVuelo_siguienteMSG;

            double distanciaVueloTHR;
            double distanciaVueloTHR_MSGposterior;

            bool condicion05NMvuelo = false;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                condicion05NMvuelo = false;

                for (int j = 0; j < vuelo.mensajesVuelo.Count - 1; j++)
                {
                    posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j][Ycol]));
                    posVuelo_siguienteMSG = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j + 1][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j + 1][Ycol]));

                    if (vuelo.pistadesp == "LEBL-24L")
                    {
                        distanciaVueloTHR = CalcularDistanciaEntrePuntos(posVuelo, posTHR_06R);
                        distanciaVueloTHR_MSGposterior = CalcularDistanciaEntrePuntos(posVuelo_siguienteMSG, posTHR_06R);

                        condicion05NMvuelo = (distanciaVueloTHR > 1852 / 2) && (distanciaVueloTHR < distanciaVueloTHR_MSGposterior);
                    }
                    else
                    {
                        distanciaVueloTHR = CalcularDistanciaEntrePuntos(posVuelo, posTHR_24L);
                        distanciaVueloTHR_MSGposterior = CalcularDistanciaEntrePuntos(posVuelo_siguienteMSG, posTHR_24L);

                        condicion05NMvuelo = (distanciaVueloTHR > 1852 / 2) && (distanciaVueloTHR < distanciaVueloTHR_MSGposterior);
                    }

                    if (condicion05NMvuelo)
                    {
                        vuelo.timeDEP_05NM = vuelo.mensajesVuelo[j][TIMEcol];
                        break;
                    }
                }
            }

            return vuelosOrdenados;
        }

        public List<Vuelo> AñadirMotorizacion(List<Vuelo> vuelosOrdenados, ClasificacionAeronavesLoA clasificacionAeronavesLoA)
        {
            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                if (clasificacionAeronavesLoA.HP.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "HP";
                else if (clasificacionAeronavesLoA.NR.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR";
                else if (clasificacionAeronavesLoA.LP.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "LP";
                else if (clasificacionAeronavesLoA.NRminus.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR-";
                else if (clasificacionAeronavesLoA.NRplus.Contains(vuelo.tipo_aeronave)) vuelo.motorizacion = "NR+";
                else vuelo.motorizacion = "R";
            }

            return vuelosOrdenados;
        }

    }
}
