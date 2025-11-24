using AsterixParser;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsterixViewer.Tabs
{
    public partial class TableTab : UserControl
    {
        private DataStore dataStore;
        private FiltersViewModel filtersViewModel;
        private ICollectionView? view;

        public TableTab()
        {
            dataStore = ((App)Application.Current).DataStore;
            filtersViewModel = ((App)Application.Current).FiltersViewModel;

            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void FiltersViewMode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            view?.Refresh();
        }

        private void DataStore_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStore.Messages))
            {
                view = CollectionViewSource.GetDefaultView(dataStore.Messages);
                if (view != null)
                {
                    view.Filter = filtersViewModel.FilterMessages;
                    DataGrid.ItemsSource = view;
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (dataStore == null || dataStore.Messages == null)
                return; // Evita el crash si aún no está inicializado

            filtersViewModel.PropertyChanged += FiltersViewMode_PropertyChanged;
            dataStore.PropertyChanged += DataStore_PropertyChanged;
        }

        private void OnTRDClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var item = btn.DataContext;
                if (item is AsterixMessage message)
                {
                    if (message.TargetReportDescriptor048 == null)
                    {
                        MessageBox.Show("No data", "Target Report Descriptor");
                    }
                    else
                    {
                        MessageBox.Show(string.Join("\n", message.TargetReportDescriptor048), "Target Report Descriptor");
                    }
                }
            }
        }
    }
}
