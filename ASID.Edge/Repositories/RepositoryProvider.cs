using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.MsSql;
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

        public static ITransactionRepository Transactions { get; } = CreateTransactionRepo();
        public static IDailyDemandRepository DailyDemands { get; } = CreateDailyDemandRepo();

        // Users (authentication) — PostgreSQL or MSSQL
        public static IUserRepository Users { get; } =
            CreateUserRepo();

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

        private static ITransactionRepository CreateTransactionRepo()
        {
            string engine = Database.Database.Engine;
            if (engine.Equals("mssql", StringComparison.OrdinalIgnoreCase))
                return new MssqlTransactionRepository();
            return new PostgreSqlTransactionRepository();
        }

        private static IDailyDemandRepository CreateDailyDemandRepo()
        {
            string engine = Database.Database.Engine;
            if (engine.Equals("mssql", StringComparison.OrdinalIgnoreCase))
                return new MssqlDailyDemandRepository();
            return new PostgreSqlDailyDemandRepository();
        }

        private static IUserRepository CreateUserRepo()
        {
            string engine = Database.Database.Engine;
            if (engine.Equals("mssql", StringComparison.OrdinalIgnoreCase))
                return new MssqlUserRepository();
            return new PostgreSqlUserRepository();
        }
    }
}
