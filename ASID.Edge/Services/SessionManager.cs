using System;
using System.IO;
using System.Text.Json;

namespace ASID.Edge.Services
{
    /// <summary>
    /// Persists the user session to a local JSON file so the user does not
    /// need to log in every time the application starts.
    /// 
    /// Session is stored under %LOCALAPPDATA%\ASID\session.json and expires
    /// after <see cref="SessionExpiryHours"/> hours (default 24 h).
    /// </summary>
    public static class SessionManager
    {
        private static readonly string SessionDir =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ASID");

        private static readonly string SessionFile =
            Path.Combine(SessionDir, "session.json");

        /// <summary>How many hours a session stays valid.</summary>
        public const int SessionExpiryHours = 24;

        /// <summary>Payload saved to disk.</summary>
        private class SessionData
        {
            public string Username { get; set; } = "";
            public string Role { get; set; } = "";
            public DateTime LoginTime { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        /// <summary>
        /// Save a session for <paramref name="username"/> with the given role.
        /// Called after a successful login.
        /// </summary>
        public static void Save(string username, string role)
        {
            try
            {
                Directory.CreateDirectory(SessionDir);

                var data = new SessionData
                {
                    Username = username,
                    Role = role,
                    LoginTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(SessionExpiryHours)
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SessionFile, json);
            }
            catch
            {
                // Non-critical: if we can't save the session the user just
                // logs in again next time.
            }
        }

        /// <summary>
        /// Try to restore a previous session.
        /// Returns (username, role) if a valid, non-expired session exists;
        /// otherwise returns (null, null).
        /// </summary>
        public static (string? Username, string? Role) Restore()
        {
            try
            {
                if (!File.Exists(SessionFile))
                    return (null, null);

                string json = File.ReadAllText(SessionFile);
                var data = JsonSerializer.Deserialize<SessionData>(json);

                if (data == null)
                    return (null, null);

                if (string.IsNullOrWhiteSpace(data.Username))
                    return (null, null);

                if (DateTime.UtcNow > data.ExpiresAt)
                {
                    Clear();
                    return (null, null);
                }

                return (data.Username, data.Role);
            }
            catch
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Delete the saved session file. Called on logout.
        /// </summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(SessionFile))
                    File.Delete(SessionFile);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }
}
