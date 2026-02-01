using System;
using BilansKaloryczny.Enums;

namespace BilansKaloryczny.Models;

public class Meal
{
    public int Id { get; set; }

    public DailyBalance? DailyBalance { get; set; }

    public string Name { get; set; } = string.Empty;
    public MealCategory Category { get; set; }
    public DateTime DateTime { get; set; } = System.DateTime.Now;

    // Ułatwienie do UI: osobno data i czas (DatePicker + HH:mm)
    public DateTime Date
    {
        get => DateTime.Date;
        set => DateTime = new DateTime(value.Year, value.Month, value.Day, DateTime.Hour, DateTime.Minute, 0);
    }

    public string Time
    {
        get => DateTime.ToString("HH:mm");
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (TimeSpan.TryParse(value, out var ts))
                DateTime = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, ts.Hours, ts.Minutes, 0);
        }
    }

    public int TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalFat { get; set; }
    public double TotalCarbs { get; set; }

    public Meal() { }

    public Meal(
        int id,
        DailyBalance dailyBalance,
        string name,
        MealCategory category,
        DateTime dateTime,
        int totalCalories,
        double totalProtein,
        double totalFat,
        double totalCarbs)
    {
        Id = id;
        DailyBalance = dailyBalance;
        Name = name;
        Category = category;
        DateTime = dateTime;
        TotalCalories = totalCalories;
        TotalProtein = totalProtein;
        TotalFat = totalFat;
        TotalCarbs = totalCarbs;
    }
}