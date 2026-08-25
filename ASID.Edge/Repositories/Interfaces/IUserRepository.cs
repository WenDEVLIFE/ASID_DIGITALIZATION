using ASID.Edge.Models;
using System.Collections.Generic;

namespace ASID.Edge.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? GetByUsername(string username);

        IReadOnlyList<User> GetAll();
    }
}
