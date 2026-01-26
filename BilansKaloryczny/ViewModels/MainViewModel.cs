using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate.Date == value.Date) return;
            _selectedDate = value.Date;
            OnPropertyChanged();

            // Docelowo: tu bêdzie LoadDay(_selectedDate) z repozytorium/bazy
            SelectedDay = new DailyBalance { Id = SelectedDay.Id, Date = _selectedDate, User = CurrentUser };

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

    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<PhysicalActivity> Activities { get; } = new();

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

    // Pola liczone (tylko do odczytu)
    public int CaloriesConsumed => Meals.Sum(m => m.TotalCalories);
    public int CaloriesBurned => Activities.Sum(a => a.BurnedCalories);
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

        AddMealCommand = new RelayCommand(_ => AddMeal());
        AddActivityCommand = new RelayCommand(_ => AddActivity());

        DeleteMealCommand = new RelayCommand(p => DeleteMeal(p as Meal));
        DeleteActivityCommand = new RelayCommand(p => DeleteActivity(p as PhysicalActivity));

        // Gdy ktoœ doda/usunie element (np. w przysz³oœci z innego miejsca), te¿ odœwie¿
        Meals.CollectionChanged += OnMealsCollectionChanged;
        Activities.CollectionChanged += OnActivitiesCollectionChanged;

        SeedExampleData();
    }

    private void OnMealsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshComputed();
    private void OnActivitiesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshComputed();

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

    private void DeleteMeal(Meal? meal)
    {
        if (meal is null) return;
        Meals.Remove(meal);
        RefreshComputed();
    }

    private void DeleteActivity(PhysicalActivity? activity)
    {
        if (activity is null) return;
        Activities.Remove(activity);
        RefreshComputed();
    }

    // Wywo³uj z MainWindow.xaml.cs po klikniêciu "Zapisz" / "Anuluj"
    public void RefreshAfterEdit() => RefreshComputed();

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(CaloriesConsumed));
        OnPropertyChanged(nameof(CaloriesBurned));
        OnPropertyChanged(nameof(NetBalance));
        OnPropertyChanged(nameof(CaloriesRemainingToGoal));
        OnPropertyChanged(nameof(GoalProgressText));
    }
}
