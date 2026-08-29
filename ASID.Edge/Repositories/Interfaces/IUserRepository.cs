using ASID.Edge.Models;
using System.Collections.Generic;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? GetByUsername(string username);

        IReadOnlyList<User> GetAll();

        void Add(User user);

        void Update(User user);

        void Delete(Guid userId);
    }
}
