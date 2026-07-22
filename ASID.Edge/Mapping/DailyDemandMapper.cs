using ASID.Edge.Models;
using System.Collections.Generic;

namespace ASID.Edge.Mapping
{
    public static class DailyDemandMapper
    {
        public static List<PUBodyDailyDemandItem> Map(
            IEnumerable<StorageTransaction> transactions)
        {
            return new List<PUBodyDailyDemandItem>();

            // We'll implement this later.
        }
    }
}