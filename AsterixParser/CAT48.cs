using AsterixParser.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AsterixParser
{
    internal class CAT48(byte[] body, AsterixMessage message)
    {
        public int CAT48Reader(int i, ushort length) 
        {
            int error = 0;

            Queue<int> vFSPEC = FSPECReader();

            int errorDI = DataItem(vFSPEC);


            return error;
        }

        int k = 0; //Byte en el que estas del body (como un punto de libro)

        public Queue<int> FSPECReader()
        {
            int error = 0;

            bool fx = true;

            //vFSPEC es la vector de bits de FSPEC de mida variable (seguro que hay mejor manera)
            Queue<int> vFSPEC = new Queue<int>();
            //nFSPEC es la vector de numeros de FSPEC aparecen de FSPEC(seguro que hay mejor manera)
            Queue<int> nFSPEC = new Queue<int>();

            int FRN = 1;

            while (fx || FRN > 22) {
                byte b = body[k];
                for (int i = 7; i > 0; i--)
                {
                    int v = (b >> i) & 1;
                    if(v == 1) nFSPEC.Enqueue(FRN);
                    vFSPEC.Enqueue(v);
                    FRN++;
                }

                int bit = b & 1;
                if (bit == 0) fx = false;
                k++;
            }
            
            return nFSPEC;
        }

        public int DataItem(Queue<int> nFSPEC)
        {
            int error = 0;
            var parser = new DataItemParser48(message);
            
            while (nFSPEC.Count > 0)
            { 
                int n = nFSPEC.Dequeue();
                //Console.WriteLine(n);
                //En un array de funciones le pasa el numero del DataField que quiere decodificar
                parser.functions[n-1](ref k, body); 
            }

            ComputeCoordinates(message);

            return error;
        }

        private static readonly GeoUtils geoUtils = new();
        private static readonly CoordinatesWGS84 radar = new(41.30070222222222 * GeoUtils.DEGS2RADS, 2.1020581944444445 * GeoUtils.DEGS2RADS, 2.007 + 25.25);
        private static void ComputeCoordinates(AsterixMessage message)
        {
            if (message == null) return;
            if (message.Distance == null || message.Azimuth == null || message.FlightLevel?.flightLevel == null) return;

            double elevation = GeoUtils.CalculateElevation(
                radar,
                geoUtils.CalculateEarthRadius(radar),
                message.Distance.Value * GeoUtils.NM2METERS,
                message.FlightLevel.flightLevel.Value * 100 * GeoUtils.FEET2METERS
                );

            var radarSpherical = new CoordinatesPolar(message.Distance.Value * GeoUtils.NM2METERS, message.Azimuth.Value * GeoUtils.DEGS2RADS, elevation);
            var radarCartesian = GeoUtils.change_radar_spherical2radar_cartesian(radarSpherical);

            var geocentric = geoUtils.change_radar_cartesian2geocentric(radar, radarCartesian);
            var coordinates = geoUtils.change_geocentric2geodesic(geocentric);

            message.Latitude = coordinates.Lat * GeoUtils.RADS2DEGS;
            message.Longitude = coordinates.Lon * GeoUtils.RADS2DEGS;
        }
    }
}
