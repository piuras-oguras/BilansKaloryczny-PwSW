using BilansKaloryczny.Enums;

namespace BilansKaloryczny.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public int Age { get; set; }
    public int HeightCm { get; set; }
    public double WeightKg { get; set; }

    public Gender Gender { get; set; }
    public ActivityLevel ActivityLevel { get; set; }

    public int DailyCaloriesGoal { get; set; }

    public double ProteinPercentGoal { get; set; }
    public double FatPercentGoal { get; set; }
    public double CarbsPercentGoal { get; set; }

    public User() { }

    public User(
        int id,
        string firstName,
        int age,
        int heightCm,
        double weightKg,
        Gender gender,
        ActivityLevel activityLevel,
        int dailyCaloriesGoal,
        double proteinPercentGoal,
        double fatPercentGoal,
        double carbsPercentGoal)
    {
        Id = id;
        FirstName = firstName;
        Age = age;
        HeightCm = heightCm;
        WeightKg = weightKg;
        Gender = gender;
        ActivityLevel = activityLevel;
        DailyCaloriesGoal = dailyCaloriesGoal;
        ProteinPercentGoal = proteinPercentGoal;
        FatPercentGoal = fatPercentGoal;
        CarbsPercentGoal = carbsPercentGoal;
    }
}