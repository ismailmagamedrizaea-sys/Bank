using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaApplication29;
using AvaloniaApplication29.Models;

namespace AvaloniaApplication29.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetUsersAsync();
    Task AddUserAsync(User user);
    Task DeleteUserAsync(User user);
}
