using ASID.Edge.Models;
using ASID.Edge.Workflows.PUBody.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Mapping
{
    public static class StorageTransactionMapper
    {
        public static StorageTransaction ToTransaction(StorageContext context)

        {
            return new StorageTransaction
            {
                DataMatrix = context.DataMatrix,

                OperatorId = context.OperatorId,

                KanbanNo = context.KanbanNo,

                LineNo = context.LineNo,

                TrolleyNo = context.TrolleyNo,

                LaneNo = context.LaneNo,

                CreatedAt = DateTime.Now
            };
        }
    }
}
