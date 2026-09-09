using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;

namespace ASID.Edge.Services
{
    public class NonConformanceService
    {
        private readonly ITransactionRepository _repository;

        public NonConformanceService(
            ITransactionRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Flag a material as Suspected NC.
        /// Shows warning symbol in transaction grid.
        /// </summary>
        public void FlagAsSuspected(string dataMatrix, int ncQuantity)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            if (transaction.IsSuspectedNC)
                throw new Exception("Material is already flagged as Suspected NC.");

            if (transaction.Status == MaterialStatus.Scrapped)
                throw new Exception("Material is already scrapped.");

            transaction.IsSuspectedNC = true;
            transaction.IsNCConfirmed = false;
            transaction.IsNCRejected = false;
            transaction.NCQuantity = ncQuantity > 0 ? ncQuantity : 1;

            _repository.Update(transaction);
        }

        /// <summary>
        /// Legacy: register NC item directly (confirms NC immediately).
        /// Kept for backward compatibility.
        /// </summary>
        public void RegisterNCItem(string dataMatrix, int ncQuantity)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = true;
            transaction.IsNCRejected = false;
            transaction.NCQuantity = ncQuantity > 0 ? ncQuantity : 1;

            _repository.Update(transaction);
        }

        /// <summary>
        /// Unflag: QA reviewed and found OK.
        /// Removes the warning symbol from the transaction.
        /// </summary>
        public void Unflag(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            if (!transaction.IsSuspectedNC)
                throw new Exception("Material is not flagged as Suspected NC.");

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = false;
            transaction.IsNCRejected = true;
            transaction.NCQuantity = 0;

            _repository.Update(transaction);
        }

        /// <summary>
        /// Scrap: QA reviewed and found NG.
        /// Changes status to Scrapped, sets scrapped quantity.
        /// This quantity is deducted from inventory in the dashboard.
        /// </summary>
        public void Scrap(string dataMatrix, int scrapQuantity)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            if (!transaction.IsSuspectedNC)
                throw new Exception("Material is not flagged as Suspected NC.");

            int qty = scrapQuantity > 0 ? scrapQuantity : transaction.NCQuantity;
            if (qty <= 0)
                qty = transaction.SNP; // scrap entire quantity if not specified

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = true;
            transaction.IsNCRejected = false;
            transaction.NCQuantity = qty;
            transaction.Status = MaterialStatus.Scrapped;

            _repository.Update(transaction);
        }

        /// <summary>Legacy confirm NC.</summary>
        public void ConfirmNC(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = true;
            transaction.IsNCRejected = false;

            _repository.Update(transaction);
        }

        /// <summary>Legacy reject NC.</summary>
        public void RejectNC(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
                throw new Exception("Material not found.");

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = false;
            transaction.IsNCRejected = true;

            _repository.Update(transaction);
        }
    }
}


