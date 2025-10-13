using AsterixParser;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for TableTab.xaml
    /// </summary>
    public partial class TableTab : UserControl
    {
        public TableTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataStore dataStore = (DataStore)DataContext;
            DataGrid.ItemsSource = dataStore.messages;
        }

        private void OnTRDClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var item = btn.DataContext;
                if (item is AsterixMessage message)
                {
                    if (message.TargetReportDescriptor == null) return;
                    MessageBox.Show(String.Join("\n", message.TargetReportDescriptor), "Target Report Descriptor");
                }
            }
        }
    }
}
