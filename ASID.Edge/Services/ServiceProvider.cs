using ASID.Edge.Repositories;
using ASID.Edge.Repositories.SQLite;

namespace ASID.Edge.Services
{
    public static class ServiceProvider
    {
        // Background sync: SQLite → PostgreSQL
        public static SyncService? Sync { get; set; }

        public static DashboardService Dashboard { get; } =
            new DashboardService(
                RepositoryProvider.Transactions,
                RepositoryProvider.DailyDemands);


        public static StorageService Storage { get; } =
            new StorageService(
                RepositoryProvider.Transactions);

        public static WithdrawalService Withdrawal { get; } =
            new WithdrawalService(
                RepositoryProvider.Transactions);

        public static P2LoadingBayService P2LoadingBay { get; } =
    new P2LoadingBayService(
        RepositoryProvider.Transactions);

        public static P1LoadingBayService P1LoadingBay { get; } =
new P1LoadingBayService(
RepositoryProvider.Transactions);

        public static P1ProductionService P1Production { get; } =
new P1ProductionService(
RepositoryProvider.Transactions);

        public static NonConformanceService NonConformance { get; } =
        new NonConformanceService(
            RepositoryProvider.Transactions);

        public static AuthService Auth { get; } =
            new AuthService(
                RepositoryProvider.Users);
    }
}