using AvaloniaApplication29.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace AvaloniaApplication29.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int CurrentAccountId = 1;

    private readonly BankingService _bankingService;
    private readonly Task _initialization;

    public ObservableCollection<OperationRow> History { get; } = new();

    [ObservableProperty]
    private string balanceText = "Загрузка…";

    [ObservableProperty]
    private string purchaseTitle = "";

    [ObservableProperty]
    private string purchaseAmountText = "";

    [ObservableProperty]
    private string recipientIdText = "2";

    [ObservableProperty]
    private string transferAmountText = "";

    [ObservableProperty]
    private string statusMessage = "";

    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

    partial void OnStatusMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasStatus));
    }

    public MainViewModel() : this(new BankingService())
    {
    }

    public MainViewModel(BankingService bankingService)
    {
        _bankingService = bankingService;
        _initialization = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _bankingService.InitializeAsync();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task BuyAsync()
    {
        await _initialization;

        if (!TryReadAmount(PurchaseAmountText, out var amount))
        {
            StatusMessage = "Введите корректную сумму покупки.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PurchaseTitle))
        {
            StatusMessage = "Введите название покупки.";
            return;
        }

        await RunOperationAsync(async () =>
        {
            await _bankingService.PurchaseAsync(
                CurrentAccountId,
                PurchaseTitle.Trim(),
                amount);

            PurchaseTitle = "";
            PurchaseAmountText = "";
        });
    }

    [RelayCommand]
    private async Task TransferAsync()
    {
        await _initialization;

        if (!int.TryParse(
                RecipientIdText?.Trim(),
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out var recipientId))
        {
            StatusMessage = "Введите номер счета получателя.";
            return;
        }

        if (!TryReadAmount(TransferAmountText, out var amount))
        {
            StatusMessage = "Введите корректную сумму перевода.";
            return;
        }

        await RunOperationAsync(async () =>
        {
            await _bankingService.TransferAsync(
                CurrentAccountId,
                recipientId,
                amount);

            TransferAmountText = "";
        });
    }

    private async Task RunOperationAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            await RefreshAsync();
            StatusMessage = "Операция выполнена.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task RefreshAsync()
    {
        var account = await _bankingService.GetAccountAsync(CurrentAccountId);
        var operations = await _bankingService.GetHistoryAsync(CurrentAccountId);

        BalanceText = account.Balance.ToString("C", CultureInfo.CurrentCulture);

        History.Clear();

        foreach (var operation in operations)
        {
            History.Add(new OperationRow(
                operation.Description,
                operation.CreatedAtUtc,
                operation.SignedAmount));
        }
    }

    private static bool TryReadAmount(string text, out decimal amount)
    {
        amount = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text
            .Trim()
            .Replace(" ", "")
            .Replace("\u00A0", "")
            .Replace("\u202F", "");

        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
            || decimal.TryParse(normalized.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            return amount > 0;
        }

        return false;
    }
}
