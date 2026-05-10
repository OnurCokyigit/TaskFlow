using TaskFlow.Models;

namespace TaskFlow.Services
{
    public static class SessionService
    {
        public static User? CurrentUser { get; private set; }

        public static void SetUser(User user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}