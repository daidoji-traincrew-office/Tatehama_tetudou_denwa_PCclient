using System.Windows;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.Views;

public partial class LocationSelector : Window
{
    public CallListItem? SelectedLocation { get; private set; }

    public LocationSelector()
    {
        InitializeComponent();
        LocationListBox.ItemsSource = LocationData.GetLocations();
        LocationListBox.SelectedIndex = 0;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (LocationListBox.SelectedItem is CallListItem selected)
        {
            SelectedLocation = selected;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("勤務地を選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
