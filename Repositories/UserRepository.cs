using System;
using System.Collections.Generic;
using System.Text;
using AvaloniaApplication29.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AvaloniaApplication29.Repositories;

public class UserRepository(IDbContextFactory<AppDbContex> factory) : IUserRepository
{
    public async Task AddUserAsync(User user)
    {
        using var db = factory.CreateDbContext();
        await db.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(User? user)
    {
        using var db = factory.CreateDbContext();
        if (user != null)
        {
            db.Remove(user);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        using var db = factory.CreateDbContext();
        return await db.Users.ToListAsync();
    }
}
