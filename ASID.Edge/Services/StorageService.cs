using ASID.Edge.Mapping;
using ASID.Edge.Models;
using ASID.Edge.Repositories;
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

    public string? LastLaneWarning { get; private set; }

    public PUBodyTransactionHistoryItem Commit(StorageContext context)
    {
        LastLaneWarning = null;

        var transaction =
            StorageTransactionMapper.ToTransaction(context);

        var parser = new KanbanParser();

        var kanban = parser.Parse(context.KanbanNo);

        // Reuse context.SerialNo if generated during Print(), otherwise generate one
        var serial = !string.IsNullOrWhiteSpace(context.SerialNo)
            ? context.SerialNo
            : Guid.NewGuid().ToString("N")[..8].ToUpper();

        var item = new PUBodyTransactionHistoryItem
        {
            Model = kanban.Model,
            PartNo = kanban.PartNo,
            SNP = kanban.Quantity,
            KanbanNo = kanban.KanbanNo,

            SerialNo = serial,

            DataMatrix = context.DataMatrix,

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

        // Save to MSSQL directly
        _repository.Add(transaction);

        // Update lane_management: increment stored qty for this lane
        try
        {
            RepositoryProvider.LaneManagement
                .IncrementStoredQty(context.LaneNo, kanban.PartNo, 1);
        }
        catch (Exception ex)
        {
            LastLaneWarning = $"Lane record update failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Lane record update failed: {ex}");
        }

        return item;
    }
}