using ASID.Edge.Mapping;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.PostgreSql;
using ASID.Edge.Services;
using ASID.Edge.Workflows.PUBody.Storage;
using System;

public class StorageService
{
    private readonly ITransactionRepository _repository;

    public StorageService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public PUBodyTransactionHistoryItem Commit(StorageContext context)
    {
        var transaction =
            StorageTransactionMapper.ToTransaction(context);

        var parser = new KanbanParser();

        var kanban = parser.Parse(context.KanbanNo);

        // Generate ONE serial number
        var serial =
            Guid.NewGuid().ToString("N")[..8].ToUpper();

        var item = new PUBodyTransactionHistoryItem
        {
            Model = kanban.Model,
            PartNo = kanban.PartNo,
            SNP = kanban.Quantity,
            KanbanNo = kanban.KanbanNo,

            SerialNo = serial,

            OperatorId = context.OperatorId,
            LineNo = context.CellNo,
            TrolleyNo = context.TrolleyNo,
            LaneNo = context.LaneNo,

            Status = MaterialStatus.Stored,

            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Time = DateTime.Now.ToString("HH:mm:ss")
        };

        transaction.SerialNo = serial;
        transaction.Station = "ST001";
        transaction.Status = MaterialStatus.Stored;

        transaction.Model = item.Model;
        transaction.PartNo = item.PartNo;
        transaction.SNP = item.SNP;
        transaction.Station = "ST001";
        //TODO: Change later to this --
        //transaction.Station = AppConfig.StationId;

        // Save to repository
        _repository.Add(transaction);

        return item;
    }
}