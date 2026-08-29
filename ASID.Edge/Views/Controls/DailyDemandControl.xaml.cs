using ASID.Edge.Helpers;
using ASID.Edge.Models;
using ASID.Edge.Repositories.PostgreSql;
using ASID.Edge.Services;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Views.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for DailyDemandControl.xaml
    /// </summary>
    public partial class DailyDemandControl : UserControl
    {
        public event EventHandler? ImportCompleted;

        private readonly DailyDemandService _dailyDemandService = new(
            new PostgreSqlDailyDemandRepository());

        private bool modelAsc = true;
        private bool dateAsc = true;

        // Change detection: tracks last known import timestamp
        private DateTime? _lastKnownImport;
        private readonly DispatcherTimer _changeDetectionTimer = new();

        public DailyDemandControl()
        {
            InitializeComponent();

            // Poll for demand changes every 30 seconds
            _changeDetectionTimer.Interval = TimeSpan.FromSeconds(30);
            _changeDetectionTimer.Tick += ChangeDetectionTimer_Tick;
        }

        /// <summary>Start change detection polling.</summary>
        public void StartChangeDetection()
        {
            _lastKnownImport = _dailyDemandService.GetLastImportTimestamp();
            _changeDetectionTimer.Start();
        }

        /// <summary>Stop change detection polling.</summary>
        public void StopChangeDetection()
        {
            _changeDetectionTimer.Stop();
        }

        private void ChangeDetectionTimer_Tick(object? sender, EventArgs e)
        {
            if (_dailyDemandService.HasDataChanged(_lastKnownImport))
            {
                _lastKnownImport = _dailyDemandService.GetLastImportTimestamp();
                ShowChangeBanner();
            }
        }

        private void ShowChangeBanner()
        {
            ChangeBanner.Visibility = Visibility.Visible;
            ChangeText.Text = "\u26a0\ufe0f  Demand data has been updated by the planner. Click \"Import Production Plan\" to refresh.";
        }

        private void HideChangeBanner()
        {
            ChangeBanner.Visibility = Visibility.Collapsed;
        }

        /// <summary>Load display items and show workweek label.</summary>
        public void Load(IEnumerable<PUBodyDailyDemandItem> items)
        {
            DailyDemandGrid.ItemsSource = null;
            DailyDemandGrid.ItemsSource = items.ToList();

            // RBAC gate (UI layer) — handler re-checks defensively.
            ImportPlannerButton.IsEnabled =
                ServiceProvider.Auth.CanImportDemand;

            // Update change detection baseline
            _lastKnownImport = _dailyDemandService.GetLastImportTimestamp();
            HideChangeBanner();
        }

        /// <summary>Load items with a workweek label header.</summary>
        public void LoadWithWorkweek(
            IEnumerable<PUBodyDailyDemandItem> items,
            string workweekLabel)
        {
            if (!string.IsNullOrWhiteSpace(workweekLabel))
            {
                WorkweekBanner.Visibility = Visibility.Visible;
                WorkweekText.Text = $"Production Workweek: {workweekLabel}";
            }
            else
            {
                WorkweekBanner.Visibility = Visibility.Collapsed;
            }

            Load(items);
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

        private ToastNotification? _toast;
        private ToastNotification Toast => _toast ??= FindToast();
        private ToastNotification FindToast()
        {
            var w = Window.GetWindow(this) as MainWindow;
            return w?.MainShell?.Toasts ?? new ToastNotification();
        }

        private void ImportPlanner_Click(object sender, RoutedEventArgs e)
        {
            if (!ServiceProvider.Auth.CanImportDemand)
            {
                MessageBox.Show("You do not have permission to import production plans.",
                    "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Import Production Plan",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var result = _dailyDemandService.ImportExcel(dialog.FileName);

                if (result.Demands.Count == 0)
                {
                    MessageBox.Show(
                        "No demand records found in the Excel file.\n\n" +
                        "The importer looks for a sheet with 'Work Week' or 'Serial Production' headers.\n" +
                        "Data rows must have a non-empty Model (Col B) and a numeric Demand (Col E).",
                        "Import Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var displayItems = result.Demands
                    .GroupBy(x => new
                    {
                        x.Model,
                        x.PartNo,
                        x.ProductionDate
                    })
                    .Select(g => new PUBodyDailyDemandItem
                    {
                        Date = result.WorkweekLabel,
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = g.Sum(x => x.Quantity),
                        P2Inventory = 0,
                        DeliveredToP1 = 0,
                        Scrapped = g.Sum(x => x.Scrapped)
                    })
                    .OrderBy(x => x.Model)
                    .ToList();

                LoadWithWorkweek(displayItems, result.WorkweekLabel);

                MessageBox.Show(
                    $"Successfully imported {result.Demands.Count} records for {result.WorkweekLabel}.\n\n" +
                    $"Displaying {displayItems.Count} grouped items.",
                    "Import Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Import failed: {ex.Message}\n\n{ex.StackTrace}",
                    "Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
