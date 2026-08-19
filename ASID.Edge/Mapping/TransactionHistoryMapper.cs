using ASID.Edge.Models;

namespace ASID.Edge.Mapping
{
    public static class TransactionHistoryMapper
    {
        public static PUBodyTransactionHistoryItem Map(StorageTransaction t)
        {
            return new PUBodyTransactionHistoryItem
            {
                Date = t.CreatedAt.ToString("yyyy-MM-dd"),
                Time = t.CreatedAt.ToString("HH:mm:ss"),
                Model = t.Model,
                PartNo = t.PartNo,
                SNP = t.SNP,
                SerialNo = t.SerialNo,
                LineNo = t.LineNo,
                LaneNo = t.LaneNo,
                TrolleyNo = t.TrolleyNo,
                Status = t.Status,
                IsSuspectedNC = t.IsSuspectedNC


            };
        }
    }
}