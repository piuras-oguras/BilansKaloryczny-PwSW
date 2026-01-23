using System.Windows;
using BilansKaloryczny.ViewModels;

namespace BilansKaloryczny;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}