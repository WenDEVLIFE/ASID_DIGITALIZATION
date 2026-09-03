using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.MsSql;
using System;
using System.Collections.Generic;
using System.Text;
using ASID.Edge.Workflows.PUBody.P2LoadingBay;

namespace ASID.Edge.Services
{
    public class P2LoadingBayService
    {
        private readonly ITransactionRepository _repository;

        public P2LoadingBayService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        public StorageTransaction Commit(P2LoadingBayContext context)
        {
            var transaction = context.Transaction!;

            transaction.Status = MaterialStatus.ForPickup;
            transaction.Station = "ST003";
            transaction.ForPickupAt = DateTime.Now;

            _repository.Update(transaction);
            try { new MssqlTransactionRepository().Update(transaction); } catch { }

            return transaction;
        }
    }
}
