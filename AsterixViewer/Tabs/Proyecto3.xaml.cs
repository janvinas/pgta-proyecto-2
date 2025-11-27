using Accord.IO;
using AsterixParser;
using AsterixParser.Utils;
using AsterixViewer.Projecte3;
using ExcelDataReader;
using ExcelDataReader.Log;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Management.Deployment;

using static AsterixViewer.Projecte3.LecturaArchivos;
using static AsterixViewer.Projecte3.PerdidasSeparacion;
using static AsterixViewer.Projecte3.DistanciasSonometro;
using static AsterixViewer.Projecte3.VelocidadesDespegue;
using static AsterixViewer.Projecte3.DatosVirajes;

using static AsterixViewer.Tabs.Proyecto3;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixViewer.Tabs
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class Proyecto3 : UserControl
    {
        // -------------------------------- DEFINICIÓN DE VARIABLES GLOBALES ----------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------- 

        public bool calculosPreliminaresHechos = false;

        // Listado de msg Asterix en formato filas de variables
        List<List<string>> datosAsterix = new List<List<string>>();

        // Listado de planes de Vuelo
        List<List<string>> listaPV = new List<List<string>>();

        // Datos sobre clasificacion de las aeronaves (Input P3)
        ClasificacionAeronavesLoA clasificacionAeronavesLoA;

        // Definicion de distintos puntos fijos
        CoordinatesUVH THR_06R = new CoordinatesUVH();
        CoordinatesUVH THR_24L = new CoordinatesUVH();
        CoordinatesUVH sonometro = new CoordinatesUVH();
        CoordinatesUVH DVOR_BCN = new CoordinatesUVH();

        // Clase Vuelo, cada despegue es una instancia, contiene el codigo del avion y la lista de mensajes Asterix que le corresponden
        public class Vuelo
        {
            public string codigoVuelo { get; set; }
            public string horaPV { get; set; }
            public string estela { get; set; }
            public string pistadesp {  get; set; }
            public string tipo_aeronave { get; set; }
            public string sid { get; set; }
            public string motorizacion { get; set; }
            public string ATOT { get; set; }
            public string timeDEP_05NM { get; set; }
            public List<List<string>> mensajesVuelo { get; set; } = new List<List<string>>();
        }

        // Lista de todos los vuelos ya ordenados y filtrados
        List<Vuelo> vuelosOrdenados = new List<Vuelo>();                

        // Lista de conjuntos de distancias de despegues consecutivos
        List<DistanciasDespeguesConsecutivos> listaConjuntosDistanciasDespeguesConsecutivos = new List<DistanciasDespeguesConsecutivos>();

        // Lista de datos de distancias minimas de vuelos respecto sonometro
        List<DistanciaMinimaSonometro> listaDistanciasMinimasSonometro = new List<DistanciaMinimaSonometro>();

        // Lista de datos de velocidades en despegue a distintas altitudes
        List<IASaltitudes> listaVelocidadesIASDespegue = new List<IASaltitudes>();

        // Lista de datos de los virajes de los vuelos y sus radiales respecto al DVOR
        List<DatosViraje> listaVirajes = new List<DatosViraje>();

        //
        List<THRAltitudVelocidad> listaTHRAltitudVelocidad = new List<THRAltitudVelocidad>();

        public Proyecto3()
        {
            InitializeComponent();
        }

        private void DatosAsterix_Click(object sender, RoutedEventArgs e)
        {
            datosAsterix.Clear();

            LecturaArchivos lect = new LecturaArchivos();
            datosAsterix = lect.LeerCsvASTERIX();
        }

        public void Planvuelo_click(object sender, RoutedEventArgs e)
        {
            listaPV.Clear();

            LecturaArchivos lect = new LecturaArchivos();
            listaPV = lect.LeerExcelPV();
        }

        private void ClasificacionAeronaves_Click(object sender, RoutedEventArgs e)
        {
            LecturaArchivos lect = new LecturaArchivos();
            clasificacionAeronavesLoA = lect.LeerClasificacionAeronaves();
        }

        private void CalculosPreliminares_Click(object sender, RoutedEventArgs e)
        {
            if (!calculosPreliminaresHechos)
            {
                try
                {
                    AcondicionarPV();
                    FiltroDeparturesLEBL();

                    DefinirPosicionesEstereograficasPuntosfijos();
                    CalcularPosicionesEstereograficas();

                    ClasificarDistintosVuelos();
                    // OrdenarVuelos();
                    
                    CalcularTiempo05NMfromTHR();

                    calculosPreliminaresHechos = true;

                    MessageBox.Show(
                        $"Se han hecho los calculos preliminares correctamente",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch
                {
                    MessageBox.Show(
                        $"Se deben leer todos los archivos apropiados!!",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        // Todas las llamadas necesarias para calcular las separciones entre despegues consecutivos
        private void CalculoSeparacionesDespegues_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaConjuntosDistanciasDespeguesConsecutivos.Clear();

                PerdidasSeparacion sep = new PerdidasSeparacion();
                listaConjuntosDistanciasDespeguesConsecutivos = sep.CalcularDistanciasDespeguesConsecutivos(vuelosOrdenados,datosAsterix,THR_24L,THR_06R,listaConjuntosDistanciasDespeguesConsecutivos);
                sep.GuardarDistDESPConsecutivos(vuelosOrdenados,clasificacionAeronavesLoA,listaConjuntosDistanciasDespeguesConsecutivos);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void DistanciasMinimasSonometro_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaDistanciasMinimasSonometro.Clear();

                DistanciasSonometro dist = new DistanciasSonometro();
                listaDistanciasMinimasSonometro = dist.CalcularDistanciaMinimaSonometro(datosAsterix,vuelosOrdenados,sonometro,listaDistanciasMinimasSonometro);
                dist.GuardarDistMinSonometro(listaDistanciasMinimasSonometro);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void VelocidadIASDespegue_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaVelocidadesIASDespegue.Clear();

                VelocidadesDespegue vel = new VelocidadesDespegue();
                listaVelocidadesIASDespegue = vel.CalcularVelocidadIASDespegue(vuelosOrdenados,listaVelocidadesIASDespegue);
                vel.GuardarVelocidadIASDespegue(listaVelocidadesIASDespegue);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void PosicionAltitudViraje_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaVirajes.Clear();
                
                DatosVirajes vir = new DatosVirajes();
                listaVirajes = vir.CalcularPosicionAltitudViraje(datosAsterix,vuelosOrdenados,listaVirajes,DVOR_BCN);
                vir.GuardarPosicionAltitudViraje(listaVirajes);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private void AltitudVelocidadTHR_Click(object sender, RoutedEventArgs e)
        {
            if (calculosPreliminaresHechos)
            {
                listaTHRAltitudVelocidad.Clear();

                DatosTHR thr = new DatosTHR();
                listaTHRAltitudVelocidad = thr.CalcularAltitudVelocidadTHR(datosAsterix, vuelosOrdenados, listaTHRAltitudVelocidad);
                thr.GuardarAltitudVelocidadTHR(listaTHRAltitudVelocidad);
            }
            else
            {
                MessageBox.Show(
                    $"NO se pueden hacer los cálculos apropiados \n" +
                    $"realizar previamente los cálculos preliminares",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // -------------------------------- FUNCIONES UTILIZADAS EN EL CODIGO ---------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------- 
        public void AcondicionarPV()
        {
            int pistadesp = -1;
            int procdesp = -1;
            int rutasacta = -1;
            for (int i = 0;i < listaPV[0].Count;i++) // Buscamos columnas clave
            {
                if (listaPV[0][i] == "PistaDesp") pistadesp = i;
                if (listaPV[0][i] == "ProcDesp") procdesp = i;
                if (listaPV[0][i] == "RutaSACTA") rutasacta = i;
            }

            for (int i = 1;i < listaPV.Count -1;i++) // Solo dejamos las de 24L y 06R
            {
                if (listaPV[i][pistadesp] != "LEBL-24L" && listaPV[i][pistadesp] != "LEBL-06R")
                {
                    listaPV.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 1; i < listaPV.Count -1; i++)
            {
                if (listaPV[i][procdesp] == "-")
                {
                    var puntos = listaPV[i][rutasacta].Split(' ');
                    bool encontrado1 = false;
                    bool encontrado2 = false;
                    int j = 0;
                    int m = 0;

                    // ------------ QUE HACER CON NITBA ------------
                    var puntosdeseados = new HashSet<string> { "OLOXO" , "NATPI" , "MOPAS" , "GRAUS" , "LOBAR" , "MAMUK" , "REBUL" , "VIBOK" , "DUQQI" , "DUNES" , "LARPA" , "LOTOS" , "SENIA" , "DALIN" , "AGENA" , "DIPES"};
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
            // MessageBox.Show("Planes de vuelo acondicionados");
        }

        public void FiltroDeparturesLEBL()
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

            for (int i = 0;i < listaPV[0].Count; i++)
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

            // MessageBox.Show($"Se han eliminado {rmv} filas");
        }

        private void DefinirPosicionesEstereograficasPuntosfijos()
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            // Calcular Estereograficas de THR_24L
            CoordinatesWGS84 coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:17:31.99N 02:06:11.81E", 8 * 0.3048);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_24L = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de THR_06R
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:56.32N 02:04:27.66E", 8 * 0.3048);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_06R = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de Sonometro
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:19.00N 02:02:52.00E", 8 * 0.3048);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            sonometro = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de DVOR BCN
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:18:25.60N 02:06:28.10E", 8 * 0.3048);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            DVOR_BCN = tma.change_system_cartesian2stereographic(coords_system_cartesian);
        }

        private void CalcularPosicionesEstereograficas()
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            for (int i = 0; i < datosAsterix.Count; i++)
            {
                CoordinatesUVH coords_stereographic = ObtenerCoordsEstereograficas(datosAsterix[i]);
                datosAsterix[i].Add(coords_stereographic.U.ToString());
                datosAsterix[i].Add(coords_stereographic.V.ToString());
            }
        }

        /// <summary>
        /// Función que devuelve las coordenadas Estereográficas de un mensaje Asterix CAT48 que contenga - LAT, LON y ALT -
        /// </summary>
        /// <param name="msg"> Mensaje Asterix del cual extrae las coordenadas Estereográficas </param>
        /// <returns></returns>
        private CoordinatesUVH ObtenerCoordsEstereograficas(List<string> msg)
        {
            // Adaptar valores segun fichero que estamos leyendo
            int LATcol = 4;     // Posición de columna en que se encuentra la variable LAT[º]
            int LONcol = 5;     // Posición de columna en que se encuentra la variable LON[º]
            int ALTcol = 6;     // Posición de columna en que se encuentra la variable Alt[m]

            // Se definen las variables
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            CoordinatesWGS84 coords_geodesic = new CoordinatesWGS84(msg[LATcol], msg[LONcol], Convert.ToDouble(msg[ALTcol]) + rt);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);

            CoordinatesUVH coordsUVH = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            return coordsUVH;
        }

        // HAY QUE REDEFINIR ESTA FUNCION ENTERA
        /* IDEA:
         * 1- Iterar lista de PV par crear los nuevos "Aviones"
         * 2- Cada nuevo TI/TA/TrackNum -> nuevo Avion
         * 3- Bucle en datosAsterix desde 0 hasta encontrar ID -> comprobar si ATOT - Time < 30'
         * 4- Introducir todos los mensajes desde ese punto hasta que no haya durante 100 mensajes
         * 5- Introducir Avion en listaAviones
         */
        private void ClasificarDistintosVuelos()
        {
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
        }

        /// <summary>
        /// Función que ordena los vuelos por hora de despegue (para luego comprobar distancias entre despegues de manera fácil)
        /// </summary>
        private void OrdenarVuelos()
        {
            int ATOTcol = 10;   // Posición de columna en que se encuentra la variable ATOT en excel PlanesVuelo
            int TIcol = 1;      // Posición de columna en que se encuentra la variable TI en excel PlanesVuelo

            List<List<string>> listaPVaux = listaPV;
            listaPVaux.RemoveAt(0);
            listaPVaux = listaPVaux.OrderBy(fila => TimeSpan.Parse(fila[ATOTcol].Insert(fila[ATOTcol].LastIndexOf(':'), ".").Remove(fila[ATOTcol].LastIndexOf(':'), 1)).TotalSeconds).ToList();

            List<Vuelo> vuelosOrdenadosAux = new List<Vuelo>();
            List<string> TA_usados = new List<string>();

            foreach (List<string> msg in listaPVaux)
            {
                if (!TA_usados.Contains(msg[TIcol]))
                {
                    for (int i = 0; i < vuelosOrdenados.Count; i++)
                    {
                        if (vuelosOrdenados[i].codigoVuelo == msg[TIcol])
                        {
                            vuelosOrdenadosAux.Add(vuelosOrdenados[i]);
                            TA_usados.Add(msg[TIcol]);
                            break;
                        }
                    }
                }
            }
            vuelosOrdenados = vuelosOrdenadosAux;
        }

        private double CalcularDistanciaEntrePuntos(Point punto1, Point punto2)
        {
            double dx = punto2.X - punto1.X;
            double dy = punto2.Y - punto1.Y;

            double distancia = Math.Sqrt(dx * dx + dy * dy);

            return distancia;
        }

        private void CalcularTiempo05NMfromTHR()
        {
            int TIcol = 13;     // Posición de columna en que se encuentra la variable TI en csv datosAsterix
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
                int numberOfIteratedMSGvuelo = 0;

                for (int j = 0; j < vuelo.mensajesVuelo.Count - 1; j++)
                {
                    numberOfIteratedMSGvuelo++;
                    posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j][Ycol]));
                    posVuelo_siguienteMSG = new Point(Convert.ToDouble(vuelo.mensajesVuelo[j+1][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[j+1][Ycol]));

                    if (vuelo.pistadesp == "LEBL-24L") {
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
        }

        // -------------------------------- FUNCIONES PARA EXPORTAR DATOS EN FORMATO CSV ----------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------

        private void GuardarMSG_Asterix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de mensajes ASTERIX Actualizados",
                    Filter = "Archivos CSV (*.csv)|*.csv",
                    FileName = "Asterix_Updated.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        foreach (var fila in datosAsterix)
                        {
                            writer.WriteLine(string.Join(";", fila));
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
        }

        private void GuardarMSG_PV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar CSV de Planes de Vuelo Actualizados",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "PV_Updated.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    // ✍️ Escribir el archivo (CSV con extensión XLSX)
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        foreach (var fila in listaPV)
                        {
                            writer.WriteLine(string.Join(";", fila));
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
        }
    }
}
