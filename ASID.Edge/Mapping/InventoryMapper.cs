using ASID.Edge.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ASID.Edge.Mapping
{
    public static class InventoryMapper
    {
        public static List<PUBodyInventoryItem> Map(
            IEnumerable<StorageTransaction> transactions,
            IEnumerable<DailyDemand>? dailyDemands = null)
        {
            var demandByModelPartNo = dailyDemands?
                .GroupBy(d => (d.Model, d.PartNo))
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Quantity));

            return transactions

                .GroupBy(x => new
                {
                    x.Model,
                    x.PartNo
                })

                .Select(g =>
                {
                    var item = new PUBodyInventoryItem
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
                    };

                    var key = (g.Key.Model, g.Key.PartNo);
                    if (demandByModelPartNo != null &&
                        demandByModelPartNo.TryGetValue(key, out var demand))
                    {
                        item.P2SupermarketBackground = GetBackgroundBrush(item.InventoryP2Supermarket, demand);
                    }

                    return item;
                })

                .ToList();
        }

        public static Brush? GetBackgroundBrush(int p2, int demand)
        {
            if (demand == 0 || p2 == 0)
                return null;

            double ratio = (double)p2 / demand;

            if (ratio >= 1.01)
                return Brushes.Red;

            if (ratio >= 0.90 && ratio <= 1.00)
                return Brushes.Yellow;

            return null;
        }
    }
}