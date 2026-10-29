using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaApplication29.Services;
using AvaloniaApplication29.ViewModels;
using AvaloniaApplication29.Views;

namespace AvaloniaApplication29;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new BankingService()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
