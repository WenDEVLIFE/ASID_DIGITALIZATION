using ASID.Edge.Models;
using ASID.Edge.Services;
using ASID.Edge.Workflows.PUBody.Storage;
using System;

namespace ASID.Edge.Mapping
{
    public static class StorageTransactionMapper
    {
        public static StorageTransaction ToTransaction(StorageContext context)
        {
            // Parse the Kanban QR string into structured data
            var parser = new KanbanParser();
            var kanban = parser.Parse(context.KanbanNo);

            return new StorageTransaction
            {
                // Workflow data
                DataMatrix = context.DataMatrix,
                OperatorId = context.OperatorId,
                LineNo = context.LineNo,
                LaneNo = context.LaneNo,
                TrolleyNo = context.TrolleyNo,

                // Parsed Kanban data
                KanbanNo = kanban.KanbanNo,
                Model = kanban.Model,
                PartNo = kanban.PartNo,
                SNP = kanban.Quantity,

                // Transaction metadata
                Station = "ST001",
                Status = MaterialStatus.Stored,
                CreatedAt = DateTime.Now           
            };
        }
    }
}