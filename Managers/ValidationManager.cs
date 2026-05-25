using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartBudgetMobile.Managers
{
    public static class ValidationManager
    {
        public static (bool IsValid, string ErrorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot be empty.");

            if (username.Length < 3)
                return (false, "Username must be at least 3 characters.");

            if (username.Length > 50)
                return (false, "Username must not exceed 50 characters.");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                return (false, "Username can only contain letters, numbers, and underscores.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters.");

            if (password.Length > 100)
                return (false, "Password must not exceed 100 characters.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (true, string.Empty);

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email ? (true, string.Empty) : (false, "Invalid email format.");
            }
            catch
            {
                return (false, "Invalid email format.");
            }
        }

        public static (bool IsValid, string ErrorMessage) ValidateAmount(string amount)
        {
            if (string.IsNullOrWhiteSpace(amount))
                return (false, "Amount cannot be empty.");

            if (!decimal.TryParse(amount, out decimal value))
                return (false, "Amount must be a valid number.");

            if (value < 0)
                return (false, "Amount cannot be negative.");

            if (value > 999999999.99m)
                return (false, "Amount is too large.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateRequiredField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, $"{fieldName} cannot be empty.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return (false, "Date cannot be empty.");

            if (!DateTime.TryParse(dateStr, out DateTime date))
                return (false, "Invalid date format.");

            if (date.Year < 2000 || date.Year > 2100)
                return (false, "Date year must be between 2000 and 2100.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateDate(DateTime date)
        {
            if (date.Year < 2000 || date.Year > 2100)
                return (false, "Date year must be between 2000 and 2100.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full name cannot be empty.");

            if (fullName.Length < 2)
                return (false, "Full name must be at least 2 characters.");

            if (fullName.Length > 100)
                return (false, "Full name must not exceed 100 characters.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateSearchTerm(string term)
        {
            if (!string.IsNullOrEmpty(term) && term.Length > 100)
                return (false, "Search term too long.");

            return (true, string.Empty);
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Trim();
        }
    }
}
