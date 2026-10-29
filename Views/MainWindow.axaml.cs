using Avalonia.Controls;
using AvaloniaApplication29.ViewModels;
using Avalonia.Controls;
using AvaloniaApplication29.Services;

namespace AvaloniaApplication29.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new BankingService());
    }
}