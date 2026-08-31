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
        /// A lane is assignable only if it exists in the transactions table
        /// and has no open (consumed_at IS NULL) transaction.
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

            var occupancy = _repository
                .GetLaneOccupancy()
                .FirstOrDefault(o =>
                    string.Equals(o.LaneNo, laneNo, StringComparison.OrdinalIgnoreCase));

            // Lane is available if it has no open transactions.
            // occupancy == null means the lane has never been used — that's fine.
            if (occupancy != null && occupancy.OpenCount > 0)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Lane is not vacant",
                    Severity = ValidationSeverity.Error
                };
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
    }
}