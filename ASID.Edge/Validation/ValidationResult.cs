using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Validation
{
    public class ValidationResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public StorageTransaction? Transaction { get; set; }

        public ValidationSeverity Severity { get; set; }
            = ValidationSeverity.Information;
    }

    public enum ValidationSeverity
    {
        Information,

        Warning,

        Error
    }
}
