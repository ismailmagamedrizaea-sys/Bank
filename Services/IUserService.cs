using AvaloniaApplication29.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication29.Services;
public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task AddUser(string login, string email);
    Task DeleteAsync(User? user);
    Task AddAsync(string login, string email);
}
