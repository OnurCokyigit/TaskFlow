using System.Windows;
using TaskFlow.ViewModels;

namespace TaskFlow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // PasswordBox güvenlik gereği Binding desteklemez
        // Bu yüzden butona tıklanınca şifreyi manuel aktarıyoruz
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = PasswordInput.Password;
        }
        private void PasswordInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.Password = PasswordInput.Password;
                    vm.LoginCommand.Execute(null);
                }
            }
        }
    }
}