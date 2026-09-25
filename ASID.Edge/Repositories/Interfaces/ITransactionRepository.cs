using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        void Add(StorageTransaction transaction);

        StorageTransaction? GetByDataMatrix(string dataMatrix);

        IReadOnlyList<StorageTransaction> GetAll();

        IReadOnlyList<LaneOccupancy> GetLaneOccupancy();

        void Update(StorageTransaction transaction);

        /// <summary>Supervisor correction path: updates the descriptive/identifying fields of an existing transaction (keyed by data_matrix). Does not change lifecycle/NC state.</summary>
        void UpdateDetails(StorageTransaction transaction);

        bool DeleteByDataMatrix(string dataMatrix);
    }
}
