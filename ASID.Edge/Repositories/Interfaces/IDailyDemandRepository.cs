using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface IDailyDemandRepository
    {
        void DeleteAll();

        void Insert(IEnumerable<DailyDemand> demands);

        List<DailyDemand> GetByDate(DateTime date);

        List<DailyDemand> GetAll();
    }
}
