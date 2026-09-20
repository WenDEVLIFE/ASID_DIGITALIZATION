using ASID.Edge.Helpers;
using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Services;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Views.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
            RepositoryProvider.DailyDemands);

        // One collection + one view, both created once. ItemsSource is assigned exactly once
        // (constructor) and NEVER reassigned, so week selection, column filters, search text,
        // sort, column widths and scroll survive the 5s auto-refresh and the post-scan refresh.
        private readonly ObservableCollection<PUBodyDailyDemandItem> _items = new();
        private readonly ListCollectionView _view;

        // Filter state (lower-cased snapshots of the UI inputs).
        private DateTime? _selectedWeek;
        private string _filterModel = "";
        private string _filterPartNo = "";
        private string _search = "";
        private bool _suppressWeekChange;
        private double _savedVerticalOffset;

        private static readonly WeekOption AllWeeksOption = new(default, "All weeks");

        private bool modelAsc = true;
        private bool dateAsc = true;

        // Change detection: tracks last known import timestamp
        private DateTime? _lastKnownImport;
        private readonly DispatcherTimer _changeDetectionTimer = new();

        public DailyDemandControl()
        {
            InitializeComponent();

            // Persistent view: assigned once, never reassigned.
            _view = new ListCollectionView(_items) { Filter = Match };
            DailyDemandGrid.ItemsSource = _view;

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

        /// <summary>
        /// Load display items and show workweek label.
        /// Signature is unchanged for callers; rows are mutated into the persistent collection
        /// (CLEAR + ADD) instead of reassigning <c>ItemsSource</c>, so user state survives.
        /// </summary>
        public void Load(IEnumerable<PUBodyDailyDemandItem> items)
        {
            var materialized = items.ToList();

            // Snapshot user state before mutating the collection — a refresh must only
            // update values, never reset filter/sort/width/scroll.
            _savedVerticalOffset = GetVerticalScrollOffset();

            RebuildWeekOptions(materialized);

            _items.Clear();
            foreach (var item in materialized)
                _items.Add(item);

            _view.Refresh();
            RestoreVerticalScrollOffset(_savedVerticalOffset);

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
            _view.SortDescriptions.Clear();
            _view.SortDescriptions.Add(new SortDescription(
                "Model",
                modelAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            modelAsc = !modelAsc;
        }

        private void SortByDate(object sender, RoutedEventArgs e)
        {
            _view.SortDescriptions.Clear();

            // Sort on the canonical Monday week key (not the "W39 (ISO W38)" label),
            // so cross-year weeks order chronologically.
            _view.SortDescriptions.Add(new SortDescription(
                "WeekStart",
                dateAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            dateAsc = !dateAsc;
        }

        /// <summary>
        /// Single composed predicate: week AND Model AND PartNo AND free-text search.
        /// </summary>
        private bool Match(object obj)
        {
            if (obj is not PUBodyDailyDemandItem row)
                return false;

            if (_selectedWeek.HasValue &&
                row.WeekStart.Date != _selectedWeek.Value.Date)
                return false;

            if (!ContainsFilter(_filterModel, row.Model) ||
                !ContainsFilter(_filterPartNo, row.PartNo))
                return false;

            if (_search.Length > 0 &&
                !(row.Model + " " + row.PartNo + " " + row.Date)
                    .ToLowerInvariant()
                    .Contains(_search))
                return false;

            return true;
        }

        private static bool ContainsFilter(string filter, string? cell) =>
            filter.Length == 0 || (cell ?? "").ToLowerInvariant().Contains(filter);

        /// <summary>
        /// Rebuilds the week combo from the loaded rows. Runs under a guard flag so rebuilding
        /// does not fire a filter reset. Keeps the current selection while that week is still
        /// present; otherwise resets to "All weeks".
        /// </summary>
        private void RebuildWeekOptions(IEnumerable<PUBodyDailyDemandItem> items)
        {
            var weeks = items
                .Select(i => i.WeekStart.Date)
                .Where(d => d != default)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var previous = _selectedWeek;

            _suppressWeekChange = true;
            try
            {
                CmbWeek.Items.Clear();
                CmbWeek.Items.Add(AllWeeksOption);
                foreach (var week in weeks)
                    CmbWeek.Items.Add(new WeekOption(
                        week, IsoWeekHelper.GetWeekDisplayLabel(week)));

                var match = previous.HasValue
                    ? CmbWeek.Items.OfType<WeekOption>()
                        .FirstOrDefault(o => o.WeekStart != default &&
                                             o.WeekStart.Date == previous.Value.Date)
                    : null;

                if (match != null)
                {
                    CmbWeek.SelectedItem = match;
                }
                else
                {
                    CmbWeek.SelectedIndex = 0;
                    _selectedWeek = null;
                }
            }
            finally
            {
                _suppressWeekChange = false;
            }
        }

        private void CmbWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressWeekChange)
                return;

            _selectedWeek = CmbWeek.SelectedItem is WeekOption option &&
                            option.WeekStart != default
                ? option.WeekStart.Date
                : null;

            _view.Refresh();
        }

        private void ColumnFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _filterModel = (TxtFilterModel.Text ?? "").Trim().ToLowerInvariant();
            _filterPartNo = (TxtFilterPartNo.Text ?? "").Trim().ToLowerInvariant();
            _view.Refresh();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            _search = (TxtSearch.Text ?? "").Trim().ToLowerInvariant();
            _view.Refresh();
        }

        private double GetVerticalScrollOffset() =>
            FindVisualChild<ScrollViewer>(DailyDemandGrid)?.VerticalOffset ?? 0;

        private void RestoreVerticalScrollOffset(double offset)
        {
            if (offset <= 0)
                return;

            var scrollViewer = FindVisualChild<ScrollViewer>(DailyDemandGrid);
            if (scrollViewer == null)
                return;

            DailyDemandGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(offset);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    return typed;

                var nested = FindVisualChild<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        /// <summary>Combo box entry mapping a week option to its canonical Monday.</summary>
        private sealed class WeekOption
        {
            public DateTime WeekStart { get; }
            public string Label { get; }

            public WeekOption(DateTime weekStart, string label)
            {
                WeekStart = weekStart;
                Label = label;
            }

            public override string ToString() => Label;
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
                        WeekStart = IsoWeekHelper.GetWeekStart(g.Key.ProductionDate),
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
