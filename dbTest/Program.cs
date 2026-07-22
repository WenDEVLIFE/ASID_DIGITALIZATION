using System;
using Npgsql;

class Program
{
    static void Main()
    {
        var conn =
    new NpgsqlConnection(
    "Host=ep-shiny-shadow-ao74vw7s-pooler.c-2.ap-southeast-1.aws.neon.tech;" +
    "Port=5432;" +
    "Database=asid_db;" +
    "Username=neondb_owner;" +
    "Password=npg_fGO3HP7ISeoK;" +
    "SSL Mode=Require;" +
    "Channel Binding=Require;");

        conn.Open();

        Console.WriteLine("Connected");

        //var connString = (
        //            "Host=ep-shiny-shadow-ao74vw7s-pooler.c-2.ap-southeast-1.aws.neon.tech;" +
        //            "Port=5432;" +
        //            "Database=asid;" +
        //            "Username=neondb_owner;" +
        //            "Password=npg_fGO3HP7ISeoK;" +
        //            "SSL Mode=Require;");
        ////"Host=db.pnzzbzqzeliswvyfgyfr.supabase.co;" +
        ////"Port=5432;" +
        ////"Database=postgres;" +
        ////"Username=postgres;" +
        ////"Password=Viczel_ASID2026;" +
        ////"SSL Mode=Require;" +
        ////"Trust Server Certificate=true;";

        //try
        //{
        //    using var conn = new NpgsqlConnection(connString);

        //    conn.Open();

        //    Console.WriteLine("Connected Successfully!");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.ToString());
        //}

        //Console.ReadKey();
    }
}