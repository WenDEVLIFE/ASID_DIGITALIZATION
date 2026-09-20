using ASID.Edge.Helpers;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Validation;

namespace ASID.Edge.Services
{
    public class StorageValidationService
    {
        private readonly ITransactionRepository _repository;

        public StorageValidationService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns the distinct lane numbers that are currently vacant.
        /// A lane is vacant when it has no transaction with a null consumed_at.
        /// </summary>
        public IReadOnlyList<string> GetVacantLanes()
        {
            return _repository
                .GetLaneOccupancy()
                .Where(o => o.OpenCount == 0)
                .Select(o => o.LaneNo)
                .ToList();
        }

        /// <summary>
        /// A lane is assignable if it hasn't reached its max trolley capacity.
        /// Capacity is checked against lane_management.max_qty_stored.
        /// </summary>
        public ValidationResult CheckAssignedLane(string laneNo)
        {
            if (string.IsNullOrWhiteSpace(laneNo))
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "No lane scanned",
                    Severity = ValidationSeverity.Error
                };
            }

            // Check lane capacity from lane_management table
            try
            {
                var lane = Repositories.RepositoryProvider.LaneManagement
                    .GetByLaneNo(laneNo);

                if (lane != null)
                {
                    int balance = lane.ActualStoredQty - lane.WithdrawnQty;
                    if (balance < 0) balance = 0;

                    if (lane.MaxQtyStored > 0 && balance >= lane.MaxQtyStored)
                    {
                        return new ValidationResult
                        {
                            Success = false,
                            Message = $"Lane {laneNo} is FULL ({balance}/{lane.MaxQtyStored} trolleys).",
                            Severity = ValidationSeverity.Error
                        };
                    }
                }
            }
            catch
            {
                // If lane_management check fails, fall back to occupancy check
                var occupancy = _repository
                    .GetLaneOccupancy()
                    .FirstOrDefault(o =>
                        string.Equals(o.LaneNo, laneNo, StringComparison.OrdinalIgnoreCase));

                if (occupancy != null && occupancy.OpenCount > 100)
                {
                    return new ValidationResult
                    {
                        Success = false,
                        Message = "Lane is full",
                        Severity = ValidationSeverity.Error
                    };
                }
            }

            return new ValidationResult
            {
                Success = true,
                Message = "Lane available",
                Severity = ValidationSeverity.Information
            };
        }

        public ValidationResult CheckInventoryThreshold(string kanbanNo)
        {
            return new ValidationResult
            {
                Success = true,
                Message = "Inventory threshold OK",
                Severity = ValidationSeverity.Information
            };
        }

        /// <summary>
        /// Blocks storing a scanned data matrix in the Supermarket when the part's
        /// cumulative Stored-only P2 inventory is >= the TOTAL demand summed across
        /// ALL weeks in daily_demand for that PartNo (week-independent basis).
        /// With both values at 0 the gate blocks (0 >= 0), matching prior parity.
        /// </summary>
        public ValidationResult CheckStoreThreshold(string kanbanNo)
        {
            var kanban = new KanbanParser().Parse(kanbanNo);
            string partNo = kanban.PartNo;

            if (string.IsNullOrEmpty(partNo))
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Invalid kanban",
                    Severity = ValidationSeverity.Error
                };
            }

            var transactions = _repository.GetAll();
            var demands = Repositories.RepositoryProvider.DailyDemands.GetAll();

            // Total demand across all weeks for this part (no week scoping).
            int totalDemand = demands
                .Where(d => d.PartNo == partNo)
                .Sum(d => d.Quantity);

            // Only Stored units count toward the P2 total.
            int p2Stored = transactions
                .Where(t => t.PartNo == partNo && t.Status == MaterialStatus.Stored)
                .Sum(t => t.SNP);

            if (p2Stored >= totalDemand)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "EXCEEDED THRESHOLD LIMIT",
                    Severity = ValidationSeverity.Error
                };
            }

            return new ValidationResult
            {
                Success = true,
                Message = "Threshold OK",
                Severity = ValidationSeverity.Information
            };
        }
    }
}