using ASID.Edge.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Services
{
    public class StorageValidationService
    {
        public ValidationResult CheckInventoryThreshold(string kanbanNo)
        {
            return new ValidationResult
            {
                Success = true,
                Message = "Inventory threshold OK",
                Severity = ValidationSeverity.Information
            };
        }

        public ValidationResult CheckAssignedLane(string laneNo)
        {
            return new ValidationResult
            {
                Success = true,
                Message = "Lane available",
                Severity = ValidationSeverity.Information
            };
        }
    }
}
