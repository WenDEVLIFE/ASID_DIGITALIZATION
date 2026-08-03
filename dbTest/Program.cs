using System;
using System.Collections.Generic;
//using ASID.Edge.Models;
//using ASID.Edge.Repositories.Interfaces;
//using ASID.Edge.Repositories.PostgreSql;

class Program
{
    static void Main()
    {
        //try
        //{
        //    Console.WriteLine("Testing DailyDemand Repository...");

        //    IDailyDemandRepository repository =
        //        new PostgreSqlDailyDemandRepository();

        //    repository.DeleteAll();

        //    repository.Insert(new List<DailyDemand>
        //    {
        //        new()
        //        {
        //            ProductionDate = DateTime.Today,
        //            Shift = 1,
        //            Model = "HG180 NH",
        //            PartNo = "656765900A",
        //            Quantity = 81
        //        },
        //        new()
        //        {
        //            ProductionDate = DateTime.Today,
        //            Shift = 2,
        //            Model = "HG180 NH",
        //            PartNo = "656765900A",
        //            Quantity = 90
        //        }
        //    });

        //    var result = repository.GetByDate(DateTime.Today);

        //    Console.WriteLine($"Retrieved {result.Count} record(s).");

        //    foreach (var item in result)
        //    {
        //        Console.WriteLine(
        //            $"{item.ProductionDate:yyyy-MM-dd} | " +
        //            $"Shift {item.Shift} | " +
        //            $"{item.PartNo} | " +
        //            $"{item.Quantity}");
        //    }

        //    Console.WriteLine("Repository test completed successfully.");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex);
        //}

        Console.ReadKey();
    }
}