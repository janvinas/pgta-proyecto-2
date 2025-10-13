using AsterixParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsterixViewer.Tabs
{
    public partial class TableTab : UserControl
    {
        private DataStore dataStore;
        private ICollectionView view;

        public TableTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            dataStore = DataContext as DataStore;
            if (dataStore == null || dataStore.messages == null)
                return; // Evita el crash si aún no está inicializado

            view = CollectionViewSource.GetDefaultView(dataStore.messages);
            if (view != null)
            {
                view.Filter = FilterMessages;
                DataGrid.ItemsSource = view;
            }
        }


        private bool FilterMessages(object obj)
        {
            if (obj is not AsterixMessage msg)
                return false;

            // Filtrado por categoría
            if (msg.Cat == CAT.CAT021 && !(Cat021Filter?.IsChecked ?? true))
                return false;
            if (msg.Cat == CAT.CAT048 && !(Cat048Filter?.IsChecked ?? true))
                return false;
            if (msg.Mode3A == 4095 && !(TransponderFijoFilter?.IsChecked ?? true))
                return false;

            // --- NUEVO: comprobar TargetReportDescriptor (lista de strings) ---
            // Si no existe la lista o está vacía => quitar
            if (BlancoPuroFilter?.IsChecked ?? false)
            {
                if (msg.TargetReportDescriptor == null || msg.TargetReportDescriptor.Count == 0)
                    return false;

                // Solo miramos el primer elemento
                var first = msg.TargetReportDescriptor[0];
                if (string.IsNullOrEmpty(first) ||
                    (first.IndexOf("PSR", StringComparison.OrdinalIgnoreCase) < 0 &&
                     first.IndexOf("SSR", StringComparison.OrdinalIgnoreCase) < 0))
                {
                    return false;
                }
            }


            return true;
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            // Refrescar la vista cuando cambie un filtro
            view?.Refresh();
        }

        private void OnTRDClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var item = btn.DataContext;
                if (item is AsterixMessage message)
                {
                    if (message.TargetReportDescriptor == null)
                    {
                        MessageBox.Show("No data", "Target Report Descriptor");
                    }
                    else
                    {
                        MessageBox.Show(string.Join("\n", message.TargetReportDescriptor), "Target Report Descriptor");
                    }
                }
            }
        }
    }
}
