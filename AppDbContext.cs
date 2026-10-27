using AvaloniaApplication29.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Principal;

namespace AvaloniaApplication29;

public class AppDbContex : DbContext
{
    public DbSet<User> Users => Set<User>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={AppContext.BaseDirectory}/app.db");
    }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Operation>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(operation => operation.AccountId);
        }
    }
}