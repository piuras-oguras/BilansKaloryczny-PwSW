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