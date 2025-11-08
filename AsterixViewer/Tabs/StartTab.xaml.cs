using AsterixParser;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Interaction logic for StartTab.xaml
    /// </summary>
    public partial class StartTab : UserControl
    {
        public event EventHandler? FinishedLoadingFile;

        public StartTab()
        {
            InitializeComponent();
        }

        private async void OpenFile_click (object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;

            try
            {
                Debug.WriteLine("Reading file");
                byte[] data = await File.ReadAllBytesAsync(path);
                ProgressBar.Value = 0;
                var progress = new Progress<double>(p => ProgressBar.Value = p);

                var result = await Parser.ParseFileAsync(data, progress);
                ((DataStore) DataContext).Messages = result.messages;
                ((DataStore) DataContext).Flights = result.flights;

                FinishedLoadingFile?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
            }
        }

        private void Proyecto3_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;

            List<List<string>> datos = LeerCsvComoLista(path);

            AbrirProyecto3(datos);
        }

        private List<List<string>> LeerCsvComoLista(string path)
        {
            var resultado = new List<List<string>>();

            foreach (string linea in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                // Puedes cambiar ',' por ';' si tu CSV usa punto y coma
                string[] valores = linea.Split(';');
                resultado.Add(new List<string>(valores));
            }

            return resultado;
        }

        private void AbrirProyecto3(List<List<string>> datos)
        {
            // 🔹 Obtenemos la ventana principal
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                // Creamos una nueva instancia del tab Proyecto3
                var proyecto3Tab = new Proyecto3();
                proyecto3Tab.CargarDatos(datos);
            }
        }
    }
}
