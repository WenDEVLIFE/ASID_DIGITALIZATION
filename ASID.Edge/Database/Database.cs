using Npgsql;

namespace ASID.Edge.Database;

public static class Database
{
    private static readonly string ConnectionString =
    "Host=ep-shiny-shadow-ao74vw7s-pooler.c-2.ap-southeast-1.aws.neon.tech;" +
    "Port=5432;" +
    "Database=asid_db;" +
    "Username=neondb_owner;" +      
    "Password=npg_fGO3HP7ISeoK;" +
    "SSL Mode=Require;" +
    "Channel Binding=Require;";

    public static NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}