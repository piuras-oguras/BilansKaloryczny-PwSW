using System;
using System.Collections.ObjectModel;
using System.Linq;
using BilansKaloryczny.Enums;
using BilansKaloryczny.Models;

namespace BilansKaloryczny.ViewModels;

public class MainViewModel : BaseViewModel
{
    public User CurrentUser { get; set; }

    private DailyBalance _selectedDay = new();
    public DailyBalance SelectedDay
    {
        get => _selectedDay;
        set
        {
            _selectedDay = value;
            OnPropertyChanged();
            RefreshComputed();
        }
    }

    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<PhysicalActivity> Activities { get; } = new();

    public int CaloriesConsumed => Meals.Sum(m => m.TotalCalories);
    public int CaloriesBurned => Activities.Sum(a => a.BurnedCalories);
    public int NetBalance => CaloriesConsumed - CaloriesBurned;

    public RelayCommand AddMealCommand { get; }
    public RelayCommand AddActivityCommand { get; }

    public MainViewModel()
    {
        CurrentUser = new User(
            id: 1,
            firstName: "Szymon",
            age: 22,
            heightCm: 180,
            weightKg: 80,
            gender: Gender.Male,
            activityLevel: ActivityLevel.Moderate,
            dailyCaloriesGoal: 2500,
            proteinPercentGoal: 25,
            fatPercentGoal: 30,
            carbsPercentGoal: 45
        );

        SelectedDay = new DailyBalance
        {
            Id = 1,
            Date = DateTime.Today,
            User = CurrentUser
        };

        AddMealCommand = new RelayCommand(AddMeal);
        AddActivityCommand = new RelayCommand(AddActivity);

        SeedExampleData();
    }

    private void SeedExampleData()
    {
        Meals.Add(new Meal
        {
            Id = 1,
            DailyBalance = SelectedDay,
            Name = "Owsianka",
            Category = MealCategory.Breakfast,
            DateTime = DateTime.Now,
            TotalCalories = 450,
            TotalProtein = 20,
            TotalFat = 12,
            TotalCarbs = 60
        });

        Activities.Add(new PhysicalActivity
        {
            Id = 1,
            DailyBalance = SelectedDay,
            Name = "Spacer",
            DurationMin = 30,
            BurnedCalories = 120,
            Intensity = ActivityIntensity.Low
        });

        RefreshComputed();
    }

    private void AddMeal()
    {
        var nextId = Meals.Count == 0 ? 1 : Meals.Max(m => m.Id) + 1;

        Meals.Add(new Meal
        {
            Id = nextId,
            DailyBalance = SelectedDay,
            Name = "Nowy posi³ek",
            Category = MealCategory.Snack,
            DateTime = DateTime.Now,
            TotalCalories = 300,
            TotalProtein = 10,
            TotalFat = 10,
            TotalCarbs = 40
        });

        RefreshComputed();
    }

    private void AddActivity()
    {
        var nextId = Activities.Count == 0 ? 1 : Activities.Max(a => a.Id) + 1;

        Activities.Add(new PhysicalActivity
        {
            Id = nextId,
            DailyBalance = SelectedDay,
            Name = "Nowa aktywnoœæ",
            DurationMin = 20,
            BurnedCalories = 150,
            Intensity = ActivityIntensity.Medium
        });

        RefreshComputed();
    }

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(CaloriesConsumed));
        OnPropertyChanged(nameof(CaloriesBurned));
        OnPropertyChanged(nameof(NetBalance));
    }
}
