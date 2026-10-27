using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using AvaloniaApplication29.Models;
using AvaloniaApplication29;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication29.Services;

namespace AvaloniaApplication29.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _login = "";

    [ObservableProperty]
    private string _email = "";

    public ObservableCollection<User> Users { get; set; } = [];

    [ObservableProperty]
    private User? _selectedUser;

    private IUserService userService;

    public MainViewModel(IUserService _us) { userService = _us; _ = LoadUsersAsync(); }

    private async Task LoadUsersAsync()
    {
        Users.Clear();
        var urs = await userService.GetAllAsync();
        foreach (var u in urs) Users.Add(u);
    }

    [RelayCommand]
    private async Task AddUser()
    {
        await userService.AddAsync(Login, Email);
        await LoadUsersAsync();
    }

    [RelayCommand] private async Task DeleteUser()
    {
        await userService.DeleteAsync(SelectedUser);
        await LoadUsersAsync();
    }
}
