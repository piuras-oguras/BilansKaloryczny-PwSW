using System;
using BilansKaloryczny.Enums;

namespace BilansKaloryczny.Models;

public class PhysicalActivity
{
    public int Id { get; set; }

    public DailyBalance? DailyBalance { get; set; }

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

    public string Name { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public int BurnedCalories { get; set; }
    public ActivityIntensity Intensity { get; set; }

    public PhysicalActivity() { }

    public PhysicalActivity(
        int id,
        DailyBalance dailyBalance,
        string name,
        int durationMin,
        int burnedCalories,
        ActivityIntensity intensity)
    {
        Id = id;
        DailyBalance = dailyBalance;
        Name = name;
        DurationMin = durationMin;
        BurnedCalories = burnedCalories;
        Intensity = intensity;
    }
}