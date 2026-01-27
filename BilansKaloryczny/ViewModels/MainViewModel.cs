using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BilansKaloryczny.Enums;
using BilansKaloryczny.Models;

namespace BilansKaloryczny.ViewModels;

public class MainViewModel : BaseViewModel
{
    public User CurrentUser { get; set; }

    public ObservableCollection<ActivityIntensity> Intensities { get; } =
        new(Enum.GetValues(typeof(ActivityIntensity)).Cast<ActivityIntensity>());

    public ObservableCollection<MealCategory> MealCategories { get; } =
        new(Enum.GetValues(typeof(MealCategory)).Cast<MealCategory>());

    public ObservableCollection<Gender> Genders { get; } =
        new(Enum.GetValues(typeof(Gender)).Cast<Gender>());

    public ObservableCollection<ActivityLevel> ActivityLevels { get; } =
        new(Enum.GetValues(typeof(ActivityLevel)).Cast<ActivityLevel>());

    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<PhysicalActivity> Activities { get; } = new();

    public ICollectionView MealsView { get; }
    public ICollectionView ActivitiesView { get; }

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate.Date == value.Date) return;
            _selectedDate = value.Date;
            OnPropertyChanged();

            SelectedDay = new DailyBalance { Id = SelectedDay.Id, Date = _selectedDate, User = CurrentUser };

            ApplyDateFilter();
            RefreshComputed();
            RefreshStatistics();
        }
    }

    private DailyBalance _selectedDay = new();
    public DailyBalance SelectedDay
    {
        get => _selectedDay;
        set
        {
            _selectedDay = value;
            OnPropertyChanged();
        }
    }

    private Meal? _selectedMeal;
    public Meal? SelectedMeal
    {
        get => _selectedMeal;
        set { _selectedMeal = value; OnPropertyChanged(); }
    }

    private PhysicalActivity? _selectedActivity;
    public PhysicalActivity? SelectedActivity
    {
        get => _selectedActivity;
        set { _selectedActivity = value; OnPropertyChanged(); }
    }

    public int CaloriesConsumed => MealsView.Cast<Meal>().Sum(m => m.TotalCalories);
    public int CaloriesBurned => ActivitiesView.Cast<PhysicalActivity>().Sum(a => a.BurnedCalories);
    public int NetBalance => CaloriesConsumed - CaloriesBurned;
    public int CaloriesRemainingToGoal => Math.Max(0, CurrentUser.DailyCaloriesGoal - CaloriesConsumed);

    public string GoalProgressText
        => $"{CaloriesConsumed} / {CurrentUser.DailyCaloriesGoal} kcal ({(CurrentUser.DailyCaloriesGoal == 0 ? 0 : (int)Math.Round(100.0 * CaloriesConsumed / CurrentUser.DailyCaloriesGoal))}%)";

    public RelayCommand AddMealCommand { get; }
    public RelayCommand AddActivityCommand { get; }
    public RelayCommand DeleteMealCommand { get; }
    public RelayCommand DeleteActivityCommand { get; }

    public RelayCommand ApplySettingsCommand { get; }
    public RelayCommand ResetSettingsCommand { get; }

    private string _settingsFirstName = "";
    public string SettingsFirstName
    {
        get => _settingsFirstName;
        set { _settingsFirstName = value; OnPropertyChanged(); }
    }

    private int _settingsAge;
    public int SettingsAge
    {
        get => _settingsAge;
        set { _settingsAge = value; OnPropertyChanged(); }
    }

    private int _settingsHeightCm;
    public int SettingsHeightCm
    {
        get => _settingsHeightCm;
        set { _settingsHeightCm = value; OnPropertyChanged(); }
    }

    private double _settingsWeightKg;
    public double SettingsWeightKg
    {
        get => _settingsWeightKg;
        set { _settingsWeightKg = value; OnPropertyChanged(); }
    }

    private Gender _settingsGender;
    public Gender SettingsGender
    {
        get => _settingsGender;
        set { _settingsGender = value; OnPropertyChanged(); }
    }

    private ActivityLevel _settingsActivityLevel;
    public ActivityLevel SettingsActivityLevel
    {
        get => _settingsActivityLevel;
        set { _settingsActivityLevel = value; OnPropertyChanged(); }
    }

    private int _settingsDailyGoal;
    public int SettingsDailyCaloriesGoal
    {
        get => _settingsDailyGoal;
        set { _settingsDailyGoal = value; OnPropertyChanged(); }
    }

    private int _settingsProtein;
    public int SettingsProteinPercentGoal
    {
        get => _settingsProtein;
        set { _settingsProtein = value; OnPropertyChanged(); }
    }

    private int _settingsFat;
    public int SettingsFatPercentGoal
    {
        get => _settingsFat;
        set { _settingsFat = value; OnPropertyChanged(); }
    }

    private int _settingsCarbs;
    public int SettingsCarbsPercentGoal
    {
        get => _settingsCarbs;
        set { _settingsCarbs = value; OnPropertyChanged(); }
    }

    public ObservableCollection<int> StatsRanges { get; } = new() { 7, 14, 30 };

    private int _selectedStatsRange = 7;
    public int SelectedStatsRange
    {
        get => _selectedStatsRange;
        set
        {
            if (_selectedStatsRange == value) return;
            _selectedStatsRange = value;
            OnPropertyChanged();
            RefreshStatistics();
        }
    }

    public ObservableCollection<DaySummary> StatsDays { get; } = new();

    public int StatsTotalConsumed => StatsDays.Sum(d => d.Consumed);
    public int StatsTotalBurned => StatsDays.Sum(d => d.Burned);
    public int StatsTotalNet => StatsDays.Sum(d => d.Net);
    public double StatsAvgNet => StatsDays.Count == 0 ? 0 : StatsDays.Average(d => d.Net);

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
        _selectedDate = SelectedDay.Date;

        MealsView = CollectionViewSource.GetDefaultView(Meals);
        ActivitiesView = CollectionViewSource.GetDefaultView(Activities);

        MealsView.Filter = obj =>
        {
            if (obj is not Meal m) return false;
            return m.DateTime.Date == SelectedDate.Date;
        };

        ActivitiesView.Filter = obj =>
        {
            if (obj is not PhysicalActivity a) return false;
            return a.DailyBalance != null && a.DailyBalance.Date.Date == SelectedDate.Date;
        };

        AddMealCommand = new RelayCommand(_ => AddMeal());
        AddActivityCommand = new RelayCommand(_ => AddActivity());

        DeleteMealCommand = new RelayCommand(p => DeleteMeal(p as Meal));
        DeleteActivityCommand = new RelayCommand(p => DeleteActivity(p as PhysicalActivity));

        ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
        ResetSettingsCommand = new RelayCommand(_ => LoadSettingsFromCurrentUser());

        Meals.CollectionChanged += OnAnyCollectionChanged;
        Activities.CollectionChanged += OnAnyCollectionChanged;

        SeedExampleData();

        ApplyDateFilter();
        RefreshComputed();

        LoadSettingsFromCurrentUser();
        RefreshStatistics();
    }

    private void OnAnyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyDateFilter();
        RefreshComputed();
        RefreshStatistics();
    }

    private void ApplyDateFilter()
    {
        MealsView.Refresh();
        ActivitiesView.Refresh();
    }

    private void SeedExampleData()
    {
        Meals.Add(new Meal
        {
            Id = 1,
            DailyBalance = SelectedDay,
            Name = "Owsianka",
            Category = MealCategory.Breakfast,
            DateTime = SelectedDate.Date.AddHours(9),
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
            DateTime = SelectedDate.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute),
            TotalCalories = 300,
            TotalProtein = 10,
            TotalFat = 10,
            TotalCarbs = 40
        });
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
    }

    private void DeleteMeal(Meal? meal)
    {
        if (meal is null) return;
        Meals.Remove(meal);
    }

    private void DeleteActivity(PhysicalActivity? activity)
    {
        if (activity is null) return;
        Activities.Remove(activity);
    }

    public void RefreshAfterEdit()
    {
        ApplyDateFilter();
        RefreshComputed();
        RefreshStatistics();
    }

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(CaloriesConsumed));
        OnPropertyChanged(nameof(CaloriesBurned));
        OnPropertyChanged(nameof(NetBalance));
        OnPropertyChanged(nameof(CaloriesRemainingToGoal));
        OnPropertyChanged(nameof(GoalProgressText));
    }

    private void RefreshStatistics()
    {
        StatsDays.Clear();

        var start = SelectedDate.Date.AddDays(-(SelectedStatsRange - 1));
        var end = SelectedDate.Date;

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var mealsDay = Meals.Where(m => m.DateTime.Date == day).ToList();
            var actsDay = Activities.Where(a => a.DailyBalance != null && a.DailyBalance.Date.Date == day).ToList();

            var consumed = mealsDay.Sum(m => m.TotalCalories);
            var burned = actsDay.Sum(a => a.BurnedCalories);

            StatsDays.Add(new DaySummary
            {
                Date = day,
                Consumed = consumed,
                Burned = burned,
                Net = consumed - burned,
                MealsCount = mealsDay.Count,
                ActivitiesCount = actsDay.Count
            });
        }

        OnPropertyChanged(nameof(StatsTotalConsumed));
        OnPropertyChanged(nameof(StatsTotalBurned));
        OnPropertyChanged(nameof(StatsTotalNet));
        OnPropertyChanged(nameof(StatsAvgNet));
    }

    private void LoadSettingsFromCurrentUser()
    {
        SettingsFirstName = CurrentUser.FirstName;

        SettingsAge = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.Age)));
        SettingsHeightCm = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.HeightCm)));
        SettingsWeightKg = Convert.ToDouble(CurrentUser.WeightKg);

        SettingsGender = CurrentUser.Gender;
        SettingsActivityLevel = CurrentUser.ActivityLevel;

        SettingsDailyCaloriesGoal = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.DailyCaloriesGoal)));
        SettingsProteinPercentGoal = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.ProteinPercentGoal)));
        SettingsFatPercentGoal = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.FatPercentGoal)));
        SettingsCarbsPercentGoal = Convert.ToInt32(Math.Round(Convert.ToDouble(CurrentUser.CarbsPercentGoal)));
    }

    private void ApplySettings()
    {
        if (string.IsNullOrWhiteSpace(SettingsFirstName)) SettingsFirstName = "U¿ytkownik";
        if (SettingsAge < 1) SettingsAge = 1;
        if (SettingsHeightCm < 50) SettingsHeightCm = 50;
        if (SettingsWeightKg < 1) SettingsWeightKg = 1;

        var sum = SettingsProteinPercentGoal + SettingsFatPercentGoal + SettingsCarbsPercentGoal;
        if (sum != 100)
            SettingsCarbsPercentGoal = Math.Max(0, 100 - SettingsProteinPercentGoal - SettingsFatPercentGoal);

        CurrentUser = new User(
            id: CurrentUser.Id,
            firstName: SettingsFirstName,
            age: SettingsAge,
            heightCm: SettingsHeightCm,
            weightKg: SettingsWeightKg,
            gender: SettingsGender,
            activityLevel: SettingsActivityLevel,
            dailyCaloriesGoal: SettingsDailyCaloriesGoal,
            proteinPercentGoal: SettingsProteinPercentGoal,
            fatPercentGoal: SettingsFatPercentGoal,
            carbsPercentGoal: SettingsCarbsPercentGoal
        );

        OnPropertyChanged(nameof(CurrentUser));
        RefreshComputed();
    }
}

public class DaySummary
{
    public DateTime Date { get; set; }
    public int Consumed { get; set; }
    public int Burned { get; set; }
    public int Net { get; set; }
    public int MealsCount { get; set; }
    public int ActivitiesCount { get; set; }
}
