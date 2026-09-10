using ASID.Edge.Services;
using ASID.Edge.Views.Controls;
using System.Windows.Threading;

namespace ASID.Edge.Views.Controllers
{
    public class DashboardController
    {
        private readonly DashboardService _dashboard;

        private readonly TransactionHistoryControl _transactionHistory;
        private readonly InventoryControl _inventory;
        private readonly WithdrawalControl _withdrawal;
        private readonly DailyDemandControl _dailyDemand;

        private readonly DispatcherTimer _refreshTimer = new();

        public DashboardController(
            DashboardService dashboard,
            TransactionHistoryControl transactionHistory,
            InventoryControl inventory,
            WithdrawalControl withdrawal,
            DailyDemandControl dailyDemand)
        {
            _dashboard = dashboard;

            _transactionHistory = transactionHistory;
            _inventory = inventory;
            _withdrawal = withdrawal;
            _dailyDemand = dailyDemand;

            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (_, _) => Refresh();

            Refresh();          // Initial load
            _refreshTimer.Start();
        }
        public void StartAutoRefresh()
        {
            Refresh();              // Initial load
            _refreshTimer.Start();
        }

        public void StopAutoRefresh()
        {
            _refreshTimer.Stop();
        }

        private string? _lastErrorMessage;

        public void Refresh()
        {
            try
            {
                _transactionHistory.Load(
                    _dashboard.GetTransactionHistory());

                _inventory.Load(
                    _dashboard.GetInventory());

                _withdrawal.Load(
                    _dashboard.GetWithdrawalHistory());

                _lastErrorMessage = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController] Refresh FAILED: {ex}");
                
                if (_lastErrorMessage != ex.Message)
                {
                    _lastErrorMessage = ex.Message;
                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            $"Failed to load Dashboard data from database:\n\n{ex.Message}\n\nType: {ex.GetType().Name}",
                            "Database Load Error",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    });
                }

                // SQLite/transaction queries failed — show empty grids
                _transactionHistory.Load(new List<Models.PUBodyTransactionHistoryItem>());
                _inventory.Load(new List<Models.PUBodyInventoryItem>());
                _withdrawal.Load(new List<Models.PUBodyWithdrawalItem>());
            }

            try
            {
                _dailyDemand.Load(
                    _dashboard.GetDailyDemand());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardController] DailyDemand FAILED: {ex.Message}");
                _dailyDemand.Load(new List<Models.PUBodyDailyDemandItem>());
            }
        }
    }
}