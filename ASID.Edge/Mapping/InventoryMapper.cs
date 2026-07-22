using ASID.Edge.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Mapping
{
    public static class InventoryMapper
    {
        public static List<PUBodyInventoryItem> Map(
            IEnumerable<StorageTransaction> transactions)
        {
            return transactions

                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo
                })

                .Select(g => new PUBodyInventoryItem
                {
                    Model = g.Key.Model,

                    PartNo = g.Key.PartNo,

                    InventoryP2Supermarket =
                        g.Where(x => x.Status == MaterialStatus.Stored)
                         .Sum(x => x.SNP),

                    InventoryFloating =
                        g.Where(x => x.Status == MaterialStatus.Withdrawn)
                         .Sum(x => x.SNP),

                    InventoryP2LoadingBay =
                        g.Where(x => x.Status == MaterialStatus.ForPickup)
                         .Sum(x => x.SNP),

                    InventoryP1LoadingBay =
                        g.Where(x => x.Status == MaterialStatus.Received)
                         .Sum(x => x.SNP),

                    InventoryP1Production =
                        g.Where(x => x.Status == MaterialStatus.Consumed)
                         .Sum(x => x.SNP),
                })

                .ToList();
        }
    }
}