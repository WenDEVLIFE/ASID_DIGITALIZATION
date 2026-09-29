using ASID.Edge.Helpers;
using ASID.Edge.Mapping;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using ASID.Edge.Repositories;
using System;

namespace ASID.Edge.Services
{
    public class DashboardService
    {
        private readonly ITransactionRepository _repository;
        private readonly IDailyDemandRepository _dailyDemandRepository;

        public DashboardService(
            ITransactionRepository repository,
            IDailyDemandRepository dailyDemandRepository)
        {
            _repository = repository;
            _dailyDemandRepository = dailyDemandRepository;
        }

        private IReadOnlyList<StorageTransaction> Transactions =>
            _repository.GetAll();

        public List<PUBodyTransactionHistoryItem> GetTransactionHistory()
        {
            return Transactions
                .OrderByDescending(x => x.CreatedAt)
                .Select(TransactionHistoryMapper.Map)
                .ToList();
        }

        public List<PUBodyInventoryItem> GetInventory()
        {
            List<DailyDemand>? demands = null;
            try
            {
                demands = _dailyDemandRepository.GetAll();
            }
            catch
            {
                // Demand data is optional for inventory background coloring.
            }

            // The PU-Body Inventory table has no week dimension, so its P2 Supermarket cell
            // color ratio is scoped to the production week containing today (current-week demand).
            var currentWeekStart = IsoWeekHelper.GetWeekStart(DateTime.Today);

            return InventoryMapper.Map(Transactions, demands, currentWeekStart);
        }

        public List<PUBodyWithdrawalItem> GetWithdrawalHistory()
        {
            return WithdrawalMapper.Map(Transactions);
        }

        //public List<PUBodyDailyDemandItem> GetDailyDemand()
        //{
        //    return DailyDemandMapper.Map(Transactions);
        //}

        public List<PUBodyDailyDemandItem> GetDailyDemand()
        {
            var demands = _dailyDemandRepository.GetAll();

            // Try to get transactions for inventory calc, but don't fail if unavailable
            IReadOnlyList<StorageTransaction> allTransactions;
            try { allTransactions = Transactions; }
            catch { allTransactions = new List<StorageTransaction>(); }

            // P2 Inventory basis: cumulative Stored-only units per NORMALIZED Part No across
            // ALL weeks. Repeated on every week row; never week-filtered.
            // Part numbers are normalized on BOTH sides of the join (see NormalizePartNo):
            // the legacy KanbanParser.TrimStart('P') corrupted transactions.part_no
            // (P12345 -> 12345) while the Excel-imported daily_demand kept the prefix, and
            // casing can differ between the two sources. Comparing the normalized forms lets
            // legacy already-corrupted rows still join clean/current ones.
            var p2ByPartNo = allTransactions
                .Where(t => t.Status == MaterialStatus.Stored)
                .GroupBy(t => NormalizePartNo(t.PartNo))
                .ToDictionary(g => g.Key, g => g.Sum(t => t.SNP));

            // Plant returns for the CURRENT week, aggregated by NORMALIZED Part No. Loaded once
            // (not per row) and defensively: a missing table or DB error must not break the
            // dashboard. Normalization matches the transaction join tolerance (legacy 'P'
            // stripping + case differences).
            var currentWeekStart = IsoWeekHelper.GetWeekStart(DateTime.Today);
            Dictionary<string, int> plantReturnsByPartNo = new();
            try
            {
                plantReturnsByPartNo = RepositoryProvider.PlantReturns
                    .GetByWeek(currentWeekStart)
                    .GroupBy(p => NormalizePartNo(p.PartNo))
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
            }
            catch
            {
                // Plant returns are optional for the Scrapped computation.
            }

            return demands
                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo,
                    // Canonical week key: Monday of the production week. Normalizing here (instead of
                    // grouping on the raw ProductionDate) keeps rows correct across a year boundary.
                    WeekStart = IsoWeekHelper.GetWeekStart(x.ProductionDate)
                })
                .Select(g =>
                {
                    // Match transactions by NORMALIZED Part No ONLY. The PU-Body part number is
                    // the real join key; Model is display-only and is deliberately NOT required
                    // to match (daily_demand.Model and transactions.Model can differ in casing
                    // or format, and the Plant Return feature already matches by Part No alone).
                    string normalizedPartNo = NormalizePartNo(g.Key.PartNo);

                    var matchingTx = allTransactions
                        .Where(t => NormalizePartNo(t.PartNo) == normalizedPartNo)
                        .ToList();

                    // P2 Inventory = cumulative Stored-only total (all weeks), repeated per week row.
                    int p2Inventory = p2ByPartNo.TryGetValue(normalizedPartNo, out int storedTotal) ? storedTotal : 0;

                    // Delivered to P1 = units whose P1-loading-bay receipt falls inside THIS week,
                    // regardless of any later status advance (Received -> Consumed previously made
                    // already-received units vanish). Rows with no received_at fall back to the
                    // consumed timestamp so units that skipped the loading-bay step still count.
                    int deliveredToP1 = matchingTx
                        .Where(t => IsoWeekHelper.IsInWeek(t.ReceivedAt, g.Key.WeekStart)
                                 || (t.ReceivedAt == null && IsoWeekHelper.IsInWeek(t.ConsumedAt, g.Key.WeekStart)))
                        .Sum(t => t.SNP);

                    // Scrapped = NC confirmed quantity + Scrapped status items, attributed to THIS week
                    // via CreatedAt with UpdatedAt as fallback (there is no scrapped_at column).
                    int scrapped = matchingTx
                        .Where(t => (t.IsNCConfirmed && t.NCQuantity > 0) || t.Status == MaterialStatus.Scrapped)
                        .Where(t => IsoWeekHelper.IsInWeek(ScrapTimestamp(t), g.Key.WeekStart))
                        .Sum(t => t.Status == MaterialStatus.Scrapped ? t.SNP : t.NCQuantity);

                    // Plant returns are absorbed into Scrapped ONLY for the current week, matched by
                    // NORMALIZED Part No (the Plant Return form has no Model field). Other weeks are
                    // unaffected.
                    if (g.Key.WeekStart == currentWeekStart
                        && plantReturnsByPartNo.TryGetValue(normalizedPartNo, out int plantReturnQty))
                    {
                        scrapped += plantReturnQty;
                    }

                    int demand = g.Sum(x => x.Quantity);

                    // Color ratio per row = cumulative Stored P2 / THIS row's week demand
                    // (not the all-week total). Yellow 90-100%, Red >= 101%, none when demand 0.
                    return new PUBodyDailyDemandItem
                    {
                        WeekStart = g.Key.WeekStart,
                        // Business week label only, e.g. "W39" (ISO value intentionally hidden).
                        Date = IsoWeekHelper.GetWeekDisplayLabel(g.Key.WeekStart),
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = demand,
                        P2Inventory = p2Inventory,
                        DeliveredToP1 = deliveredToP1,
                        Scrapped = scrapped,
                        P2InventoryBackground = InventoryMapper.GetBackgroundBrush(p2Inventory, demand)
                    };
                })
                // Default order: newest production workweek first, then Model/PartNo within
                // the same week so the grid stays grouped and readable.
                .OrderByDescending(x => x.WeekStart)
                .ThenBy(x => x.Model)
                .ThenBy(x => x.PartNo)
                .ToList();
        }

        /// <summary>
        /// Normalizes a PU-Body part number for joining transactions against
        /// daily_demand. The legacy KanbanParser stripped leading 'P' characters from
        /// scanned part numbers, so the two tables can hold the same part with and
        /// without that prefix. Comparison is therefore whitespace-trimmed,
        /// leading-'P'-tolerant and case-insensitive on BOTH sides.
        /// </summary>
        private static string NormalizePartNo(string? value) =>
            (value ?? string.Empty).Trim().TrimStart('P', 'p').ToUpperInvariant();

        /// <summary>
        /// Week attribution timestamp for scrapped units: CreatedAt when set, otherwise UpdatedAt.
        /// Returns null when neither is available so the row is only visible in the all-weeks view.
        /// </summary>
        private static DateTime? ScrapTimestamp(StorageTransaction transaction)
        {
            if (transaction.CreatedAt != default(DateTime))
                return transaction.CreatedAt;

            return transaction.UpdatedAt;
        }
    }
}