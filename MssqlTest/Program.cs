using Microsoft.Data.SqlClient;
using Dapper;

string connStr = "Server=localhost\\SQLEXPRESS;Database=asid32_db;User Id=eondb_dbo;Password=xuVLa!P4BT5A!Lgwf91b;TrustServerCertificate=True;Encrypt=Mandatory;Connection Timeout=5;";

Console.WriteLine("Testing daily_demand query...");

try
{
    using var conn = new SqlConnection(connStr);
    conn.Open();

    // Test 1: Count
    int count = conn.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM daily_demand");
    Console.WriteLine($"Total rows: {count}");

    // Test 2: Full query like the app uses
    var rows = conn.Query(@"
SELECT
    id AS Id,
    production_date AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity,
    scrapped AS Scrapped,
    imported_at AS ImportedAt
FROM daily_demand
ORDER BY production_date, part_no, shift;").ToList();

    Console.WriteLine($"Query returned: {rows.Count} rows");

    if (rows.Count > 0)
    {
        var first = rows[0];
        var dict = (IDictionary<string, object?>)first;
        Console.WriteLine($"  First row: Model={dict["Model"]}, PartNo={dict["PartNo"]}, Qty={dict["Quantity"]}, Date={dict["ProductionDate"]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
