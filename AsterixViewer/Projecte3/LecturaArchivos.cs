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
            var resultado = new List<List<string>>();

            var dialog = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return resultado;

            string path = dialog.FileName;

            try
            {
                foreach (string linea in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(linea))
                        continue;

                    string[] valores = linea.Split(';'); // Cambiar símbolo de separación si hace falta
                    resultado.Add(new List<string>(valores));
                }

                MessageBox.Show("Datos Asterix leidos correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al leer el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return resultado;
        }

        public List<List<string>> LeerExcelPV()
        {
            var datos = new List<List<string>>();

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return datos;

            string rutaExcel = dialog.FileName;

            try
            {
                // ExcelDataReader necesita este registro para archivos .xlsx
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                datos = new List<List<string>>();

                using (var stream = File.Open(rutaExcel, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Leer el contenido como DataSet
                    var dataSet = reader.AsDataSet();

                    // Tomamos la primera hoja
                    var tabla = dataSet.Tables[0];

                    for (int i = 0; i < tabla.Rows.Count; i++)
                    {
                        var fila = new List<string>();

                        for (int j = 0; j < tabla.Columns.Count; j++)
                        {
                            var valor = tabla.Rows[i][j]?.ToString() ?? string.Empty;
                            fila.Add(valor);
                        }

                        datos.Add(fila);
                    }
                }

                MessageBox.Show("Planes de vuelo leidos correctamente");
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al leer el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return datos;
        }

        public ClasificacionAeronavesLoA LeerClasificacionAeronaves()
        {
            ClasificacionAeronavesLoA clasificacionAeronavesLoA = new ClasificacionAeronavesLoA();

            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true)
                    return clasificacionAeronavesLoA;

                string rutaExcel = dialog.FileName;

                // ExcelDataReader necesita este registro para archivos .xlsx
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                var datos = new List<List<string>>();

                using (var stream = File.Open(rutaExcel, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Leer el contenido como DataSet
                    var dataSet = reader.AsDataSet();

                    // Tomamos la primera hoja
                    var tabla = dataSet.Tables[0];

                    for (int i = 0; i < tabla.Rows.Count; i++)
                    {
                        var fila = tabla.Rows[i];

                        // Si todas las columnas de interés están vacías, paramos
                        if (fila[0] == DBNull.Value && fila[1] == DBNull.Value &&
                            fila[2] == DBNull.Value && fila[3] == DBNull.Value &&
                            fila[4] == DBNull.Value)
                            break;

                        // Añadimos solo si hay datos, evitando string vacío
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

                MessageBox.Show("Clasificacion aeronaves leido correctamente");
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al leer el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return clasificacionAeronavesLoA;
        }

    }
}
