using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.SQLite
{
    /// <summary>
    /// Local SQLite user repository — ensures login always works
    /// even when the PostgreSQL server is unreachable.
    /// </summary>
    public class SqliteUserRepository : IUserRepository
    {
        public User? GetByUsername(string username)
        {
            using var connection = SqliteDatabase.CreateConnection();
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

            return connection.QueryFirstOrDefault<User>(sql, new { username });
        }

        public IReadOnlyList<User> GetAll()
        {
            using var connection = SqliteDatabase.CreateConnection();
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

        public void Add(User user)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            const string sql = @"
INSERT INTO users (id, username, password_hash, role, created_at, updated_at)
VALUES (@Id, @Username, @PasswordHash, @Role, @CreatedAt, @UpdatedAt);";

            connection.Execute(sql, new
            {
                user.Id,
                user.Username,
                user.PasswordHash,
                user.Role,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        public void Update(User user)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            const string sql = @"
UPDATE users
SET username = @Username,
    password_hash = @PasswordHash,
    role = @Role,
    updated_at = @UpdatedAt
WHERE id = @Id;";

            connection.Execute(sql, new
            {
                user.Id,
                user.Username,
                user.PasswordHash,
                user.Role,
                user.UpdatedAt
            });
        }

        public void Delete(Guid userId)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            const string sql = @"
DELETE FROM users
WHERE id = @Id;";

            connection.Execute(sql, new { Id = userId });
        }
    }
}
