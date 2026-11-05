using AvaloniaApplication29.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AvaloniaApplication29.Services
{
    public class BankingService
    {
        private const int PasswordIterations = 600_000;

        public async Task InitializeAsync()
        {
            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            await EnsureUserColumnsAsync(db);
        }

        public async Task<User> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password)
        {
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            email = email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new InvalidOperationException("Введите имя и фамилию.");

            if (!IsValidEmail(email))
                throw new InvalidOperationException("Введите корректный Email.");

            if (password.Length < 8)
                throw new InvalidOperationException("Пароль должен содержать не менее 8 символов.");

            await using var db = new AppDbContext();

            if (await db.Users.AnyAsync(account => account.Email == email))
                throw new InvalidOperationException("Пользователь с таким Email уже зарегистрирован.");

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Owner = $"{firstName} {lastName}",
                Email = email,
                PasswordHash = HashPassword(password),
                Balance = 0m
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();

            await using var db = new AppDbContext();
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(account => account.Email == email);

            return user is not null && VerifyPassword(password, user.PasswordHash)
                ? user
                : null;
        }

        public async Task<User> GetAccountAsync(int accountId)
        {
            await using var db = new AppDbContext();

            return await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(account => account.Id == accountId)
                ?? throw new InvalidOperationException("Счет не найден.");
        }

        public async Task<List<Operation>> GetHistoryAsync(int accountId)
        {
            await using var db = new AppDbContext();

            return await db.Operations
                .AsNoTracking()
                .Where(operation => operation.AccountId == accountId)
                .OrderByDescending(operation => operation.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task DepositAsync(int accountId, decimal amount)
        {
            ValidateAmount(amount);

            await using var db = new AppDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            var account = await db.Users
                .FirstOrDefaultAsync(item => item.Id == accountId)
                ?? throw new InvalidOperationException("Счет не найден.");

            account.Balance += amount;

            db.Operations.Add(new Operation
            {
                AccountId = accountId,
                Kind = "Пополнение",
                SignedAmount = amount,
                Description = "Пополнение баланса",
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task PurchaseAsync(int accountId, string title, decimal amount)
        {
            ValidateAmount(amount);

            await using var db = new AppDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            var account = await db.Users
                .FirstOrDefaultAsync(item => item.Id == accountId)
                ?? throw new InvalidOperationException("Счет не найден.");

            if (account.Balance < amount)
                throw new InvalidOperationException("Недостаточно средств.");

            account.Balance -= amount;

            db.Operations.Add(new Operation
            {
                AccountId = accountId,
                Kind = "Покупка",
                SignedAmount = -amount,
                Description = $"Покупка: {title}",
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task TransferAsync(
            int fromAccountId,
            int toAccountId,
            decimal amount)
        {
            ValidateAmount(amount);

            if (fromAccountId == toAccountId)
                throw new InvalidOperationException("Нельзя перевести деньги на тот же счет.");

            await using var db = new AppDbContext();
            await using var transaction = await db.Database.BeginTransactionAsync();

            var sender = await db.Users
                .FirstOrDefaultAsync(account => account.Id == fromAccountId)
                ?? throw new InvalidOperationException("Счет отправителя не найден.");

            var recipient = await db.Users
                .FirstOrDefaultAsync(account => account.Id == toAccountId)
                ?? throw new InvalidOperationException("Счет получателя не найден.");

            if (sender.Balance < amount)
                throw new InvalidOperationException("Недостаточно средств.");

            sender.Balance -= amount;
            recipient.Balance += amount;

            var now = DateTime.UtcNow;

            db.Operations.AddRange(
                new Operation
                {
                    AccountId = fromAccountId,
                    Kind = "Перевод",
                    SignedAmount = -amount,
                    Description = $"Перевод счету №{toAccountId}",
                    CreatedAtUtc = now
                },
                new Operation
                {
                    AccountId = toAccountId,
                    Kind = "Перевод",
                    SignedAmount = amount,
                    Description = $"Перевод от счета №{fromAccountId}",
                    CreatedAtUtc = now
                });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private static void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Сумма должна быть больше нуля.");
        }

        private static async Task EnsureUserColumnsAsync(AppDbContext db)
        {
            await db.Database.OpenConnectionAsync();
            var connection = db.Database.GetDbConnection();
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Users');";
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    columns.Add(reader.GetString(1));
            }

            var requiredColumns = new Dictionary<string, string>
            {
                ["FirstName"] = "TEXT NOT NULL DEFAULT ''",
                ["LastName"] = "TEXT NOT NULL DEFAULT ''",
                ["Email"] = "TEXT NOT NULL DEFAULT ''",
                ["PasswordHash"] = "TEXT NOT NULL DEFAULT ''"
            };

            foreach (var column in requiredColumns)
            {
                if (columns.Contains(column.Key))
                    continue;

                await using var command = connection.CreateCommand();
                command.CommandText = $"ALTER TABLE \"Users\" ADD COLUMN \"{column.Key}\" {column.Value};";
                await command.ExecuteNonQueryAsync();
            }

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Users SET Email = 'legacy-' || Id || '@invalid.local' WHERE Email = ''; " +
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users (Email);");
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                PasswordIterations,
                HashAlgorithmName.SHA256,
                32);

            return $"pbkdf2-sha256${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4
                || parts[0] != "pbkdf2-sha256"
                || !int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
