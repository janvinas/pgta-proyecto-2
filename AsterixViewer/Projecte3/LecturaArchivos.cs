using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static AsterixViewer.Projecte3.PerdidasSeparacion;

namespace AsterixViewer.Projecte3
{
    internal class LecturaArchivos
    {

        // Ahora devuelve también la ruta del fichero seleccionado
        public (List<List<string>> data, string filePath) LeerCsvASTERIX()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return (null, null);   // ← CLAVE: usuario canceló

            var resultado = new List<List<string>>();
            string path = dialog.FileName;

            foreach (string linea in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                string[] valores = linea.Split(';');
                resultado.Add(new List<string>(valores));
            }

            return (resultado, path);
        }

        public List<List<string>> ConcatenarDatosAsterix(List<List<string>> previo, List<List<string>> posterior)
        {
            const int colAST_time = 3; // Columna del tiempo
            const int colAST_ID = 13;  // Columna de la ID

            // Función para convertir HH:mm:ss:fff -> milisegundos
            int ParseAsterixTime(string time)
            {
                var p = time.Split(':');
                return int.Parse(p[0]) * 3600000 +
                       int.Parse(p[1]) * 60000 +
                       int.Parse(p[2]) * 1000 +
                       int.Parse(p[3]);
            }

            var concatenado = new List<List<string>>();

            if (previo == null || previo.Count == 0)
                return posterior ?? new List<List<string>>();

            if (posterior == null || posterior.Count == 0)
                return previo ?? new List<List<string>>();

            // Último tiempo del previo
            int tiempoFinalPrevio = ParseAsterixTime(previo.Last()[colAST_time]);

            // Encontrar el índice donde posterior deja de solaparse
            int indexPosteriorStart = 1;

            for (int i = 1; i < posterior.Count; i++)
            {
                int t = ParseAsterixTime(posterior[i][colAST_time]);

                if (t > tiempoFinalPrevio)
                {
                    indexPosteriorStart = i;
                    break;
                }
            }

            // Primero agregamos todo previo
            concatenado.AddRange(previo);

            // Luego agregamos posterior desde el punto no solapado
            for (int i = indexPosteriorStart; i < posterior.Count; i++)
            {
                var fila = posterior[i];
                int tPosterior = ParseAsterixTime(fila[colAST_time]);

                // ⚠ REGLA NUEVA:
                // Si el tiempo es igual, comprobar si la ID también coincide
                if (tPosterior == tiempoFinalPrevio)
                {
                    string idPosterior = fila[colAST_ID];
                    string idPrevio = previo.Last()[colAST_ID];

                    // Si la ID es igual → es duplicado → NO agregar
                    if (idPosterior == idPrevio)
                        continue;
                }

                // Si no es duplicado, se agrega
                concatenado.Add(fila);
            }

            return concatenado;
        }



        public List<List<string>> LeerExcelPV()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return null;   // ← CLAVE

            string rutaExcel = dialog.FileName;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var datos = new List<List<string>>();

            using (var stream = File.Open(rutaExcel, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet();
                var tabla = dataSet.Tables[0];

                for (int i = 0; i < tabla.Rows.Count; i++)
                {
                    var fila = new List<string>();

                    for (int j = 0; j < tabla.Columns.Count; j++)
                        fila.Add(tabla.Rows[i][j]?.ToString() ?? string.Empty);

                    datos.Add(fila);
                }
            }

            return datos;
        }

        public ClasificacionAeronavesLoA LeerClasificacionAeronaves()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return null;   // ← CLAVE

            var clasificacionAeronavesLoA = new ClasificacionAeronavesLoA();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet();
                var tabla = dataSet.Tables[0];

                for (int i = 0; i < tabla.Rows.Count; i++)
                {
                    var fila = tabla.Rows[i];

                    if (fila[0] == DBNull.Value && fila[1] == DBNull.Value &&
                        fila[2] == DBNull.Value && fila[3] == DBNull.Value &&
                        fila[4] == DBNull.Value)
                        break;

                    string valor;

                    valor = fila[0]?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor)) clasificacionAeronavesLoA.HP.Add(valor);

                    valor = fila[1]?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor)) clasificacionAeronavesLoA.NR.Add(valor);

                    valor = fila[2]?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor)) clasificacionAeronavesLoA.NRplus.Add(valor);

                    valor = fila[3]?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor)) clasificacionAeronavesLoA.NRminus.Add(valor);

                    valor = fila[4]?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor)) clasificacionAeronavesLoA.LP.Add(valor);
                }
            }

            return clasificacionAeronavesLoA;
        }
    }
}
