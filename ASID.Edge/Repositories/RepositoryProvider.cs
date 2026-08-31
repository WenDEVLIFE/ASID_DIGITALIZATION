using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.PostgreSql;
using ASID.Edge.Repositories.SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories
{


    public static class RepositoryProvider
    {
        // Storage Transactions — local SQLite (offline-first)
        public static readonly SqliteTransactionRepository SqliteTransactions
            = new();

        public static ITransactionRepository Transactions
            = SqliteTransactions;
        public static IDailyDemandRepository DailyDemands { get; } =
    new PostgreSqlDailyDemandRepository();

        // Users (authentication) — PostgreSQL (shared across all PCs)
        public static IUserRepository Users { get; } =
            new PostgreSqlUserRepository();

        // Lane Management (Supervisor only)
        public static ILaneManagementRepository LaneManagement { get; } =
            new SqliteLaneManagementRepository();


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
