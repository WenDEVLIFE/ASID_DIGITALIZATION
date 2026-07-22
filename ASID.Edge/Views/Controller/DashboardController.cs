using ASID.Edge.Services;
using ASID.Edge.Views.Controls;

namespace ASID.Edge.Views.Controllers
{
    public class DashboardController
    {
        private readonly DashboardService _dashboard;

        private readonly TransactionHistoryControl _transactionHistory;
        private readonly InventoryControl _inventory;
        private readonly WithdrawalControl _withdrawal;
        private readonly DailyDemandControl _dailyDemand;

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