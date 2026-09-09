using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ASID.Edge.Workflows.PUBody.P1Production;

namespace ASID.Edge.Services
{
    public class P1ProductionService
    {
        private readonly ITransactionRepository _repository;

        public P1ProductionService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public StorageTransaction Commit(P1ProductionContext context)
        {
            var transaction = context.Transaction!;

            transaction.Status = MaterialStatus.Consumed;
            transaction.Station = "ST005";
            transaction.ConsumedAt = DateTime.Now;

            _repository.Update(transaction);

            return transaction;
        }
    }
}
