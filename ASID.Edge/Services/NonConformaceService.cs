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

            _repository.Update(transaction);
        }
    }
}


