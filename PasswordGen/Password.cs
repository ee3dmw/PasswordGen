using System;

namespace PasswordGen
{
    public class Password
    {
        public Password()
        { }

        public Password(string userId, string salt, string hash)
        {
            UserId = userId;
            PasswordSalt = salt;
            PasswordHash = hash;
            ExpiryTime = DateTime.UtcNow.AddSeconds(30);
        }

        public string UserId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}