using System.Windows;
using TaskFlow.Services;

namespace TaskFlow
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Veritabanı ilk verilerini oluştur
            var seedService = new SeedService();
            await seedService.SeedAsync();
        }
    }
}