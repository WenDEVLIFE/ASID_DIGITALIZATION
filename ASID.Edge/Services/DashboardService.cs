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

            return demands
                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo,
                    x.ProductionDate
                })
                .Select(g =>
                {
                    var matchingTx = allTransactions.Where(t => t.Model == g.Key.Model && t.PartNo == g.Key.PartNo).ToList();

                    // P2 Inventory = sum of Stored (P2 Supermarket) transactions only
                    int p2Inventory = matchingTx
                        .Where(t => t.Status == MaterialStatus.Stored)
                        .Sum(t => t.SNP);

                    // Delivered to P1 = Received + Consumed, filtered to the current ISO week
                    int deliveredToP1 = matchingTx
                        .Where(t =>
                            (t.Status == MaterialStatus.Received && IsoWeekHelper.IsInCurrentWeek(t.ReceivedAt))
                            || (t.Status == MaterialStatus.Consumed && IsoWeekHelper.IsInCurrentWeek(t.ConsumedAt)))
                        .Sum(t => t.SNP);

                    // Scrapped = NC confirmed quantity + Scrapped status items
                    int scrapped = matchingTx
                        .Where(t => (t.IsNCConfirmed && t.NCQuantity > 0) || t.Status == MaterialStatus.Scrapped)
                        .Sum(t => t.Status == MaterialStatus.Scrapped ? t.SNP : t.NCQuantity);

                    int demand = g.Sum(x => x.Quantity);

                    // Business week label (ISO week + 1, e.g. W39) instead of raw date
                    return new PUBodyDailyDemandItem
                    {
                        Date = IsoWeekHelper.GetBusinessWeekLabel(g.Key.ProductionDate),
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = demand,
                        P2Inventory = p2Inventory,
                        DeliveredToP1 = deliveredToP1,
                        Scrapped = scrapped,
                        P2InventoryBackground = InventoryMapper.GetBackgroundBrush(p2Inventory, demand)
                    };
                })
                .OrderBy(x => x.Model)
                .ToList();
        }
    }
}