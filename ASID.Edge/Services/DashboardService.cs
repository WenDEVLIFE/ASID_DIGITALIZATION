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
            return InventoryMapper.Map(Transactions);
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

            // Get latest import timestamp for change detection
            DateTime? lastImportedAt = _dailyDemandRepository.GetLastImportedAt();

            return demands
                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo,
                    x.ProductionDate
                })
                .Select(g =>
                {
                    var matchingTx = Transactions.Where(t => t.Model == g.Key.Model && t.PartNo == g.Key.PartNo).ToList();

                    // P2 Inventory = total quantity in Supermarket Storage
                    int p2Inventory = matchingTx
                        .Where(t => t.Status == MaterialStatus.Stored)
                        .Sum(t => t.SNP);

                    // Delivered to P1 = P1 Loading Bay (Received) + P1 Production (Consumed)
                    int deliveredToP1 = matchingTx
                        .Where(t => t.Status == MaterialStatus.Received || t.Status == MaterialStatus.Consumed)
                        .Sum(t => t.SNP);

                    // Scrapped = NC confirmed quantity
                    int scrapped = matchingTx
                        .Where(t => t.IsNCConfirmed && t.NCQuantity > 0)
                        .Sum(t => t.NCQuantity);

                    return new PUBodyDailyDemandItem
                    {
                        Date = g.Key.ProductionDate.ToString("yyyy-MM-dd"),
                        Model = g.Key.Model,
                        PartNo = g.Key.PartNo,
                        Demand = g.Sum(x => x.Quantity),
                        P2Inventory = p2Inventory,
                        DeliveredToP1 = deliveredToP1,
                        Scrapped = scrapped
                    };
                })
                .OrderBy(x => x.Model)
                .ToList();
        }
    }
}