using AsterixParser;
using AsterixParser.Utils;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Management.Deployment;
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

        List<List<string>> datosAsterix = new List<List<string>>();     // Listado de msg Asterix en formato filas de variables
        List<List<string>> listaPV = new List<List<string>>();          // Listado de planes de Vuelo
        
        public class Vuelo                                              // Clase Vuelo, cada despegue es una instancia, contiene el codigo del avion y la lista de mensajes Asterix que le corresponden
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

        List<Vuelo> vuelosOrdenados = new List<Vuelo>();                // Lista de todos los vuelos ya ordenados y filtrados

        // Clase que incluye el vuelo1 (precursor) y vuelo2 (posterior) y el listado de distancias en cada actualización radar
        public class DistanciasDespeguesConsecutivos                    
        {
            public Vuelo vuelo1 { get; set; }
            public Vuelo vuelo2 { get; set; }
            public List<string> listaDistancias { get; set; } = new List<string>();
            public List<string> listaTiemposVuelo1 { get; set; } = new List<string>();
            public List<string> listaTiemposVuelo2 { get; set; } = new List<string>();
        }

        // Lista de conjuntos de distancias de despegues consecutivos
        List<DistanciasDespeguesConsecutivos> listaConjuntosDistanciasDespeguesConsecutivos = new List<DistanciasDespeguesConsecutivos>();

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

            public int ObtenerValor(string perf1, string perf2, string sid)
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
        ClasificacionAeronavesLoA clasificacionAeronavesLoA = new ClasificacionAeronavesLoA();

        // Definicion de distintos puntos fijos
        CoordinatesUVH THR_06R = new CoordinatesUVH();
        CoordinatesUVH THR_24L = new CoordinatesUVH();
        CoordinatesUVH sonometro = new CoordinatesUVH();

        // 
        public class DistanciaMinimaSonometro
        {
            public Vuelo vuelo { get; set; }
            public double distMinSonometro { get; set; }
            public string timeMinSonometro { get; set; }
        }
        List<DistanciaMinimaSonometro> listaDistanciasMinimasSonometro = new List<DistanciaMinimaSonometro>();

        public Proyecto3()
        {
            InitializeComponent();
            DefinirPosicionesEstereograficasPuntosfijos();
        }

        private void DatosAsterix_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;

            datosAsterix = LeerCsvComoLista(path);
        }

        public void Planvuelo_click(object sender, RoutedEventArgs e)
        {

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            string rutaExcel = dialog.FileName;

            listaPV = LeerExcelComoLista(rutaExcel);
            AcondicionarPV();
            FiltroDeparturesLEBL();
            CalcularDistanciasDespeguesConsecutivos();
        }

        private void ClasificacionAeronaves_Click(object sender, RoutedEventArgs e)
        {
            // Lee archivo excel de clasificación aeronaves
            LeerClasificacionAeronaves();
        }

        private void PruebasDEBUG_Click(object sender, RoutedEventArgs e)
        {
            // Calcula posiciones Esterograficas de todos los mensajes Asterix y añade las columnas X e Y a cada mensaje
            CalcularPosicionesEstereograficas();

            // Clasifica los distintos Vuelos segun su identificador TI
            ClasificarDistintosVuelos();

            // Ordena los vuelos segun el ATOT en listaPV, compara segun identificador de vuelo TI
            // OrdenarVuelos();

            // Calcula todas las distancias entre despegues consecutivos, crea clases DistanciasDespeguesConsecutivos
            // y rellena la lista listaConjuntosDistanciasDespeguesConsecutivos (ver definicion)
            CalcularDistanciasDespeguesConsecutivos();
        }

        // Todas las llamadas necesarias para calcular las separciones entre despegues consecutivos
        private void CalculoSeparacionesDespegues_Click(object sender, RoutedEventArgs e)
        {
            CalcularPosicionesEstereograficas();
            ClasificarDistintosVuelos();
            // OrdenarVuelos();
            CalcularDistanciasDespeguesConsecutivos();

            GuardarDistDESPConsecutivos();
        }

        private void DistanciasMinimasSonometro_Click(object sender, RoutedEventArgs e)
        {
            CalcularPosicionesEstereograficas();
            ClasificarDistintosVuelos();
            // OrdenarVuelos();
            CalcularDistanciaMinimaSonometro();

            // Guardar
        }

        // -------------------------------- LECTORES DE ARCHIVOS DE PARAMETROS DE INPUT -----------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------- 

        private List<List<string>> LeerCsvComoLista(string path)
        {
            var resultado = new List<List<string>>();

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

        public static List<List<string>> LeerExcelComoLista(string rutaExcel)
        {
            try
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

                MessageBox.Show("Planes de vuelo leidos correctamente");
                return datos;
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al leer el archivo:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return null;
            }
        }

        private void LeerClasificacionAeronaves()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true)
                    return;

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
            MessageBox.Show("Planes de vuelo acondicionados");
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
            MessageBox.Show($"Se han eliminado {rmv} filas");
        }

        private void DefinirPosicionesEstereograficasPuntosfijos()
        {
            GeoUtils geo = new GeoUtils();
            CoordinatesWGS84 centro_tma = GeoUtils.LatLonStringBoth2Radians("41:06:56.5600N 01:41:33.0100E", 6368942.808);
            GeoUtils tma = new GeoUtils(Math.Sqrt(geo.E2), geo.A, centro_tma);
            double rt = 6356752.3142;

            // Calcular Estereograficas de THR_24L
            CoordinatesWGS84 coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:17:31.99N 02:06:11.81E", 8);
            CoordinatesXYZ coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            CoordinatesXYZ coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_24L = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de THR_06R
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:56.32N 02:04:27.66E", 8);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            THR_06R = tma.change_system_cartesian2stereographic(coords_system_cartesian);

            // Calcular Estereograficas de Sonometro
            coords_geodesic = GeoUtils.LatLonStringBoth2Radians("41:16:19.00N 02:02:52.00E", 8);
            coords_geocentric = tma.change_geodesic2geocentric(coords_geodesic);
            coords_system_cartesian = tma.change_geocentric2system_cartesian(coords_geocentric);
            sonometro = tma.change_system_cartesian2stereographic(coords_system_cartesian);
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

        private void ObtenerATOTvuelo()
        {

        }

        private void ClasificarDistintosVuelos()
        {
            List<string> TI_usados = new List<string>();
            string TI;

            int TIcol = 13;     // Posición de columna en que se encuentra la variable TI en csv datosAsterix
            
            for (int i = 0; i < datosAsterix.Count(); i++)
            {
                if (!TI_usados.Contains(datosAsterix[i][TIcol]))
                {
                    TI = datosAsterix[i][TIcol];

                    Vuelo vuelo = new Vuelo();
                    vuelo.codigoVuelo = TI;
                    vuelo.horaPV = datosAsterix [i][3]; // Columna con la hora del Plan de Vuelo
                    for (int j = 0; j < listaPV.Count(); j++)
                    {
                        if (listaPV[j][1] == datosAsterix[i][TIcol])
                        { 
                            vuelo.estela = listaPV[j][7];
                            vuelo.pistadesp = listaPV[j][11];
                            vuelo.tipo_aeronave = listaPV[j][6];
                            vuelo.sid = listaPV[j][9];
                        }
                    }

                    foreach (List<string> msg in datosAsterix) if (msg[TIcol] == TI) vuelo.mensajesVuelo.Add(msg);

                    vuelosOrdenados.Add(vuelo);
                    TI_usados.Add(datosAsterix[i][TIcol]);
                }
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

        private void CalcularDistanciasDespeguesConsecutivos()
        {
            int TIcol = 13;     // Posición de columna en que se encuentra la variable TI en csv datosAsterix
            int TIMEcol = 3;
            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;

            int tiempo_ms_vuelo1;
            int tiempo_ms_vuelo2;

            Point posTHR_24L = new Point(THR_24L.U, THR_24L.V);
            Point posTHR_06R = new Point(THR_06R.U, THR_06R.V);
            bool condicion05NMvuelo1 = false;
            bool condicion05NMvuelo2 = false;

            for (int i = 0; i < vuelosOrdenados.Count - 1; i++)
            {
                Vuelo vuelo1 = vuelosOrdenados[i];
                Vuelo vuelo2 = vuelosOrdenados[i + 1];

                DistanciasDespeguesConsecutivos distanciasDespeguesConsecutivos = new DistanciasDespeguesConsecutivos();
                distanciasDespeguesConsecutivos.vuelo1 = vuelo1;
                distanciasDespeguesConsecutivos.vuelo2 = vuelo2;

                int numberOfIteratedMSGvuelo1 = 0;
                for (int j = 0; j < datosAsterix.Count - 1; j++)
                {
                    if (datosAsterix[j][TIcol] == vuelo1.codigoVuelo)
                    {
                        numberOfIteratedMSGvuelo1++;
                        Point posVuelo1 = new Point(Convert.ToDouble(datosAsterix[j][Xcol]), Convert.ToDouble(datosAsterix[j][Ycol]));
                        tiempo_ms_vuelo1 = int.Parse(datosAsterix[j][TIMEcol].Split(':')[1]) * 60000 + int.Parse(datosAsterix[j][TIMEcol].Split(':')[2]) * 1000 + int.Parse(datosAsterix[j][TIMEcol].Split(':')[3]);

                        if (vuelo1.pistadesp == "LEBL-24L") condicion05NMvuelo1 = (CalcularDistanciaEntrePuntos(posVuelo1, posTHR_24L) > 1852/2);
                        else condicion05NMvuelo1 = (CalcularDistanciaEntrePuntos(posVuelo1, posTHR_06R) > 1852 / 2);

                        if (condicion05NMvuelo1)
                        {
                            // Iterar sobre los siguientes N vuelos para encontrar la detección simultanea del vuelo2 (N = 5 arbitrario)
                            for (int j2 = j + 1; j2 < Math.Min(j + 5, datosAsterix.Count); j2++)
                            {
                                if (datosAsterix[j2][TIcol] == vuelo2.codigoVuelo && vuelo1.pistadesp == vuelo2.pistadesp)
                                {
                                    tiempo_ms_vuelo2 = int.Parse(datosAsterix[j2][TIMEcol].Split(':')[1]) * 60000 + int.Parse(datosAsterix[j2][TIMEcol].Split(':')[2]) * 1000 + int.Parse(datosAsterix[j2][TIMEcol].Split(':')[3]);
                                    Point posVuelo2 = new Point(Convert.ToDouble(datosAsterix[j2][Xcol]), Convert.ToDouble(datosAsterix[j2][Ycol]));

                                    if (vuelo2.pistadesp == "LEBL-24L") condicion05NMvuelo2 = (CalcularDistanciaEntrePuntos(posVuelo2, posTHR_24L) > 1852 / 2);
                                    else condicion05NMvuelo2 = (CalcularDistanciaEntrePuntos(posVuelo2, posTHR_06R) > 1852 / 2);

                                    if (Math.Abs(tiempo_ms_vuelo2 - tiempo_ms_vuelo1) < 3000 && condicion05NMvuelo2)
                                    {
                                        double distance = CalcularDistanciaEntrePuntos(posVuelo1, posVuelo2);

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
        }

        private bool IncumplimientoRadar(string distancia)
        {
            if (Convert.ToDouble(distancia) < 3 * 1852) { return true; }
            else return false;
        }

        private List<string> IncumplimientoEstela(DistanciasDespeguesConsecutivos conjunto, string distancia)
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

        private void AñadirMotorizacion()
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

        private List<string> IncumplimientoLoA(DistanciasDespeguesConsecutivos conjunto, string distancia)
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

            if (Convert.ToDouble(distancia) < LoA.ObtenerValor(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma)) return ["True", misma, Convert.ToString(LoA.ObtenerValor(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma) / 1852)];
            else return ["False", misma, Convert.ToString(LoA.ObtenerValor(conjunto.vuelo1.motorizacion, conjunto.vuelo2.motorizacion, misma) / 1852)];
        }

        private void CalcularDistanciaMinimaSonometro()
        {

            int Xcol = datosAsterix[1].Count - 2;
            int Ycol = datosAsterix[1].Count - 1;
            int Timecol = 3;
            Point posSonometro = new Point(sonometro.U, sonometro.V);
            Point posVuelo;

            double dist;
            double distMin;
            string timeMin;

            foreach (Vuelo vuelo in vuelosOrdenados)
            {
                if (vuelo.pistadesp == "LEBL-24L")
                {
                    posVuelo = new Point(Convert.ToDouble(vuelo.mensajesVuelo[0][Xcol]), Convert.ToDouble(vuelo.mensajesVuelo[0][Ycol]));
                    distMin = CalcularDistanciaEntrePuntos(posVuelo, posSonometro);
                    timeMin = vuelo.mensajesVuelo[0][Timecol];

                    foreach (List<string> MSG in vuelo.mensajesVuelo)
                    {
                        posVuelo = new Point(Convert.ToDouble(MSG[Xcol]), Convert.ToDouble(MSG[Ycol]));
                        dist = CalcularDistanciaEntrePuntos(posVuelo, posSonometro);

                        if (dist < distMin)
                        {
                            distMin = dist;
                            timeMin = MSG[Timecol];
                        }
                    }

                    DistanciaMinimaSonometro distanciaMinima = new DistanciaMinimaSonometro();
                    distanciaMinima.vuelo = vuelo;
                    distanciaMinima.distMinSonometro = distMin;
                    distanciaMinima.timeMinSonometro = timeMin;

                    listaDistanciasMinimasSonometro.Add(distanciaMinima);
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

        private void GuardarDistDESPConsecutivos()
        {
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

            // csv como nos piden
            try
            {
                var saveFileDialog2 = new SaveFileDialog
                {
                    Title = "Guardar CSV de Distancias Vuelos Consecutivos",
                    Filter = "Archivos CSV (*.xlsx)|*.csv",
                    FileName = "Consecutive Distances corrected.csv",
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
                            AñadirMotorizacion();
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
