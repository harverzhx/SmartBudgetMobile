using System;

namespace SmartBudgetTracker.Models
{
    public class LoginAttempt
    {
        public int Count { get; set; }
        public DateTime LockoutUntil { get; set; }

        public LoginAttempt()
        {
            Count = 1;
            LockoutUntil = DateTime.MinValue;
        }
    }
}
