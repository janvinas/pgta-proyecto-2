using AsterixViewer.AsterixMap;
using System.Diagnostics;
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

namespace AsterixViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataStore dataStore = new DataStore();
            DataContext = dataStore;
        }

        private void StartTab_FinishedLoadingFile(object sender, EventArgs e)
        {
            // go to table
            TabControl.SelectedIndex = 1;
        }
    }


}