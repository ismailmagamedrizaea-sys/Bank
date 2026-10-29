using AvaloniaApplication29.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Principal;

namespace AvaloniaApplication29;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Operation> Operations => Set<Operation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankDemo");

        Directory.CreateDirectory(folder);

        var databasePath = Path.Combine(folder, "bank.db");
        optionsBuilder.UseSqlite($"Data Source={databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Operation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(operation => operation.AccountId);
    }
}