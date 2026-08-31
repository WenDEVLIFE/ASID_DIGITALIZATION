using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Workflows.PUBody.Withdrawal;

namespace ASID.Edge.Services
{
    public class WithdrawalService
    {
        private readonly ITransactionRepository _repository;

        public WithdrawalService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public StorageTransaction Commit(WithdrawalContext context)
        {
            var transaction = context.Transaction;

            transaction.Status = MaterialStatus.Withdrawn;
            transaction.Station = "ST002";
            transaction.WithdrawnAt = DateTime.Now;

            _repository.Update(transaction);

            // Update lane_management: increment withdrawn qty for this lane
            try
            {
                if (!string.IsNullOrEmpty(transaction.LaneNo))
                {
                    RepositoryProvider.LaneManagement
                        .IncrementWithdrawnQty(transaction.LaneNo, 1);
                }
            }
            catch { /* lane update is best-effort */ }

            return transaction;
        }
    }
}
