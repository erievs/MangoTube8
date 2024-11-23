using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace MangoTube8
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            string savedQuality = Settings.VideoQuality;

            cmbCurrFrom.ItemsSource = Settings.Qualities;

            cmbCurrFrom.SelectedItem = savedQuality;

            Debug.WriteLine("Initial video quality set to: " + savedQuality);
        }

        private void cmbCurrFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = cmbCurrFrom.SelectedItem as string;

            if (selectedItem != null)
            {
                if (selectedItem != Settings.VideoQuality)
                {
                    Settings.VideoQuality = "Sorry This Isn't Supported On Sliverlight";
                    Debug.WriteLine("Selected video quality: " + selectedItem);
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Visibility == Visibility.Collapsed)
            {
                ShowSearchBox.Begin();
            }
            else
            {
                HideSearchBox.Begin();
            }
        }

        private void HideSearchBox_Completed(object sender, EventArgs e)
        {
            YouTubeLogo.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchText = SearchTextBox.Text;
                Debug.WriteLine("Search Text: " + searchText);

                NavigationService.Navigate(new Uri("/SearchPage.xaml?query=" + Uri.EscapeDataString(searchText), UriKind.Relative));
            }
        }
    }
}