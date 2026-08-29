using System;

namespace ASID.Edge.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        // Stored as TEXT in the database; parsed to the Role enum at the
        // AuthService boundary (Dapper string->enum mapping is unreliable).
        public string Role { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
