using System;

namespace SmartBudgetMobile.Models
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsDarkMode { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public bool BackupReminder { get; set; } = true;
    }
}
