using System.Windows;
using System.Windows.Controls;
using BilansKaloryczny.ViewModels;

namespace BilansKaloryczny
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void EditRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not DataGrid grid) return;

            var item = btn.DataContext;
            if (item is null) return;

            grid.SelectedItem = item;
            grid.ScrollIntoView(item);

            if (grid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
            {
                row.DetailsVisibility = Visibility.Visible;
                grid.BeginEdit();
            }
        }

        private void SaveRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not DataGrid grid) return;

            grid.CommitEdit(DataGridEditingUnit.Row, true);

            var item = grid.SelectedItem;
            if (item is null) return;

            if (grid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                row.DetailsVisibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
                vm.RefreshAfterEdit();
        }

        private void CancelRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not DataGrid grid) return;

            grid.CancelEdit(DataGridEditingUnit.Row);

            var item = grid.SelectedItem;
            if (item is null) return;

            if (grid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                row.DetailsVisibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
                vm.RefreshAfterEdit();
        }
    }
}
