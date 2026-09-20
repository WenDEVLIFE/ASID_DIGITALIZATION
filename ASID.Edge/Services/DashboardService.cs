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
                .OrderBy(x => x.CreatedAt)
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

            return InventoryMapper.Map(Transactions, demands);
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

            // P2 Inventory basis: cumulative Stored-only units per Model+PartNo across ALL weeks.
            // Repeated on every week row; never week-filtered.
            var p2ByModelPartNo = allTransactions
                .Where(t => t.Status == MaterialStatus.Stored)
                .GroupBy(t => (t.Model, t.PartNo))
                .ToDictionary(g => g.Key, g => g.Sum(t => t.SNP));

            // Color denominator: TOTAL all-week demand per Model+PartNo (consistent with the gate basis).
            var totalDemandByModelPartNo = demands
                .GroupBy(d => (d.Model, d.PartNo))
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Quantity));

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
                    var key = (g.Key.Model, g.Key.PartNo);

                    var matchingTx = allTransactions
                        .Where(t => t.Model == g.Key.Model && t.PartNo == g.Key.PartNo)
                        .ToList();

                    // P2 Inventory = cumulative Stored-only total (all weeks), repeated per week row.
                    int p2Inventory = p2ByModelPartNo.TryGetValue(key, out int storedTotal) ? storedTotal : 0;

                    // Delivered to P1 = Received + Consumed attributed to THIS production week.
                    int deliveredToP1 = matchingTx
                        .Where(t =>
                            (t.Status == MaterialStatus.Received && IsoWeekHelper.IsInWeek(t.ReceivedAt, g.Key.WeekStart))
                            || (t.Status == MaterialStatus.Consumed && IsoWeekHelper.IsInWeek(t.ConsumedAt, g.Key.WeekStart)))
                        .Sum(t => t.SNP);

                    // Scrapped = NC confirmed quantity + Scrapped status items, attributed to THIS week
                    // via CreatedAt with UpdatedAt as fallback (there is no scrapped_at column).
                    int scrapped = matchingTx
                        .Where(t => (t.IsNCConfirmed && t.NCQuantity > 0) || t.Status == MaterialStatus.Scrapped)
                        .Where(t => IsoWeekHelper.IsInWeek(ScrapTimestamp(t), g.Key.WeekStart))
                        .Sum(t => t.Status == MaterialStatus.Scrapped ? t.SNP : t.NCQuantity);

                    int demand = g.Sum(x => x.Quantity);

                    // Cumulative color ratio uses the all-week demand denominator, not this week's demand.
                    int totalDemand = totalDemandByModelPartNo.TryGetValue(key, out int allWeekDemand)
                        ? allWeekDemand
                        : demand;

                    // Dual-convention week label, e.g. "W39 (ISO W38)".
                    return new PUBodyDailyDemandItem
                    {
                        WeekStart = g.Key.WeekStart,
                        Date = IsoWeekHelper.GetWeekDisplayLabel(g.Key.WeekStart),
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = demand,
                        P2Inventory = p2Inventory,
                        DeliveredToP1 = deliveredToP1,
                        Scrapped = scrapped,
                        P2InventoryBackground = InventoryMapper.GetBackgroundBrush(p2Inventory, totalDemand)
                    };
                })
                .OrderBy(x => x.Model)
                .ThenBy(x => x.PartNo)
                .ThenBy(x => x.WeekStart)
                .ToList();
        }

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