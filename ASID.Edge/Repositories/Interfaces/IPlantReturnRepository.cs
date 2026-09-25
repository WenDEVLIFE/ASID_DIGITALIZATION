using ASID.Edge.Models;
using System;
using System.Collections.Generic;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface IPlantReturnRepository
    {
        void Add(PlantReturn plantReturn);

        IReadOnlyList<PlantReturn> GetAll();

        /// <summary>
        /// Returns the plant returns whose created_at falls inside the work week
        /// starting at <paramref name="weekStart"/> (a Monday).
        /// </summary>
        IReadOnlyList<PlantReturn> GetByWeek(DateTime weekStart);
    }
}
