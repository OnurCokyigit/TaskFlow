using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskFlow.Models;
using TaskFlow.ViewModels;
using TaskStatus = TaskFlow.Models.TaskStatus;

namespace TaskFlow.Views
{
    public partial class ProjectDetailWindow : Window
    {
        private ProjectDetailViewModel _vm;
        private Button? _selectedProgressBtn;

        public ProjectDetailWindow(int projectId)
        {
            InitializeComponent();
            _vm = new ProjectDetailViewModel();
            DataContext = _vm;
            Loaded += async (s, e) => await LoadAsync(projectId);
        }

        private async Task LoadAsync(int projectId)
        {
            await _vm.InitializeAsync(projectId);

            ProjectTitleText.Text = _vm.ProjectTitle;
            TeamNameText.Text = _vm.TeamName;
            StartDateText.Text = _vm.StartDateDisplay;
            LeaderText.Text = _vm.LeaderName;

            RefreshUI();
        }

        // Tüm listeleri güncelle
        private void RefreshUI()
        {
            MembersList.ItemsSource = _vm.Members;

            AllTasksList.ItemsSource = null;
            AllTasksList.ItemsSource = _vm.AllTasks;

            TodoList.ItemsSource = null;
            TodoList.ItemsSource = _vm.TodoTasks;

            InProgressList.ItemsSource = null;
            InProgressList.ItemsSource = _vm.InProgressTasks;

            DoneList.ItemsSource = null;
            DoneList.ItemsSource = _vm.DoneTasks;

            NoTasksText.Visibility = _vm.AllTasks.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            CommitsList.ItemsSource = null;
            CommitsList.ItemsSource = _vm.Commits;

            NoCommitsText.Visibility = _vm.Commits.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            DoneTasksList.ItemsSource = null;
            DoneTasksList.ItemsSource = _vm.DoneTasks;
        }

        // ─── GÖREV DURUM DEĞİŞTİRME ──────────────────────

        private async void MoveToTodo_Click(object sender, RoutedEventArgs e)
        {
            if (GetTaskFromSender(sender) is ProjectTask task)
            {
                await _vm.ChangeTaskStatusAsync(task, TaskStatus.Todo);
                RefreshUI();
            }
        }

        private async void MoveToInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (GetTaskFromSender(sender) is ProjectTask task)
            {
                await _vm.ChangeTaskStatusAsync(task, TaskStatus.InProgress);
                RefreshUI();
            }
        }

        private async void MoveToDone_Click(object sender, RoutedEventArgs e)
        {
            if (GetTaskFromSender(sender) is ProjectTask task)
            {
                await _vm.ChangeTaskStatusAsync(task, TaskStatus.Done);
                RefreshUI();
            }
        }

        private ProjectTask? GetTaskFromSender(object sender)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectTask task)
                return task;
            return null;
        }

        // ─── SEKME GEÇİŞLERİ ─────────────────────────────

        private void ShowListView(object sender, MouseButtonEventArgs e)
        {
            ListViewPanel.Visibility = Visibility.Visible;
            KanbanViewPanel.Visibility = Visibility.Collapsed;
            ListTab.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#3498DB"));
            KanbanTab.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#ECF0F1"));
            ((TextBlock)ListTab.Child).Foreground = Brushes.White;
            ((TextBlock)KanbanTab.Child).Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#7F8C8D"));
        }

        private void ShowKanbanView(object sender, MouseButtonEventArgs e)
        {
            ListViewPanel.Visibility = Visibility.Collapsed;
            KanbanViewPanel.Visibility = Visibility.Visible;
            KanbanTab.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#3498DB"));
            ListTab.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#ECF0F1"));
            ((TextBlock)KanbanTab.Child).Foreground = Brushes.White;
            ((TextBlock)ListTab.Child).Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#7F8C8D"));
        }

        // ─── PROGRESS BUTONLARI ───────────────────────────

        private void ProgressBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            if (_selectedProgressBtn != null)
            {
                _selectedProgressBtn.Background = Brushes.White;
                _selectedProgressBtn.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2C3E50"));
                _selectedProgressBtn.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#BDC3C7"));
            }

            btn.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#3498DB"));
            btn.Foreground = Brushes.White;
            btn.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#3498DB"));
            _selectedProgressBtn = btn;

            int value = int.Parse(btn.Tag.ToString()!);
            ProgressBarControl.Value = value;
            ProgressDisplayText.Text = $"%{value * 10} Tamamlandı";
            _vm.ProgressValue = value;
        }

        // ─── COMMİT ──────────────────────────────────────

        private async void SubmitCommit_Click(object sender, RoutedEventArgs e)
        {
            _vm.NewCommitMessage = CommitMessageBox.Text;
            await _vm.SubmitCommitAsync();

            CommitFeedback.Text = _vm.CommitMessage;
            CommitFeedback.Foreground = _vm.CommitMessage.StartsWith("✅")
                ? Brushes.Green : Brushes.Red;

            if (_vm.CommitMessage.StartsWith("✅"))
            {
                CommitMessageBox.Text = string.Empty;
                RefreshUI();
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}