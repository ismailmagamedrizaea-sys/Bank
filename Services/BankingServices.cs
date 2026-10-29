using AvaloniaApplication29.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AvaloniaApplication29.Services
{
    public class BankingService
    {
        public async Task InitializeAsync()
        {
            await using var db = new AppDbContex();
            await db.Database.EnsureCreatedAsync();

            if (await db.Users.AnyAsync())
                return;

            db.Users.AddRange(
                new User { Id = 1, Owner = "Демо-пользователь", Balance = 10000m },
                new User { Id = 2, Owner = "Второй демо-счет", Balance = 5000m }
            );

            await db.SaveChangesAsync();
        }

        public async Task<User> GetAccountAsync(int accountId)
        {
            await using var db = new AppDbContext();

            return await db.User
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

            var sender = await db.Accounts
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
    }
}
