using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskFlow.Services;
using TaskFlow.Helpers;

namespace TaskFlow.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService();
        }

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        // Şifre XAML'dan direkt alınacak (güvenlik için)
        public string Password { get; set; } = string.Empty;

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            var (success, message) = await _authService.LoginAsync(Username, Password);

            IsLoading = false;

            if (!success)
            {
                ErrorMessage = message;
                return;
            }

            // Role göre yönlendir
            switch (_authService.CurrentUser!.Role)
            {
                case Models.UserRole.Admin:
                    NavigationService.NavigateTo<Views.DashboardWindow>();
                    break;
                case Models.UserRole.DepartmentManager:
                    NavigationService.NavigateTo<Views.DepartmentManagerWindow>(); // ← değişti
                    break;
                case Models.UserRole.TeamLead:
                    NavigationService.NavigateTo<Views.TeamLeadWindow>();
                    break;
                case Models.UserRole.Employee:
                    NavigationService.NavigateTo<Views.ProfileWindow>();
                    break;
            }
        }
    }
}