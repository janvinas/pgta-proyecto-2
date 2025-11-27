using AsterixParser;
using AsterixViewer.AsterixMap;
using AsterixViewer.Tabs;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace AsterixViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView.Navigate(typeof(StartTab));
        }

        private async void OpenFile_click(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Archivos ASTERIX|*.ast";
            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;

            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                Debug.WriteLine("Reading file");
                byte[] data = await File.ReadAllBytesAsync(path);
                ProgressBar.Value = 0;
                var progress = new Progress<double>(p => ProgressBar.Value = p);

                var result = await Parser.ParseFileAsync(data, progress);
                ((App)Application.Current).DataStore.Messages = result.messages;
                ((App)Application.Current).DataStore.Flights = result.flights;

                //FinishedLoadingFile?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
    }


}