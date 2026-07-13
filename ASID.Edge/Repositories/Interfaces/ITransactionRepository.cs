using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        void Add(StorageTransaction transaction);

        IReadOnlyList<StorageTransaction> GetAll();

        void Clear();
    }
}
