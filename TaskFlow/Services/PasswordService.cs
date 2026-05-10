using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.Services
{
    public class PasswordService
    {
        // Şifreyi hash'e çevirir
        public string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }

        // Girilen şifre ile kayıtlı hash eşleşiyor mu?
        public bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}