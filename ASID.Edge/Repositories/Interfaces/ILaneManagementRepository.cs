using ASID.Edge.Models;
using System.Collections.Generic;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface ILaneManagementRepository
    {
        IReadOnlyList<LaneManagement> GetAll();
        LaneManagement? GetByLaneNo(string laneNo);
        void Add(LaneManagement lane);
        void Update(LaneManagement lane);
        void Delete(int id);
        void SeedDefaultLanes();
    }
}
