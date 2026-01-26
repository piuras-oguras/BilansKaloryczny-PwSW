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

    // Listy do ComboBoxów w edycji (RowDetails)
    public ObservableCollection<ActivityIntensity> Intensities { get; } =
        new(Enum.GetValues(typeof(ActivityIntensity)).Cast<ActivityIntensity>());

    public ObservableCollection<MealCategory> MealCategories { get; } =
        new(Enum.GetValues(typeof(MealCategory)).Cast<MealCategory>());

    // Dane "Ÿród³owe" (wszystkie rekordy)
    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<PhysicalActivity> Activities { get; } = new();

    // Widoki filtrowane po dacie (pod to bindujesz DataGrid)
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

            // Docelowo: LoadDay(_selectedDate) z bazy/repo
            SelectedDay = new DailyBalance { Id = SelectedDay.Id, Date = _selectedDate, User = CurrentUser };

            ApplyDateFilter();
            RefreshComputed();
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

    // Pola liczone – liczymy tylko z widoków (czyli z wybranego dnia)
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

        // Tworzymy widoki i filtry
        MealsView = CollectionViewSource.GetDefaultView(Meals);
        ActivitiesView = CollectionViewSource.GetDefaultView(Activities);

        MealsView.Filter = obj =>
        {
            if (obj is not Meal m) return false;
            return m.DateTime.Date == SelectedDate.Date;
        };

        // PhysicalActivity u Ciebie nie ma DateTime, wiêc filtrujemy po DailyBalance.Date
        ActivitiesView.Filter = obj =>
        {
            if (obj is not PhysicalActivity a) return false;
            return a.DailyBalance != null && a.DailyBalance.Date.Date == SelectedDate.Date;
        };

        AddMealCommand = new RelayCommand(_ => AddMeal());
        AddActivityCommand = new RelayCommand(_ => AddActivity());

        DeleteMealCommand = new RelayCommand(p => DeleteMeal(p as Meal));
        DeleteActivityCommand = new RelayCommand(p => DeleteActivity(p as PhysicalActivity));

        Meals.CollectionChanged += OnAnyCollectionChanged;
        Activities.CollectionChanged += OnAnyCollectionChanged;

        SeedExampleData();
        ApplyDateFilter();
        RefreshComputed();
    }

    private void OnAnyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyDateFilter();
        RefreshComputed();
    }

    private void ApplyDateFilter()
    {
        MealsView.Refresh();
        ActivitiesView.Refresh();

        OnPropertyChanged(nameof(CaloriesConsumed));
        OnPropertyChanged(nameof(CaloriesBurned));
        OnPropertyChanged(nameof(NetBalance));
        OnPropertyChanged(nameof(CaloriesRemainingToGoal));
        OnPropertyChanged(nameof(GoalProgressText));
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
            DailyBalance = SelectedDay, // kluczowe do filtrowania po dacie
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
    }

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(CaloriesConsumed));
        OnPropertyChanged(nameof(CaloriesBurned));
        OnPropertyChanged(nameof(NetBalance));
        OnPropertyChanged(nameof(CaloriesRemainingToGoal));
        OnPropertyChanged(nameof(GoalProgressText));
    }
}
