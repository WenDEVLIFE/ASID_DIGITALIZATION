using ASID.Edge.Repositories;
using ASID.Edge.Services;
using ASID.Edge.Views.PUBody;
using System;
using System.Collections.Generic;
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

        private readonly StorageWorkStationView _storage;
        private readonly WithdrawalWorkStationView _withdrawal;
        private readonly P2LoadingBayWorkStationView _p2LoadingBay;
        private readonly P1LoadingBayWorkStationView _p1LoadingBay;
        private readonly P1ProductionWorkStationView _p1Production;

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

            StationHost.Content = _storage;

            var auth = ServiceProvider.Auth;
            TxtSession.Text =
                $"{auth.CurrentUser?.Username ?? "—"} — {auth.CurrentRole}";

            // Subscribe to sync status changes.
            if (ServiceProvider.Sync is SyncService sync)
            {
                sync.SyncCompleted += SyncCompleted;
                sync.NetworkStatusChanged += NetworkStatusChanged;
            }
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

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
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

    }
}
