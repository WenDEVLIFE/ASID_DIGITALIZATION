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

            return demands
                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo,
                    x.ProductionDate
                })
                .Select(g => new PUBodyDailyDemandItem
                {
                    Date = g.Key.ProductionDate.ToString("yyyy-MM-dd"),
                    Model = g.Key.Model,
                    PartNo = g.Key.PartNo,
                    Demand = g.Sum(x => x.Quantity),

                    P2Inventory = 0,

                    // We'll calculate this later
                    DeliveredToP1 = 0
                })
                .OrderBy(x => x.Model)
                .ToList();
        }
    }
}