using ASID.Edge.Models;
using ASID.Edge.Services;
using Dapper;
using ASID.Edge.Repositories;
using System.Linq;
using ASID.Edge.Views.Controllers;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Views.Controls;
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
        private List<PUBodyTransactionHistoryItem> _allItems = new();

        public void Load(IEnumerable<PUBodyTransactionHistoryItem> items)
        {
            _allItems = items.ToList();

            ApplyFilter();

            // RBAC gate (UI layer) — handlers re-check defensively.
            NonConformance.IsEnabled =
                ServiceProvider.Auth.CanFlagNC;

            QAReview.IsEnabled =
                ServiceProvider.Auth.CanReviewNC;

            // Override (delete) button — Supervisor only
            BtnOverride.IsEnabled =
                ServiceProvider.Auth.CanOverride;
            BtnOverride.Visibility =
                ServiceProvider.Auth.CanOverride ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = TxtSearch?.Text?.Trim().ToLowerInvariant() ?? "";

            var filtered = string.IsNullOrEmpty(query)
                ? _allItems
                : _allItems.Where(item =>
                    (item.Status.ToString().ToLowerInvariant().Contains(query)) ||
                    (item.Model?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.PartNo?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.SerialNo?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.OperatorId?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.LineNo?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.TrolleyNo?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.LaneNo?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.Date?.ToLowerInvariant().Contains(query) ?? false) ||
                    (item.NCRemarks?.ToLowerInvariant().Contains(query) ?? false)
                ).ToList();

            TransactionGrid.ItemsSource = null;
            TransactionGrid.ItemsSource = filtered;
            TxtRecordCount.Text = filtered.Count.ToString();
            TxtLastRefresh.Text = DateTime.Now.ToString("HH:mm:ss");
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

        private ToastNotification? _toast;
        private ToastNotification Toast => _toast ??= FindToast();
        private ToastNotification FindToast()
        {
            var w = Window.GetWindow(this) as MainWindow;
            return w?.MainShell?.Toasts ?? new ToastNotification();
        }

        private void NonConformance_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ServiceProvider.Auth.CanFlagNC)
            {
                Toast.Warning("You do not have permission to flag NC items.");
                return;
            }

            var passwordDialog = new PasswordDialog();
            if (passwordDialog.ShowDialog() != true)
                return;

            var dialog = new NonConformanceScanDialog();
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                ServiceProvider
                    .NonConformance
                    .FlagAsSuspected(
                        dialog.DataMatrix,
                        dialog.NCQuantity);

                Toast.Success($"Material flagged as Suspected NC (Qty: {dialog.NCQuantity}). Warning symbol added.");
            }
            catch (Exception ex)
            {
                Toast.Error(ex.Message);
            }
        }

        private void QAReview_Click(object sender, RoutedEventArgs e)
        {
            if (!ServiceProvider.Auth.CanReviewNC)
            {
                Toast.Warning("You do not have permission to review NC items.");
                return;
            }

            string prefillDataMatrix = "";
            if (TransactionGrid.SelectedItem is PUBodyTransactionHistoryItem selectedItem)
            {
                prefillDataMatrix = selectedItem.SerialNo;
            }

            var dialog = new QANonConformanceDialog(prefillDataMatrix);
            if (dialog.ShowDialog() == true)
            {
                if (dialog.IsUnflagged)
                {
                    Toast.Success("Material marked as OK. Warning symbol removed.");
                }
                else if (dialog.IsScrapped)
                {
                    Toast.Success($"Material scrapped (Qty: {dialog.ScrapQuantity}). Deducted from inventory.");
                }
            }
        }

        private void Override_Click(object sender, RoutedEventArgs e)
        {
            if (!ServiceProvider.Auth.CanOverride)
            {
                Toast.Warning("Only Supervisor can override (delete) stored data.");
                return;
            }

            if (TransactionGrid.SelectedItem is not PUBodyTransactionHistoryItem selected)
            {
                Toast.Warning("Select a transaction row to override.");
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Delete transaction '{selected.SerialNo}' (Part: {selected.PartNo}, Status: {selected.Status})?\n\nThis action cannot be undone.",
                "Confirm Override",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                // Delete from both SQLite and PostgreSQL
                RepositoryProvider.Transactions.DeleteByDataMatrix(selected.SerialNo);

                Toast.Success($"Transaction '{selected.SerialNo}' deleted.");

                // Refresh the grid from local data
                var all = RepositoryProvider.Transactions.GetAll();
                var historyItems = all.Select(t => new PUBodyTransactionHistoryItem
                {
                    Status = t.Status,
                    Model = t.Model,
                    PartNo = t.PartNo,
                    SerialNo = t.DataMatrix,
                    SNP = t.SNP,
                    OperatorId = t.OperatorId,
                    LineNo = t.LineNo,
                    TrolleyNo = t.TrolleyNo,
                    LaneNo = t.LaneNo,
                    Date = t.CreatedAt.ToString("yyyy-MM-dd"),
                    Time = t.CreatedAt.ToString("HH:mm:ss"),
                    IsSuspectedNC = t.IsSuspectedNC,
                    IsNCConfirmed = t.IsNCConfirmed,
                }).ToList();

                TransactionGrid.ItemsSource = historyItems;
                TxtRecordCount.Text = historyItems.Count.ToString();
                TxtLastRefresh.Text = DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                Toast.Error($"Override failed: {ex.Message}");
            }
        }
    }
}

