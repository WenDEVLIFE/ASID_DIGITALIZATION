using ASID.Edge.Models;
using ASID.Edge.Repositories.PostgreSql;
using ASID.Edge.Services;
using ASID.Edge.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for DailyDemandControl.xaml
    /// </summary>
    public partial class DailyDemandControl : UserControl
    {
        /// <summary>
        /// Raised after a successful Excel import so the hosting view
        /// can refresh its own dashboard immediately.
        /// </summary>
        public event EventHandler? ImportCompleted;

        public DailyDemandControl()
        {
            InitializeComponent();
        }
        private bool modelAsc = true;
        private bool dateAsc = true;

        public void Load(IEnumerable<PUBodyDailyDemandItem> items)
        {
            DailyDemandGrid.ItemsSource = null;
            DailyDemandGrid.ItemsSource = items.ToList();
        }

        private void SortByModel(object sender, RoutedEventArgs e)
        {
            if (DailyDemandGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(DailyDemandGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Model",
                modelAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            modelAsc = !modelAsc;
        }

        private void SortByDate(object sender, RoutedEventArgs e)
        {
            if (DailyDemandGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(DailyDemandGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Date",
                dateAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            dateAsc = !dateAsc;
        }

        private readonly DailyDemandService _dailyDemandService = new(
            new PostgreSqlDailyDemandRepository());

        private void ImportPlanner_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Production Plan",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var count = _dailyDemandService.ImportExcel(dialog.FileName);

                AutoCloseMessageBox.Show(
                    "Import Successful",
                    $"{count} records imported successfully.");

                ImportCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Import Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
