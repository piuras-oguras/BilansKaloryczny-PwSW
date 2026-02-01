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

    // ComboBox-y
    public ObservableCollection<ActivityIntensity> Intensities { get; } =
        new(Enum.GetValues(typeof(ActivityIntensity)).Cast<ActivityIntensity>());

    public ObservableCollection<MealCategory> MealCategories { get; } =
        new(Enum.GetValues(typeof(MealCategory)).Cast<MealCategory>());

    public ObservableCollection<Gender> Genders { get; } =
        new(Enum.GetValues(typeof(Gender)).Cast<Gender>());

    public ObservableCollection<ActivityLevel> ActivityLevels { get; } =
        new(Enum.GetValues(typeof(ActivityLevel)).Cast<ActivityLevel>());

    // Dane Ÿród³owe (wszystkie rekordy)
    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<PhysicalActivity> Activities { get; } = new();

    // Widoki filtrowane po dacie (pod DataGrid)
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
        set { _selectedDay = value; OnPropertyChanged(); }
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

    // ===== HISTORIA (szablony) =====
    public ObservableCollection<Meal> MealHistory { get; } = new();
    public ObservableCollection<PhysicalActivity> ActivityHistory { get; } = new();

    private Meal? _selectedMealFromHistory;
    public Meal? SelectedMealFromHistory
    {
        get => _selectedMealFromHistory;
        set
        {
            _selectedMealFromHistory = value;
            OnPropertyChanged();
            AddMealFromHistoryCommand.RaiseCanExecuteChanged();
        }
    }

    private PhysicalActivity? _selectedActivityFromHistory;
    public PhysicalActivity? SelectedActivityFromHistory
    {
        get => _selectedActivityFromHistory;
        set
        {
            _selectedActivityFromHistory = value;
            OnPropertyChanged();
            AddActivityFromHistoryCommand.RaiseCanExecuteChanged();
        }
    }

    // Liczniki liczone z widoków (czyli z wybranego dnia)
    public int CaloriesConsumed => MealsView.Cast<Meal>().Sum(m => m.TotalCalories);
    public int CaloriesBurned => ActivitiesView.Cast<PhysicalActivity>().Sum(a => a.BurnedCalories);
    public int NetBalance => CaloriesConsumed - CaloriesBurned;
    public int CaloriesRemainingToGoal => Math.Max(0, CurrentUser.DailyCaloriesGoal - CaloriesConsumed);

    // ===== MAKRO (dla wybranego dnia) =====
    public double ProteinConsumed => MealsView.Cast<Meal>().Sum(m => m.TotalProtein);
    public double FatConsumed => MealsView.Cast<Meal>().Sum(m => m.TotalFat);
    public double CarbsConsumed => MealsView.Cast<Meal>().Sum(m => m.TotalCarbs);

    // Cele w gramach (prosto z User)
    public double ProteinGoalGrams => CurrentUser.ProteinGramsGoal;
    public double FatGoalGrams => CurrentUser.FatGramsGoal;
    public double CarbsGoalGrams => CurrentUser.CarbsGramsGoal;

    public double ProteinRemainingToGoal => Math.Max(0, ProteinGoalGrams - ProteinConsumed);
    public double FatRemainingToGoal => Math.Max(0, FatGoalGrams - FatConsumed);
    public double CarbsRemainingToGoal => Math.Max(0, CarbsGoalGrams - CarbsConsumed);

    public string ProteinProgressText => $"{ProteinConsumed:0.#} / {ProteinGoalGrams:0.#} g";
    public string FatProgressText => $"{FatConsumed:0.#} / {FatGoalGrams:0.#} g";
    public string CarbsProgressText => $"{CarbsConsumed:0.#} / {CarbsGoalGrams:0.#} g";

    public string GoalProgressText
        => $"{CaloriesConsumed} / {CurrentUser.DailyCaloriesGoal} kcal ({(CurrentUser.DailyCaloriesGoal == 0 ? 0 : (int)Math.Round(100.0 * CaloriesConsumed / CurrentUser.DailyCaloriesGoal))}%)";

    // Komendy
    public RelayCommand AddMealCommand { get; }
    public RelayCommand AddActivityCommand { get; }
    public RelayCommand DeleteMealCommand { get; }
    public RelayCommand DeleteActivityCommand { get; }

    public RelayCommand AddMealFromHistoryCommand { get; }
    public RelayCommand AddActivityFromHistoryCommand { get; }

    public RelayCommand ApplySettingsCommand { get; }
    public RelayCommand ResetSettingsCommand { get; }

    // ===== USTAWIENIA (pola edycyjne) =====
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

    // Makro cele w gramach (zamiast %)
    private double _settingsProteinG;
    public double SettingsProteinGramsGoal
    {
        get => _settingsProteinG;
        set { _settingsProteinG = value; OnPropertyChanged(); }
    }

    private double _settingsFatG;
    public double SettingsFatGramsGoal
    {
        get => _settingsFatG;
        set { _settingsFatG = value; OnPropertyChanged(); }
    }

    private double _settingsCarbsG;
    public double SettingsCarbsGramsGoal
    {
        get => _settingsCarbsG;
        set { _settingsCarbsG = value; OnPropertyChanged(); }
    }

    // ===== STATYSTYKI =====
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
        // UWAGA: konstruktor User musi przyjmowaæ gramy (albo ustaw je po konstruktorze)
        CurrentUser = new User(
            id: 1,
            firstName: "Szymon",
            age: 22,
            heightCm: 180,
            weightKg: 80,
            gender: Gender.Male,
            activityLevel: ActivityLevel.Moderate,
            dailyCaloriesGoal: 2500,
            proteinGramsGoal: 160,
            fatGramsGoal: 80,
            carbsGramsGoal: 260
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
            return a.DateTime.Date == SelectedDate.Date;
        };

        AddMealCommand = new RelayCommand(_ => AddMeal());
        AddActivityCommand = new RelayCommand(_ => AddActivity());

        AddMealFromHistoryCommand = new RelayCommand(_ => AddMealFromHistory(), _ => SelectedMealFromHistory != null);
        AddActivityFromHistoryCommand = new RelayCommand(_ => AddActivityFromHistory(), _ => SelectedActivityFromHistory != null);

        DeleteMealCommand = new RelayCommand(p => DeleteMeal(p as Meal));
        DeleteActivityCommand = new RelayCommand(p => DeleteActivity(p as PhysicalActivity));

        ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
        ResetSettingsCommand = new RelayCommand(_ => LoadSettingsFromCurrentUser());

        Meals.CollectionChanged += OnAnyCollectionChanged;
        Activities.CollectionChanged += OnAnyCollectionChanged;

        SeedExampleData();
        RefreshHistory();

        ApplyDateFilter();
        RefreshComputed();

        LoadSettingsFromCurrentUser();
        RefreshStatistics();
    }

    private void OnAnyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyDateFilter();
        RefreshHistory();
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
            DateTime = SelectedDate.Date.AddHours(18),
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
            DateTime = SelectedDate.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute),
            DurationMin = 20,
            BurnedCalories = 150,
            Intensity = ActivityIntensity.Medium
        });
    }

    private void RefreshHistory()
    {
        var mealPresets = Meals
            .GroupBy(m => new { m.Name, m.Category, m.TotalCalories, m.TotalProtein, m.TotalFat, m.TotalCarbs })
            .Select(g => g.First())
            .OrderBy(m => m.Name)
            .ToList();

        MealHistory.Clear();
        foreach (var m in mealPresets)
            MealHistory.Add(m);

        var actPresets = Activities
            .GroupBy(a => new { a.Name, a.DurationMin, a.BurnedCalories, a.Intensity })
            .Select(g => g.First())
            .OrderBy(a => a.Name)
            .ToList();

        ActivityHistory.Clear();
        foreach (var a in actPresets)
            ActivityHistory.Add(a);

        AddMealFromHistoryCommand.RaiseCanExecuteChanged();
        AddActivityFromHistoryCommand.RaiseCanExecuteChanged();
    }

    private void AddMealFromHistory()
    {
        if (SelectedMealFromHistory is null) return;

        var nextId = Meals.Count == 0 ? 1 : Meals.Max(m => m.Id) + 1;

        Meals.Add(new Meal
        {
            Id = nextId,
            DailyBalance = SelectedDay,
            Name = SelectedMealFromHistory.Name,
            Category = SelectedMealFromHistory.Category,
            DateTime = SelectedDate.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute),
            TotalCalories = SelectedMealFromHistory.TotalCalories,
            TotalProtein = SelectedMealFromHistory.TotalProtein,
            TotalFat = SelectedMealFromHistory.TotalFat,
            TotalCarbs = SelectedMealFromHistory.TotalCarbs
        });
    }

    private void AddActivityFromHistory()
    {
        if (SelectedActivityFromHistory is null) return;

        var nextId = Activities.Count == 0 ? 1 : Activities.Max(a => a.Id) + 1;

        Activities.Add(new PhysicalActivity
        {
            Id = nextId,
            DailyBalance = SelectedDay,
            Name = SelectedActivityFromHistory.Name,
            DateTime = SelectedDate.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute),
            DurationMin = SelectedActivityFromHistory.DurationMin,
            BurnedCalories = SelectedActivityFromHistory.BurnedCalories,
            Intensity = SelectedActivityFromHistory.Intensity
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
        RefreshHistory();
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

        OnPropertyChanged(nameof(ProteinConsumed));
        OnPropertyChanged(nameof(FatConsumed));
        OnPropertyChanged(nameof(CarbsConsumed));
        OnPropertyChanged(nameof(ProteinGoalGrams));
        OnPropertyChanged(nameof(FatGoalGrams));
        OnPropertyChanged(nameof(CarbsGoalGrams));
        OnPropertyChanged(nameof(ProteinRemainingToGoal));
        OnPropertyChanged(nameof(FatRemainingToGoal));
        OnPropertyChanged(nameof(CarbsRemainingToGoal));
        OnPropertyChanged(nameof(ProteinProgressText));
        OnPropertyChanged(nameof(FatProgressText));
        OnPropertyChanged(nameof(CarbsProgressText));
    }

    private void RefreshStatistics()
    {
        StatsDays.Clear();

        var start = SelectedDate.Date.AddDays(-(SelectedStatsRange - 1));
        var end = SelectedDate.Date;

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var mealsDay = Meals.Where(m => m.DateTime.Date == day).ToList();
            var actsDay = Activities.Where(a => a.DateTime.Date == day).ToList();

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

        SettingsAge = CurrentUser.Age;
        SettingsHeightCm = CurrentUser.HeightCm;
        SettingsWeightKg = CurrentUser.WeightKg;

        SettingsGender = CurrentUser.Gender;
        SettingsActivityLevel = CurrentUser.ActivityLevel;

        SettingsDailyCaloriesGoal = CurrentUser.DailyCaloriesGoal;

        // gramy
        SettingsProteinGramsGoal = CurrentUser.ProteinGramsGoal;
        SettingsFatGramsGoal = CurrentUser.FatGramsGoal;
        SettingsCarbsGramsGoal = CurrentUser.CarbsGramsGoal;
    }

    private void ApplySettings()
    {
        if (string.IsNullOrWhiteSpace(SettingsFirstName)) SettingsFirstName = "U¿ytkownik";
        if (SettingsAge < 1) SettingsAge = 1;
        if (SettingsHeightCm < 50) SettingsHeightCm = 50;
        if (SettingsWeightKg < 1) SettingsWeightKg = 1;

        if (SettingsDailyCaloriesGoal < 0) SettingsDailyCaloriesGoal = 0;

        if (SettingsProteinGramsGoal < 0) SettingsProteinGramsGoal = 0;
        if (SettingsFatGramsGoal < 0) SettingsFatGramsGoal = 0;
        if (SettingsCarbsGramsGoal < 0) SettingsCarbsGramsGoal = 0;

        CurrentUser = new User(
            id: CurrentUser.Id,
            firstName: SettingsFirstName,
            age: SettingsAge,
            heightCm: SettingsHeightCm,
            weightKg: SettingsWeightKg,
            gender: SettingsGender,
            activityLevel: SettingsActivityLevel,
            dailyCaloriesGoal: SettingsDailyCaloriesGoal,
            proteinGramsGoal: SettingsProteinGramsGoal,
            fatGramsGoal: SettingsFatGramsGoal,
            carbsGramsGoal: SettingsCarbsGramsGoal
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
