using System;
using System.Collections.Generic;
using System.Linq;

namespace BilansKaloryczny.Models;

public class DailyBalance
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    public User? User { get; set; }

    public List<Meal> Meals { get; set; }
    public List<PhysicalActivity> Activities { get; set; }

    public int CaloriesConsumed => Meals?.Sum(m => m.TotalCalories) ?? 0;
    public int CaloriesBurned => Activities?.Sum(a => a.BurnedCalories) ?? 0;
    public int NetBalance => CaloriesConsumed - CaloriesBurned;

    // Makroskładniki (suma z posiłków dla danego dnia)
    public double TotalProtein => Meals?.Sum(m => m.TotalProtein) ?? 0;
    public double TotalFat => Meals?.Sum(m => m.TotalFat) ?? 0;
    public double TotalCarbs => Meals?.Sum(m => m.TotalCarbs) ?? 0;

    public DailyBalance()
    {
        Date = DateTime.Today;
        Meals = new List<Meal>();
        Activities = new List<PhysicalActivity>();
    }

    public DailyBalance(int id, DateTime date, User user, List<Meal>? meals, List<PhysicalActivity>? activities)
    {
        Id = id;
        Date = date;
        User = user;
        Meals = meals ?? new List<Meal>();
        Activities = activities ?? new List<PhysicalActivity>();

        foreach (var meal in Meals) meal.DailyBalance = this;
        foreach (var act in Activities) act.DailyBalance = this;
    }
}