using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SmartBudgetTracker.Models;

namespace SmartBudgetMobile.Managers
{
    public class AuthenticationManager
    {
        private List<User> _users;
        private User _currentUser;
        private static readonly Dictionary<string, LoginAttempt> _loginAttempts = new();

        public AuthenticationManager()
        {
            _users = FileManager.LoadUsers();
        }

        public User CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;

        public (bool Success, string Message) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            if (IsLockedOut(username))
            {
                var attempt = _loginAttempts[username.ToLower()];
                int remaining = (int)(attempt.LockoutUntil - DateTime.Now).TotalMinutes;
                return (false, $"Account locked. Please try again in {remaining + 1} minute(s).");
            }

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return (false, "Invalid username or password.");

            string hashed = HashPassword(password);
            if (user.PasswordHash != hashed)
            {
                RecordFailedAttempt(username);
                return (false, "Invalid username or password.");
            }

            ResetAttempts(username);
            user.LastLogin = DateTime.Now;
            _currentUser = user;
            FileManager.SaveUsers(_users);
            return (true, "Login successful.");
        }

        public (bool Success, string Message) Register(string username, string password, string fullName, string email)
        {
            var userValid = ValidationManager.ValidateUsername(username);
            if (!userValid.IsValid) return (false, userValid.ErrorMessage);

            var passValid = ValidationManager.ValidatePassword(password);
            if (!passValid.IsValid) return (false, passValid.ErrorMessage);

            var nameValid = ValidationManager.ValidateFullName(fullName);
            if (!nameValid.IsValid) return (false, nameValid.ErrorMessage);

            var emailValid = ValidationManager.ValidateEmail(email);
            if (!emailValid.IsValid) return (false, emailValid.ErrorMessage);

            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return (false, "Username already exists.");

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Email = email ?? string.Empty,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now
            };

            _users.Add(user);
            _currentUser = user;
            FileManager.SaveUsers(_users);
            return (true, "Registration successful.");
        }

        public (bool Success, string Message) ForgotPassword(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot be empty.");

            var passValid = ValidationManager.ValidatePassword(newPassword);
            if (!passValid.IsValid) return (false, passValid.ErrorMessage);

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return (false, "Username not found.");

            user.PasswordHash = HashPassword(newPassword);
            FileManager.SaveUsers(_users);
            return (true, "Password reset successful. You can now login with your new password.");
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public static bool IsLockedOut(string username)
        {
            var key = username.ToLower();
            if (_loginAttempts.ContainsKey(key))
            {
                var attempt = _loginAttempts[key];
                if (attempt.Count >= 5 && DateTime.Now < attempt.LockoutUntil)
                    return true;
                if (DateTime.Now >= attempt.LockoutUntil)
                    _loginAttempts.Remove(key);
            }
            return false;
        }

        private static void RecordFailedAttempt(string username)
        {
            var key = username.ToLower();
            if (_loginAttempts.ContainsKey(key))
            {
                _loginAttempts[key].Count++;
                if (_loginAttempts[key].Count >= 5)
                    _loginAttempts[key].LockoutUntil = DateTime.Now.AddMinutes(5);
            }
            else
            {
                _loginAttempts[key] = new LoginAttempt();
            }
        }

        private static void ResetAttempts(string username)
        {
            _loginAttempts.Remove(username.ToLower());
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public void UpdateUserSettings(bool isDarkMode, bool notificationsEnabled, bool backupReminder)
        {
            if (_currentUser == null) return;

            _currentUser.IsDarkMode = isDarkMode;
            _currentUser.NotificationsEnabled = notificationsEnabled;
            _currentUser.BackupReminder = backupReminder;

            var idx = _users.FindIndex(u => u.Username == _currentUser.Username);
            if (idx >= 0)
            {
                _users[idx] = _currentUser;
                FileManager.SaveUsers(_users);
            }
        }

        public void ReloadUsers()
        {
            _users = FileManager.LoadUsers();
            if (_currentUser != null)
            {
                _currentUser = _users.FirstOrDefault(u => u.Username == _currentUser.Username);
            }
        }
    }
}
