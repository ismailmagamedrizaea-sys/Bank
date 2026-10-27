using AvaloniaApplication29.Models;
using AvaloniaApplication29.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication29.Services;

public class UserService(IUserRepository repos) : IUserService
{
    public Task AddAsync(string login, string email)
    {
        throw new NotImplementedException();
    }

    public async Task AddUser(string login, string email)
    {
        var newUser = new User
        {
            Login = login,
            Email = email
        };
        await repos.AddUserAsync(newUser);
    }

    public async Task DeleteAsync(User? user)
    {
        if (user == null) return;
        await repos.DeleteUserAsync(user);
    }

    public async Task<List<User>> GetAllAsync() => await repos.GetUsersAsync();
}
