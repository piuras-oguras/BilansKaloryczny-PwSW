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

    public double ProteinGramsGoal { get; set; }
    public double FatGramsGoal { get; set; }
    public double CarbsGramsGoal { get; set; }

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
        double proteinGramsGoal,
        double fatGramsGoal,
        double carbsGramsGoal)
    {
        Id = id;
        FirstName = firstName;
        Age = age;
        HeightCm = heightCm;
        WeightKg = weightKg;
        Gender = gender;
        ActivityLevel = activityLevel;
        DailyCaloriesGoal = dailyCaloriesGoal;

        ProteinGramsGoal = proteinGramsGoal;
        FatGramsGoal = fatGramsGoal;
        CarbsGramsGoal = carbsGramsGoal;
    }
}