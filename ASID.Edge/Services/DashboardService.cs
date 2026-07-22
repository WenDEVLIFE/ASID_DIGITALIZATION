using ASID.Edge.Mapping;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Services
{
    public class DashboardService
    {
        private readonly ITransactionRepository _repository;

        public DashboardService(ITransactionRepository repository)
        {
            _repository = repository;
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

        public List<PUBodyDailyDemandItem> GetDailyDemand()
        {
            return DailyDemandMapper.Map(Transactions);
        }
    }
}