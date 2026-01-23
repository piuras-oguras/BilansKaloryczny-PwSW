using BilansKaloryczny.Enums;

namespace BilansKaloryczny.Models;

public class PhysicalActivity
{
    public int Id { get; set; }

    public DailyBalance? DailyBalance { get; set; }

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