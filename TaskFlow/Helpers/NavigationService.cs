using System.Windows;

namespace TaskFlow.Helpers
{
    public static class NavigationService
    {
        public static void NavigateTo<T>() where T : Window, new()
        {
            var newWindow = new T();
            newWindow.Show();

            // Mevcut pencereyi kapat
            foreach (Window window in Application.Current.Windows)
            {
                if (window != newWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}