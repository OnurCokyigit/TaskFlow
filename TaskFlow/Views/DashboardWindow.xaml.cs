using System.Windows;
using System.Windows.Input;
using TaskFlow.Models;
using TaskFlow.ViewModels;
using TaskFlow.Views;

namespace TaskFlow.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
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