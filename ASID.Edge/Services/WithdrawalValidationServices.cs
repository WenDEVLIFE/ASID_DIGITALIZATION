using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ASID.Edge.Services
{
    public class WithdrawalValidationService
    {
        private readonly ITransactionRepository _repository = RepositoryProvider.Transactions;

        /// <summary>
        /// Validate a withdrawal scan.
        /// Checks:
        ///   1. Data matrix exists in the system
        ///   2. Material status is Stored
        ///   3. FIFO: no older stored items for the same part number
        /// </summary>
        public ValidationResult Validate(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Data Matrix not found."
                };
            }

            if (transaction.Status != MaterialStatus.Stored)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = $"Invalid material status: {transaction.Status}."
                };
            }

            // ── FIFO CHECK ──
            // Find all stored items for the same part number.
            var allStored = _repository.GetAll()
                .Where(t => t.PartNo == transaction.PartNo
                         && t.Status == MaterialStatus.Stored)
                .OrderBy(t => t.CreatedAt)
                .ToList();

            // The item being scanned should be the oldest (first in FIFO).
            var oldest = allStored.FirstOrDefault();

            if (oldest != null && oldest.DataMatrix != transaction.DataMatrix)
            {
                // There is an older item — block withdrawal.
                string olderDate = oldest.CreatedAt.ToString("MM/dd/yyyy HH:mm");
                string olderSerial = oldest.SerialNo;

                return new ValidationResult
                {
                    Success = false,
                    Severity = ValidationSeverity.Warning,
                    Message = $"FIFO Violation!\n\n"
                            + $"This item was stored on {transaction.CreatedAt:MM/dd/yyyy HH:mm}.\n\n"
                            + $"An older item exists:\n"
                            + $"Serial: {olderSerial}\n"
                            + $"Stored: {olderDate}\n\n"
                            + $"Look for the earlier date."
                };
            }

            return new ValidationResult
            {
                Success = true,
                Message = "Material found. FIFO check passed.",
                Transaction = transaction
            };
        }

    }
}
