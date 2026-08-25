using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.PostgreSql
{
    public class PostgreSqlUserRepository
        : IUserRepository
    {
        public User? GetByUsername(string username)
        {
            using var connection =
                Database.Database.CreateConnection();

            connection.Open();

            const string sql = @"
SELECT
    id AS Id,
    username AS Username,
    password_hash AS PasswordHash,
    role AS Role,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt
FROM users
WHERE username = @username
LIMIT 1;";

            return connection.QueryFirstOrDefault<User>(
                sql,
                new { username });
        }

        public IReadOnlyList<User> GetAll()
        {
            using var connection =
                Database.Database.CreateConnection();

            connection.Open();

            const string sql = @"
SELECT
    id AS Id,
    username AS Username,
    password_hash AS PasswordHash,
    role AS Role,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt
FROM users
ORDER BY username;";

            return connection.Query<User>(sql).ToList();
        }
    }
}
