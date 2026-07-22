using ASID.Edge.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Mapping
{
    public static class WithdrawalMapper
    {
        public static List<PUBodyWithdrawalItem> Map(
            IEnumerable<StorageTransaction> transactions)
        {
            return transactions

                .Where(x => x.WithdrawnAt.HasValue)

                .OrderByDescending(x => x.WithdrawnAt)

                .Select(x => new PUBodyWithdrawalItem
                {
                    Date = x.WithdrawnAt?.ToString("yyyy-MM-dd") ?? "",

                    Time = x.WithdrawnAt?.ToString("HH:mm:ss") ?? "",

                    Model = x.Model,

                    PartNo = x.PartNo,

                    TrolleyNo = x.TrolleyNo,

                    Quantity = x.SNP
                })

                .ToList();
        }
    }
}