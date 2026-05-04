using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }

        // Nullable StudentId for users of type Instructor
        public int? StudentId { get; set; }

        public User(int id, string name, string email, string password, UserType type, int? studentId = null)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = HashPassword(password);
            Type = type;
            StudentId = studentId;
        }

        private string HashPassword(string password)
        {
            byte[] salt = GetSalt();

            var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32);

            byte[] hashPassword = new byte[48];

            Buffer.BlockCopy(salt, 0, hashPassword, 0, 16);
            Buffer.BlockCopy(hash, 0, hashPassword, 16, 32);

            return Convert.ToBase64String(hashPassword);
        }

        private bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                return false;
            }

            byte[] hashedPasswordInBytes = Convert.FromBase64String(Password);

            byte[] salt = new byte[16];
            Buffer.BlockCopy(hashedPasswordInBytes, 0, salt, 0, 16);

            var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32);

            var hashBytes = new byte[32];
            Buffer.BlockCopy(hashedPasswordInBytes, 16, hashBytes, 0, 32);

            return hash.SequenceEqual<byte>(hashBytes);
        }

        private byte[] GetSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }
    }
}
