using AsterixParser.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsterixViewer.Projecte3
{
    internal class CalculosEstereograficos
    {
        public List<List<string>> CalcularPosicionesEstereograficasMatriz(List<List<string>> datos)
        {
            for (int i = 0; i < datos.Count; i++)
            {
                CoordinatesUVH coords_stereographic = ObtenerCoordsEstereograficas(datos[i]);
                datos[i].Add(coords_stereographic.U.ToString());
                datos[i].Add(coords_stereographic.V.ToString());
            }

            return datos;
        }

        /// <summary>
        /// Función que devuelve las coordenadas Estereográficas de un mensaje Asterix CAT48 que contenga - LAT, LON y ALT -
        /// </summary>
        /// <param name="msg"> Mensaje Asterix del cual extrae las coordenadas Estereográficas </param>
        /// <returns></returns>
        public CoordinatesUVH ObtenerCoordsEstereograficas(List<string> msg)
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
    }
}
