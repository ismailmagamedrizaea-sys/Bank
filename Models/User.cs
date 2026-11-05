namespace AvaloniaApplication29.Models;

public class User
{
    public int Id { get; set; }
    public string Owner { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public decimal Balance { get; set; }
}