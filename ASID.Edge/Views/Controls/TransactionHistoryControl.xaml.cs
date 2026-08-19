using ASID.Edge.Models;
using ASID.Edge.Services;
using ASID.Edge.Views.Controllers;
using ASID.Edge.Views.Dialogs;
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
    /// Interaction logic for TransactionHistoryControl.xaml
    /// </summary>
    public partial class TransactionHistoryControl : UserControl
    {
        public TransactionHistoryControl()
        {
            InitializeComponent();
        }

        private bool modelAsc = true;
        private bool dateAsc = true;

        public void Load(IEnumerable<PUBodyTransactionHistoryItem> items)
        {
            var list = items.ToList();

            TransactionGrid.ItemsSource = null;
            TransactionGrid.ItemsSource = list;

            TxtRecordCount.Text = list.Count.ToString();

            TxtLastRefresh.Text =
                DateTime.Now.ToString("HH:mm:ss");
        }

        private void OnSortByModelClick(object sender, RoutedEventArgs e)
        {
            if (TransactionGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(TransactionGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Model",
                modelAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            modelAsc = !modelAsc;
        }

        private void OnSortByDateClick(object sender, RoutedEventArgs e)
        {
            if (TransactionGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(TransactionGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Date",
                dateAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            dateAsc = !dateAsc;
        }

        private void NonConformance_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog =
                new NonConformanceScanDialog();

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                ServiceProvider
                    .NonConformance
                    .FlagAsSuspected(
                        dialog.DataMatrix);

                AutoCloseMessageBox.Show(
                    "Success",
                    "Material flagged as Suspected NC.");

                //DashboardController.Refresh();
            }
            catch (Exception ex)
            {
                AutoCloseMessageBox.Show(
                    "Error",
                    ex.Message);
            }
        }
    }
}

