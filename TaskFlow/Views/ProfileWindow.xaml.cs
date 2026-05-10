using System.Windows;
using System.Windows.Input;
using TaskFlow.Models;
using TaskFlow.Views;

namespace TaskFlow.Views
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow()
        {
            InitializeComponent();
        }

        private void ProjectCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.DataContext is Project project)
            {
                var detailWindow = new ProjectDetailWindow(project.Id);
                detailWindow.Show();
            }
        }
    }
}