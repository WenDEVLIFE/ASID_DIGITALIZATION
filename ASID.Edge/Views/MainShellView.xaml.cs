using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Services;
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.PUBody;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace ASID.Edge.Views
{
    public partial class MainShellView : UserControl
    {
        private readonly TcpScannerService _scanner = new();

        /// <summary>Global toast notification panel accessible by all views.</summary>
        public ToastNotification Toasts => ToastsControl;

        private readonly StorageWorkStationView _storage;
        private readonly WithdrawalWorkStationView _withdrawal;
        private readonly P2LoadingBayWorkStationView _p2LoadingBay;
        private readonly P1LoadingBayWorkStationView _p1LoadingBay;
        private readonly P1ProductionWorkStationView _p1Production;
        private readonly DailyDemandControl _dashboard = new();

        /// <summary>Raised when the user clicks Logout; consumed by MainWindow/App.</summary>
        public event EventHandler? LogoutRequested;

        public MainShellView()
        {
            InitializeComponent();

            _storage = new StorageWorkStationView(_scanner);
            _withdrawal = new WithdrawalWorkStationView(_scanner);
            _p2LoadingBay = new P2LoadingBayWorkStationView(_scanner);
            _p1LoadingBay = new P1LoadingBayWorkStationView(_scanner);
            _p1Production = new P1ProductionWorkStationView(_scanner);

            _ = _scanner.StartAsync();

            _storage.Activate();

            StationHost.Content = _dashboard;

            var auth = ServiceProvider.Auth;
            TxtSession.Text =
                $"{auth.CurrentUser?.Username ?? "—"} — {auth.CurrentRole}";

            // Role-based navigation
            var role = auth.CurrentRole;
            bool isPlanner = role == Role.Planner;
            bool isSupervisor = role == Role.Supervisor;
            bool isNonPlanner = !isPlanner;

            // Planner only → Dashboard visible
            LblDashboard.Visibility = isPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnDashboard.Visibility = isPlanner ? Visibility.Visible : Visibility.Collapsed;
            SepDashboard.Visibility = isPlanner ? Visibility.Visible : Visibility.Collapsed;

            // All non-planner roles see stations
            LblPuBody.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnStorage.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnWithdrawal.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnP2LoadingBay.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnP1LoadingBay.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;
            btnP1Production.Visibility = isNonPlanner ? Visibility.Visible : Visibility.Collapsed;

            // Supervisor → System section visible
            LblSystem.Visibility = isSupervisor ? Visibility.Visible : Visibility.Collapsed;
            btnUserManagement.Visibility = isSupervisor ? Visibility.Visible : Visibility.Collapsed;
            btnLaneManagement.Visibility = isSupervisor ? Visibility.Visible : Visibility.Collapsed;

            // Subscribe to sync status changes.
            if (ServiceProvider.Sync is SyncService sync)
            {
                sync.SyncCompleted += SyncCompleted;
                sync.NetworkStatusChanged += NetworkStatusChanged;
            }

            // Welcome toast
            Loaded += (_, _) =>
            {
                var user = auth.CurrentUser?.Username ?? "User";
                var role = auth.CurrentRole;
                Toasts.Success($"Welcome, {user}! Logged in as {role}.", 2500);

                // For Planner/Supervisor, load the dashboard with existing demand data
                switch (auth.CurrentRole)
                {
                    case Role.Planner:
                        LoadDashboard();
                        break;
                    default:
                        // Load the Storage station by default
                        StationHost.Content = _storage;
                        break;
                }
            };
        }

        private void SyncCompleted(int rows)
        {
            Dispatcher.Invoke(() =>
            {
                SyncBorder.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2E, 0x7D, 0x32));
                TxtSyncStatus.Text = $"✓ Synced ({rows})";
            });
        }

        private void NetworkStatusChanged(bool isOnline)
        {
            Dispatcher.Invoke(() =>
            {
                if (isOnline)
                {
                    SyncBorder.Background = new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x2E, 0x7D, 0x32));
                    TxtSyncStatus.Text = "✓ Online";
                }
                else
                {
                    SyncBorder.Background = new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0xD3, 0x2F, 0x2F));
                    TxtSyncStatus.Text = "✗ Offline — data saved locally";
                }
            });
        }

        /// <summary>
        /// Called by MainWindow when the USB scanner detects a barcode.
        /// Forwards it to whatever workstation is currently active.
        /// </summary>
        public void OnUsbBarcodeReceived(string barcode)
        {
            // Route to whichever workstation is in StationHost
            switch (StationHost.Content)
            {
                case StorageWorkStationView ws:
                    ws.AcceptBarcode(barcode);
                    break;
                case WithdrawalWorkStationView ws:
                    ws.AcceptBarcode(barcode);
                    break;
                case P2LoadingBayWorkStationView ws:
                    ws.AcceptBarcode(barcode);
                    break;
                case P1LoadingBayWorkStationView ws:
                    ws.AcceptBarcode(barcode);
                    break;
                case P1ProductionWorkStationView ws:
                    ws.AcceptBarcode(barcode);
                    break;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadDashboard()
        {
            try
            {
                var allDemands = RepositoryProvider.DailyDemands.GetAll();
                System.Diagnostics.Debug.WriteLine($"[LoadDashboard] Got {allDemands.Count} demands from MSSQL");

                var displayItems = allDemands
                    .GroupBy(d => new { d.Model, d.PartNo })
                    .Select(g => new PUBodyDailyDemandItem
                    {
                        Date = $"W{ISOWeek.GetWeekOfYear(g.First().ProductionDate)}",
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = g.Sum(x => x.Quantity),
                        P2Inventory = 0,
                        DeliveredToP1 = 0,
                        Scrapped = g.Sum(x => x.Scrapped)
                    })
                    .OrderBy(x => x.Model)
                    .ToList();

                _dashboard.Load(displayItems);
            }
            catch (Exception ex)
            {
                // DB unreachable — show empty dashboard
                _dashboard.Load(Array.Empty<PUBodyDailyDemandItem>());
                System.Diagnostics.Debug.WriteLine($"LoadDashboard FAILED: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"LoadDashboard error: {ex.Message}", "Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            StationHost.Content = _dashboard;
            LoadDashboard();
        }

        private void btnStorage_Click(object sender, RoutedEventArgs e)
        {
            _storage.Activate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            StationHost.Content = _storage;
        }

        private void btnWithdrawal_Click(object sender, RoutedEventArgs e)
        {

            _storage.Deactivate();
            _withdrawal.Activate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            StationHost.Content = _withdrawal;
        }

        private void btnP2LoadingBay_Click(object sender, RoutedEventArgs e)
        {

            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Activate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            StationHost.Content = _p2LoadingBay;
        }

        private void btnP1LoadingBay_Click(object sender, RoutedEventArgs e)
        {

            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Activate();
            _p1Production.Deactivate();

            StationHost.Content = _p1LoadingBay;
        }

        private void btnP1Production_Click(object sender, RoutedEventArgs e)
        {

            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Activate();

            StationHost.Content = _p1Production;
        }

        private void btnUserManagement_Click(object sender, RoutedEventArgs e)
        {
            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            StationHost.Content = new UserManagementControl();
        }

        private void btnLaneManagement_Click(object sender, RoutedEventArgs e)
        {
            _storage.Deactivate();
            _withdrawal.Deactivate();
            _p2LoadingBay.Deactivate();
            _p1LoadingBay.Deactivate();
            _p1Production.Deactivate();

            var ctrl = new LaneManagementControl();
            StationHost.Content = ctrl;
        }



    }
}
