using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories.Memory
{
    public class MemoryTransactionRepository
            : ITransactionRepository
    {
        private readonly List<StorageTransaction> _transactions = new();

        public void Add(StorageTransaction transaction)
        {
            _transactions.Add(transaction);
        }

        public IReadOnlyList<StorageTransaction> GetAll()
        {
            return _transactions;
        }

        public StorageTransaction? GetByDataMatrix(string dataMatrix)
        {
            return _transactions
                .FirstOrDefault(x => x.DataMatrix == dataMatrix);
        }

        public void Update(StorageTransaction transaction)
        {
            var existing = _transactions
                .FirstOrDefault(x => x.DataMatrix == transaction.DataMatrix);

            if (existing == null)
                return;

            existing.Status = transaction.Status;
            existing.WithdrawnAt = transaction.WithdrawnAt;
            existing.Station = transaction.Station;
        }

        public void Clear()
        {
            _transactions.Clear();
        }
    }
}
