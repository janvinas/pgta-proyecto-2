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

        public List<List<string>> LeerCsvASTERIX()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return null;   // ← CLAVE

            var resultado = new List<List<string>>();
            string path = dialog.FileName;

            foreach (string linea in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                string[] valores = linea.Split(';');
                resultado.Add(new List<string>(valores));
            }

            return resultado;
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
