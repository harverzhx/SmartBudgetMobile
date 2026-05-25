using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public static class FileManager
    {
        private static readonly string BasePath = Path.Combine(FileSystem.AppDataDirectory, "data");
        private static readonly string UsersFile = Path.Combine(BasePath, "users.json");
        private static readonly string ExpensesFile = Path.Combine(BasePath, "expenses.json");
        private static readonly string BudgetsFile = Path.Combine(BasePath, "budgets.json");
        private static readonly string BillsFile = Path.Combine(BasePath, "bills.json");
        private static readonly string SavingsFile = Path.Combine(BasePath, "savings.json");
        private static readonly string LogsFile = Path.Combine(BasePath, "logs.json");
        private static readonly string TemplatesFile = Path.Combine(BasePath, "templates.json");
        private static readonly string IncomesFile = Path.Combine(BasePath, "incomes.json");
        private static readonly string BackupPath = Path.Combine(FileSystem.AppDataDirectory, "backups");

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static void Initialize()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            if (!Directory.Exists(BackupPath))
                Directory.CreateDirectory(BackupPath);

            if (!File.Exists(UsersFile))
                File.WriteAllText(UsersFile, "[]");

            if (!File.Exists(ExpensesFile))
                File.WriteAllText(ExpensesFile, "[]");

            if (!File.Exists(BudgetsFile))
                File.WriteAllText(BudgetsFile, "[]");

            if (!File.Exists(BillsFile))
                File.WriteAllText(BillsFile, "[]");

            if (!File.Exists(SavingsFile))
                File.WriteAllText(SavingsFile, "[]");

            if (!File.Exists(LogsFile))
                File.WriteAllText(LogsFile, "[]");

            if (!File.Exists(TemplatesFile))
                File.WriteAllText(TemplatesFile, "[]");

            if (!File.Exists(IncomesFile))
                File.WriteAllText(IncomesFile, "[]");
        }

        private static T LoadData<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "[]");
                    return new T();
                }
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json, JsonSettings);
            }
            catch
            {
                return new T();
            }
        }

        private static void Serialize<T>(string filePath, T data)
        {
            string json = JsonConvert.SerializeObject(data, JsonSettings);
            File.WriteAllText(filePath, json);
        }

        public static List<User> LoadUsers()
        {
            return LoadData<List<User>>(UsersFile) ?? new List<User>();
        }

        public static List<Expense> LoadExpenses()
        {
            return LoadData<List<Expense>>(ExpensesFile) ?? new List<Expense>();
        }

        public static List<Budget> LoadBudgets()
        {
            return LoadData<List<Budget>>(BudgetsFile) ?? new List<Budget>();
        }

        public static List<Bill> LoadBills()
        {
            return LoadData<List<Bill>>(BillsFile) ?? new List<Bill>();
        }

        public static List<Savings> LoadSavings()
        {
            return LoadData<List<Savings>>(SavingsFile) ?? new List<Savings>();
        }

        public static List<NotificationLog> LoadLogs()
        {
            return LoadData<List<NotificationLog>>(LogsFile) ?? new List<NotificationLog>();
        }

        public static void SaveUsers(List<User> users)
        {
            Serialize(UsersFile, users);
        }

        public static void SaveExpenses(List<Expense> expenses)
        {
            Serialize(ExpensesFile, expenses);
        }

        public static void SaveBudgets(List<Budget> budgets)
        {
            Serialize(BudgetsFile, budgets);
        }

        public static void SaveBills(List<Bill> bills)
        {
            Serialize(BillsFile, bills);
        }

        public static void SaveSavings(List<Savings> savings)
        {
            Serialize(SavingsFile, savings);
        }

        public static void SaveLogs(List<NotificationLog> logs)
        {
            Serialize(LogsFile, logs);
        }

        public static List<ExpenseTemplate> LoadTemplates()
        {
            return LoadData<List<ExpenseTemplate>>(TemplatesFile) ?? new List<ExpenseTemplate>();
        }

        public static void SaveTemplates(List<ExpenseTemplate> templates)
        {
            Serialize(TemplatesFile, templates);
        }

        public static List<Income> LoadIncomes()
        {
            return LoadData<List<Income>>(IncomesFile) ?? new List<Income>();
        }

        public static void SaveIncomes(List<Income> incomes)
        {
            Serialize(IncomesFile, incomes);
        }

        public static void CreateBackup()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(BackupPath, $"backup_{timestamp}");
            Directory.CreateDirectory(backupDir);

            foreach (string file in Directory.GetFiles(BasePath, "*.json"))
            {
                string dest = Path.Combine(backupDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            string readme = $"Backup created on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            File.WriteAllText(Path.Combine(backupDir, "backup_info.txt"), readme);
        }

        public static List<string> GetAvailableBackups()
        {
            if (!Directory.Exists(BackupPath))
                return new List<string>();

            return Directory.GetDirectories(BackupPath)
                .Select(d => new { Path = d, Time = Directory.GetCreationTime(d) })
                .OrderByDescending(x => x.Time)
                .Select(x => x.Path)
                .ToList();
        }

        public static void RestoreFromBackup(string backupDir)
        {
            if (!Directory.Exists(backupDir))
                throw new DirectoryNotFoundException("Backup directory not found.");

            foreach (string file in Directory.GetFiles(backupDir, "*.json"))
            {
                string dest = Path.Combine(BasePath, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
        }

        public static void ExportToTxt(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public static void ExportToCsv(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public static string ExportBackupForSharing()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string exportDir = Path.Combine(FileSystem.CacheDirectory, "export");
            if (Directory.Exists(exportDir))
                Directory.Delete(exportDir, true);
            Directory.CreateDirectory(exportDir);

            foreach (string file in Directory.GetFiles(BasePath, "*.json"))
            {
                string dest = Path.Combine(exportDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            string readme = $"SmartBudget Backup - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            File.WriteAllText(Path.Combine(exportDir, "backup_info.txt"), readme);

            string zipPath = Path.Combine(FileSystem.CacheDirectory, $"SmartBudget_Backup_{timestamp}.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            System.IO.Compression.ZipFile.CreateFromDirectory(exportDir, zipPath);
            Directory.Delete(exportDir, true);

            return zipPath;
        }

        public static string GetDataPath()
        {
            return BasePath;
        }

        public static void ResetAllData()
        {
            if (Directory.Exists(BasePath))
            {
                foreach (string file in Directory.GetFiles(BasePath, "*.json"))
                {
                    File.Delete(file);
                }
            }
            Initialize();
        }
    }
}
