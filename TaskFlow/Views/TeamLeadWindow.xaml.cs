using System.Windows;
using System.Windows.Input;
using TaskFlow.Helpers;
using TaskFlow.Services;
using TaskFlow.ViewModels;

namespace TaskFlow.Views
{
    public partial class TeamLeadWindow : Window
    {
        public TeamLeadWindow()
        {
            InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            SessionService.Clear();
            NavigationService.NavigateTo<MainWindow>();
        }

        private void ProjectCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.DataContext is ProjectSummary summary)
            {
                var detailWindow = new ProjectDetailWindow(summary.Id);
                detailWindow.Show();
            }
        }
    }
}