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

        public void Refresh()
        {
            _transactionHistory.Load(
                _dashboard.GetTransactionHistory());

            _inventory.Load(
                _dashboard.GetInventory());

            _withdrawal.Load(
                _dashboard.GetWithdrawalHistory());

            _dailyDemand.Load(
                _dashboard.GetDailyDemand());
        }
    }
}