using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ASID.Edge.Workflows.PUBody.P1LoadingBay;

namespace ASID.Edge.Services
{
    public class P1LoadingBayService
    {
        private readonly ITransactionRepository _repository;

        public P1LoadingBayService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public StorageTransaction Commit(P1LoadingBayContext context)
        {
            var transaction = context.Transaction!;

            transaction.Status = MaterialStatus.Received;
            transaction.Station = "ST004";
            transaction.ReceivedAt = DateTime.Now;

            _repository.Update(transaction);

            return transaction;
        }
    }
}
