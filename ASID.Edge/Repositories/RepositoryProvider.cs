using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.PostgreSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories
{


    public static class RepositoryProvider
    {
        // Storage Transactions (used by workflows)
        public static ITransactionRepository Transactions
            = new PostgreSqlTransactionRepository();
        public static IDailyDemandRepository DailyDemands { get; } =
    new PostgreSqlDailyDemandRepository();


        // Shared UI Data
        public static List<PUBodyTransactionHistoryItem> TransactionHistory { get; }
            = new();

        public static List<PUBodyInventoryItem> Inventory { get; }
            = new();

        public static List<PUBodyWithdrawalItem> Withdrawal { get; }
            = new();

        public static List<PUBodyDailyDemandItem> DailyDemand { get; }
            = new();
    }
}
