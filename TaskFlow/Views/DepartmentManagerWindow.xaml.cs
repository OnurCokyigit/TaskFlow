using System.Windows;
using System.Windows.Input;
using TaskFlow.ViewModels;
using TaskFlow.Views;

namespace TaskFlow.Views
{
    public partial class DepartmentManagerWindow : Window
    {
        public DepartmentManagerWindow()
        {
            InitializeComponent();
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