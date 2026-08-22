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

        public void FlagAsSuspected(string dataMatrix)
        {
            var transaction =
    _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
            {
                throw new Exception(
                    "Material not found.");
            }

            if (transaction.IsSuspectedNC)
            {
                throw new Exception(
                    "Material is already flagged as Suspected NC.");
            }

            transaction.IsSuspectedNC = true;
            transaction.IsNCConfirmed = false;
            transaction.IsNCRejected = false;

            _repository.Update(transaction);
        }

        public void ConfirmNC(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
            {
                throw new Exception("Material not found.");
            }

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = true;
            transaction.IsNCRejected = false;

            _repository.Update(transaction);
        }

        public void RejectNC(string dataMatrix)
        {
            var transaction = _repository.GetByDataMatrix(dataMatrix);

            if (transaction == null)
            {
                throw new Exception("Material not found.");
            }

            transaction.IsSuspectedNC = false;
            transaction.IsNCConfirmed = false;
            transaction.IsNCRejected = true;

            _repository.Update(transaction);
        }
    }
}


