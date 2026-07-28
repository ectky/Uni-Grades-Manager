using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; } = default!;
        public UserType Type { get; private set; }

        // Nullable StudentId for users of type Instructor.
        // NOTE: what this field means for a Student or Admin user, and where a
        // Student's "real" student number is supposed to live, was never
        // resolved in the source discussion — carried through as-is. Don't
        // build logic that depends on its meaning until that's clarified.
        public int? StudentId { get; private set; }

        /// <summary>
        /// EF Core's required parameterless constructor, used only for
        /// materializing existing rows from the database. Deliberately private
        /// and deliberately does NOT touch Password — see the fix note below.
        /// </summary>
        private User() { }

        /// <summary>
        /// Creates a brand-new user with a plaintext password, which is hashed
        /// immediately. Use this from AuthService.RegisterAsync — never for
        /// reconstructing a user that already exists in the database.
        /// </summary>
        public User(string name, string email, string password, UserType type, int? studentId = null)
        {
            Name = name;
            Email = email;
            Password = HashPassword(password);
            Type = type;
            StudentId = studentId;
        }

        /// <summary>
        /// Verifies a login attempt against the stored PBKDF2 hash.
        /// Fix: this was originally private, which meant nothing outside the
        /// class — i.e. AuthService — could ever call it, so login could not
        /// actually work. Made public; everything else about the algorithm is
        /// unchanged from what you gave me.
        /// </summary>
        public bool VerifyPassword(string password)
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

            return hash.SequenceEqual(hashBytes);
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
