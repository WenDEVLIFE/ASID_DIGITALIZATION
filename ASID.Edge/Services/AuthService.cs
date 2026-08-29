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
            CurrentRole is Role.QA or Role.Supervisor;

        public bool CanImportDemand =>
            CurrentRole is Role.Supervisor or Role.Planner;

        public bool CanOverride =>
            CurrentRole is Role.Supervisor;

        public bool Login(string username, string password)
        {
            string normalized = username.Trim().ToLowerInvariant();

            User? user;
            try
            {
                user = _users.GetByUsername(normalized);
            }
            catch
            {
                // Server unreachable — cannot authenticate
                return false;
            }

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

            User? user;
            try
            {
                user = _users.GetByUsername(username);
            }
            catch
            {
                // Server unreachable — use cached session data
                if (role != null)
                {
                    CurrentUser = new User { Username = username, Role = role };
                    CurrentRole = ToRole(role);
                    return true;
                }
                return false;
            }

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
                "planner" => Role.Planner,
                _ => Role.Operator
            };
        }
    }
}
