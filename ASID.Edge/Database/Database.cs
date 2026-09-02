using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using Microsoft.Data.SqlClient;

namespace ASID.Edge.Database;

public static class Database
{
    private static readonly IReadOnlyDictionary<string, string> EnvValues;

    static Database()
    {
        EnvValues = LoadEnvFile();
    }

    /// <summary>
    /// Returns the configured database engine: "postgresql" or "mssql".
    /// </summary>
    public static string Engine =>
        GetOptionalValue(EnvValues, "ASID_DB_ENGINE", "postgresql");

    /// <summary>
    /// Creates a PostgreSQL connection (legacy).
    /// </summary>
    public static NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(BuildPostgresConnectionString(EnvValues));
    }

    /// <summary>
    /// Creates an MSSQL connection.
    /// </summary>
    public static SqlConnection CreateMssqlConnection()
    {
        return new SqlConnection(BuildMssqlConnectionString(EnvValues));
    }

    /// <summary>
    /// Creates a connection based on the configured engine.
    /// Returns IDbConnection-compatible object — callers should cast.
    /// </summary>
    public static object CreateSmartConnection()
    {
        if (Engine.Equals("mssql", StringComparison.OrdinalIgnoreCase))
            return CreateMssqlConnection();
        return CreateConnection();
    }

    private static IReadOnlyDictionary<string, string> LoadEnvFile()
    {
        string envPath = FindEnvFile();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string rawLine in File.ReadAllLines(envPath))
        {
            string line = rawLine.Trim();

            // Skip blank lines and comments.
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            // Split on the first '=' only, so values may contain '='.
            int equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue; // No key (or missing '='): not a valid entry.
            }

            string key = line.Substring(0, equalsIndex).Trim();
            string value = line.Substring(equalsIndex + 1).Trim();
            values[key] = value;
        }

        return values;
    }

    private static string FindEnvFile()
    {
        // Desktop app: the executable lives under ASID.Edge\bin\..., so walk
        // upward from AppContext.BaseDirectory until a .env is found.
        string? envFile = FindEnvFileUpward(AppContext.BaseDirectory);
        if (envFile != null)
        {
            return envFile;
        }

        // Fall back to the current working directory.
        envFile = FindEnvFileUpward(Directory.GetCurrentDirectory());
        if (envFile != null)
        {
            return envFile;
        }

        throw new FileNotFoundException(
            "The .env file could not be found. Copy '.env.example' to '.env' at the repository root " +
            "and fill in the database values before starting the application.");
    }

    private static string? FindEnvFileUpward(string startDirectory)
    {
        DirectoryInfo? directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            string candidatePath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            directory = directory.Parent; // Becomes null at the filesystem root.
        }

        return null;
    }

    private static string BuildPostgresConnectionString(IReadOnlyDictionary<string, string> values)
    {
        // Choose the active variable prefix based on the ASID_USE_LOCAL toggle.
        //   ASID_USE_LOCAL=true  -> ASID_LOCAL_DB_* (localhost)
        //   ASID_USE_LOCAL=false -> ASID_DB_*       (cloud / Neon)
        bool useLocal = GetOptionalValue(values, "ASID_USE_LOCAL", "true")
            .Equals("true", StringComparison.OrdinalIgnoreCase);

        string prefix = useLocal ? "ASID_LOCAL_DB_" : "ASID_DB_";

        string host = GetRequiredValue(values, prefix + "HOST");
        string port = GetRequiredValue(values, prefix + "PORT");
        string database = GetRequiredValue(values, prefix + "NAME");
        string username = GetRequiredValue(values, prefix + "USER");
        string password = GetRequiredValue(values, prefix + "PASSWORD");

        // Local connections use plain password auth over TCP; the SSL/Channel
        // Binding options are only meaningful for the Neon (cloud) endpoint.
        string sslOptions = useLocal ? "" : "SSL Mode=Require;Channel Binding=Require;";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};" +
               sslOptions;
    }

    private static string BuildMssqlConnectionString(IReadOnlyDictionary<string, string> values)
    {
        string server = GetRequiredValue(values, "ASID_MSSQL_SERVER");
        string database = GetRequiredValue(values, "ASID_MSSQL_DB");
        string username = GetRequiredValue(values, "ASID_MSSQL_USER");
        string password = GetRequiredValue(values, "ASID_MSSQL_PASSWORD");

        return $"Server={server};Database={database};User Id={username};Password={password};" +
               $"TrustServerCertificate=True;Encrypt=Mandatory;Connection Timeout=5;";
    }

    private static string GetRequiredValue(
        IReadOnlyDictionary<string, string> values,
        string envKey)
    {
        if (!values.TryGetValue(envKey, out string? value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing or empty '{envKey}' in the .env file. Copy '.env.example' to '.env' " +
                $"and fill in the database values.");
        }

        return value;
    }

    private static string GetOptionalValue(
        IReadOnlyDictionary<string, string> values,
        string envKey,
        string defaultValue)
    {
        if (values.TryGetValue(envKey, out string? value) && !string.IsNullOrWhiteSpace(value))
            return value;
        return defaultValue;
    }
}
