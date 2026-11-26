using System.Security.Cryptography;
using System.Text;

namespace NET_MAUI_App.Services
{
    public class LocalAccountService
    {
        private const string AccountKey = "local_account_v1";
        private const int Iterations = 210_000;

        public async Task<bool> AccountExistsAsync()
        {
            return await SecureStorage.Default.GetAsync(AccountKey) is not null;
        }

        public async Task CreateAccountAsync(string name, string email, string password)
        {
            if (await AccountExistsAsync())
                throw new InvalidOperationException("Auf diesem Gerät ist bereits ein Konto vorhanden.");

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = await Task.Run(() => HashPassword(password, salt));
            var account = string.Join(":", "2", Encode(name), Encode(email),
                Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            await SecureStorage.Default.SetAsync(AccountKey, account);
        }

        public async Task<bool> VerifyAsync(string email, string password)
        {
            var account = await SecureStorage.Default.GetAsync(AccountKey);
            if (account is null)
                throw new InvalidOperationException("Bitte erstelle zuerst ein Konto auf diesem Gerät.");

            var parts = account.Split(':');
            string savedEmail;
            string encodedSalt;
            string encodedHash;
            if (parts.Length == 5 && parts[0] == "2")
            {
                savedEmail = Decode(parts[2]);
                encodedSalt = parts[3];
                encodedHash = parts[4];
            }
            else if (parts.Length == 4 && parts[0] == "1")
            {
                savedEmail = Decode(parts[1]);
                encodedSalt = parts[2];
                encodedHash = parts[3];
            }
            else
            {
                throw new InvalidOperationException("Die gespeicherten Kontodaten sind ungültig.");
            }

            var salt = Convert.FromBase64String(encodedSalt);
            var expectedHash = Convert.FromBase64String(encodedHash);
            if (salt.Length != 16 || expectedHash.Length != 32)
                throw new InvalidOperationException("Die gespeicherten Kontodaten sind ungültig.");

            var actualHash = await Task.Run(() => HashPassword(password, salt));
            return string.Equals(savedEmail, email, StringComparison.OrdinalIgnoreCase)
                && CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }

        public async Task<string> GetDisplayNameAsync()
        {
            var account = await SecureStorage.Default.GetAsync(AccountKey);
            if (account is null)
                return "Gast";

            var parts = account.Split(':');
            return parts.Length == 5 && parts[0] == "2" ? Decode(parts[1]) : "Gast";
        }

        private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

        private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        }
    }
}
