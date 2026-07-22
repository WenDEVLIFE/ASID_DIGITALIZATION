using ASID.Edge.Models;
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

            return transaction;
        }
    }
}
