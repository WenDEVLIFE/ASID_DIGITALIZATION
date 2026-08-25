using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using System;

namespace ASID.Edge.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;

        public AuthService(IUserRepository users)
        {
            _users = users;
        }

        public User? CurrentUser { get; private set; }

        public Role CurrentRole { get; private set; } = Role.Operator;

        public bool IsAuthenticated => CurrentUser != null;

        public bool CanFlagNC =>
            CurrentRole is Role.QA or Role.Supervisor;

        public bool CanReviewNC =>
            CurrentRole == Role.Supervisor;

        public bool CanImportDemand =>
            CurrentRole == Role.Supervisor;

        public bool Login(string username, string password)
        {
            string normalized = username.Trim().ToLowerInvariant();

            User? user = _users.GetByUsername(normalized);
            if (user == null)
            {
                return false;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                return false;
            }

            CurrentUser = user;
            CurrentRole = ToRole(user.Role);

            // Persist session so the user skips login next time.
            SessionManager.Save(normalized, user.Role);

            return true;
        }

        /// <summary>
        /// Restore a previously saved session (username + role).
        /// Returns true if a valid session was restored.
        /// </summary>
        public bool RestoreSession()
        {
            var (username, role) = SessionManager.Restore();

            if (string.IsNullOrEmpty(username))
                return false;

            User? user = _users.GetByUsername(username);
            if (user == null)
                return false;

            CurrentUser = user;
            CurrentRole = ToRole(role ?? user.Role);

            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentRole = Role.Operator;

            // Clear persisted session.
            SessionManager.Clear();
        }

        public static Role ToRole(string role)
        {
            return role.Trim().ToLowerInvariant() switch
            {
                "qa" => Role.QA,
                "supervisor" => Role.Supervisor,
                _ => Role.Operator
            };
        }
    }
}
