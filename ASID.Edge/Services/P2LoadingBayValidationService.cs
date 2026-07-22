using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Repositories.Memory;
using ASID.Edge.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ASID.Edge.Services
{
    public class P2LoadingBayValidationService
    {
		private readonly ITransactionRepository _repository = RepositoryProvider.Transactions;
        public ValidationResult Validate(string dataMatrix)
        {
            // var transaction = _repository
                // .GetAll()
                // .FirstOrDefault(x => x.DataMatrix == dataMatrix);
			var transaction = _repository.GetByDataMatrix(dataMatrix);														  

            if (transaction == null)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Data Matrix not found."
                };
            }

            if (transaction.Status != MaterialStatus.Withdrawn)
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = $"Invalid material status: {transaction.Status}."
                };
            }

            return new ValidationResult
            {
                Success = true,
                Message = "Material found.",
                Transaction = transaction
            };
        }

    }


    }
