using System;
using System.Collections.Generic;
using System.Linq;

namespace BilansKaloryczny.Models;

public class WeeklyReport
{
    public int Id { get; set; }

    public User? User { get; set; }

    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public List<DailyBalance> Days { get; set; }

    public double AvgCaloriesConsumed => Days.Count == 0 ? 0 : Days.Average(d => d.CaloriesConsumed);
    public double AvgCaloriesBurned => Days.Count == 0 ? 0 : Days.Average(d => d.CaloriesBurned);
    public double AvgNetBalance => Days.Count == 0 ? 0 : Days.Average(d => d.NetBalance);

    public double ProteinSharePercent
    {
        get
        {
            var (p, f, c) = GetMacroTotals();
            var total = p + f + c;
            return total <= 0 ? 0 : (p / total) * 100.0;
        }
    }

    public double FatSharePercent
    {
        get
        {
            var (p, f, c) = GetMacroTotals();
            var total = p + f + c;
            return total <= 0 ? 0 : (f / total) * 100.0;
        }
    }

    public double CarbsSharePercent
    {
        get
        {
            var (p, f, c) = GetMacroTotals();
            var total = p + f + c;
            return total <= 0 ? 0 : (c / total) * 100.0;
        }
    }

    public WeeklyReport()
    {
        Days = new List<DailyBalance>();
        DateFrom = DateTime.Today;
        DateTo = DateTime.Today;
    }

    public WeeklyReport(int id, User user, DateTime dateFrom, DateTime dateTo, List<DailyBalance>? days)
    {
        Id = id;
        User = user;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Days = days ?? new List<DailyBalance>();
    }

    private (double protein, double fat, double carbs) GetMacroTotals()
    {
        if (Days.Count == 0)
            return (0, 0, 0);

        var meals = Days.SelectMany(d => d.Meals ?? new List<Meal>());

        var protein = meals.Sum(m => m.TotalProtein);
        var fat = meals.Sum(m => m.TotalFat);
        var carbs = meals.Sum(m => m.TotalCarbs);

        return (protein, fat, carbs);
    }
}
