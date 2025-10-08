using AsterixViewer.AsterixMap;
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
        public void SetTab(int index)
        {
            Dispatcher.BeginInvoke((Action)(() => TabControl.SelectedIndex = index));
        }
    }


}