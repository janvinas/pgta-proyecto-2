using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsterixViewer.Tabs
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class Proyecto3 : UserControl
    {
        public Proyecto3()
        {
            InitializeComponent();
        }
        public void CargarDatos(List<List<string>> datos)
        {

        }

        public void Planvuelo_click(object sender, RoutedEventArgs e)
        {
            List<List<string>> datosPV = new List<List<string>>();

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            string rutaExcel = dialog.FileName;

            datosPV = LeerExcelComoLista(rutaExcel);
            datosPV = AcondicionarPV(datosPV);
        }

        public static List<List<string>> LeerExcelComoLista(string rutaExcel)
        {
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
                    var fila = new List<string>();

                    for (int j = 0; j < tabla.Columns.Count; j++)
                    {
                        var valor = tabla.Rows[i][j]?.ToString() ?? string.Empty;
                        fila.Add(valor);
                    }

                    datos.Add(fila);
                }
            }

            return datos;
        }

        public static List<List<string>> AcondicionarPV(List<List<string>> listaPV)
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
                if (listaPV[i][pistadesp] != "LEBL-24L" || listaPV[i][pistadesp] != "LEBL-06R") listaPV.RemoveAt(i);
            }

            for (int i = 1; i < listaPV.Count -1; i++)
            {
                if (listaPV[i][procdesp] == "-")
                {
                    var puntos = listaPV[i][rutasacta].Split(' ');
                    bool encontrado = false;
                    int j = 0;
                    var puntosdeseados = new HashSet<string> { "OLOXO" , "NATPI" , "MOPAS" , "GRAUS" , "LOBAR" , "MAMUK" , "REBUL" , "VIBOK" , "DUQQI" , "DUNES" , "LARPA" , "LOTOS" , "SENIA" , "DALIN" , "AGENA" , "DIPES"};
                    while (!encontrado)
                    {
                        if (puntos[puntos.Length-j-1].Any(a => puntosdeseados.Contains(a.ToString())))
                        {
                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos[puntos.Length - j - 1]}1C");
                            }
                            else if (listaPV[i][procdesp] == "LEBL-06R")
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

                            encontrado = true; 
                            j++;
                        }

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
                                    elementos[l] = listaPV[i][rutasacta][k + l];
                                }
                                puntos2.Add(new string([.. elementos]));
                            }
                        }

                        if (puntos[puntos2.Count - j - 1].Any(a => puntosdeseados.Contains(a.ToString())))
                        {

                            if (listaPV[i][pistadesp] == "LEBL-24L")
                            {
                                listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1C");
                            }
                            else if (listaPV[i][procdesp] == "LEBL-06R")
                            {
                                if (puntos2[puntos2.Count - j - 1] == "OLOXO") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1R");
                                else if (puntos2[puntos2.Count - j - 1] == "NATPI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}2R");
                                else if (puntos2[puntos2.Count - j - 1] == "MOPAS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}3R");
                                else if (puntos2[puntos2.Count - j - 1] == "GRAUS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}3R");
                                else if (puntos2[puntos2.Count - j - 1] == "LOBAR") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}4R");
                                else if (puntos2[puntos2.Count - j - 1] == "MAMUK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}I1R");
                                else if (puntos2[puntos2.Count - j - 1] == "REBUL") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1R");
                                else if (puntos2[puntos2.Count - j - 1] == "VIBOK") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1R");
                                else if (puntos2[puntos2.Count - j - 1] == "DUQQI") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1R");
                                else if (puntos2[puntos2.Count - j - 1] == "DUNES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}4R");
                                else if (puntos2[puntos2.Count - j - 1] == "LARPA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}4R");
                                else if (puntos2[puntos2.Count - j - 1] == "LOTOS") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}3R");
                                else if (puntos2[puntos2.Count - j - 1] == "SENIA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}5R");
                                else if (puntos2[puntos2.Count - j - 1] == "DALIN") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}4R");
                                else if (puntos2[puntos2.Count - j - 1] == "AGENA") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}4R");
                                else if (puntos2[puntos2.Count - j - 1] == "DIPES") listaPV[i][procdesp] = ($"{puntos2[puntos2.Count - j - 1]}1R");
                            }

                            encontrado = true;
                            j++;

                        }
                    }
                        
                }
            }
            Console.WriteLine(listaPV);
            return listaPV;
        }
    }
}
