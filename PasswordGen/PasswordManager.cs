using System;

namespace PasswordGen
{
    public class PasswordManager
    {
        private readonly IRepository<Password> _passwordRepository;

        public PasswordManager(IRepository<Password> passwordRepository)
        {
            _passwordRepository = passwordRepository;
        }

        public string GeneratePassword(string userId)
        {
            var generatedPassword = GenerateRandomPassword();

            // TODO - generate a one-time secure salt
            var generatedSalt = GenerateSalt();

            // TODO use more secure hashing algorithm such as SHA256Managed
            var generatedHash = GenerateHash(generatedPassword, generatedSalt);

            _passwordRepository.Save(new Password(userId, generatedSalt, generatedHash));

            return generatedPassword;
        }

        public bool CheckPassword(string userId, string password)
        {
            var passwordFromDatabase = _passwordRepository.Load(userId);

            if (passwordFromDatabase == null)
                return false;

            var passwordHashToCheck = GenerateHash(password, passwordFromDatabase.PasswordSalt);
            if (passwordHashToCheck != passwordFromDatabase.PasswordHash)
                return false;

            if (DateTime.UtcNow > passwordFromDatabase.ExpiryTime)
                return false;

            return true;
        }

        private static string GenerateRandomPassword()
        {
            var random = new Random();
            var generatedPassword = random.Next(int.MaxValue).ToString();
            return generatedPassword;
        }

        private string GenerateHash(string generatedPassword, string generatedSalt)
        {
            return String.Concat(generatedPassword, generatedSalt).GetHashCode().ToString();
        }

        private static string GenerateSalt()
        {
            return "hardcoded-salt";
        }
    }
}
