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
    private string depositAmountText = "";

    [ObservableProperty]
    private string recipientIdText = "2";

    [ObservableProperty]
    private string transferAmountText = "";

    [ObservableProperty]
    private string statusMessage = "";

    [ObservableProperty]
    private bool isReceiptVisible;

    [ObservableProperty]
    private string receiptTitle = "";

    [ObservableProperty]
    private string receiptAmount = "";

    [ObservableProperty]
    private string receiptDescription = "";

    [ObservableProperty]
    private string receiptAccount = "";

    [ObservableProperty]
    private string receiptDate = "";

    [ObservableProperty]
    private string receiptNumber = "";

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

        var title = PurchaseTitle.Trim();

        await RunOperationAsync(
            async () =>
            {
                await _bankingService.PurchaseAsync(
                    CurrentAccountId,
                    title,
                    amount);

                PurchaseTitle = "";
                PurchaseAmountText = "";
            },
            () => ShowReceipt(
                "Чек об оплате",
                -amount,
                $"Покупка: {title}",
                "Демо-счёт № 1"));
    }

    [RelayCommand]
    private async Task DepositAsync()
    {
        await _initialization;

        if (!TryReadAmount(DepositAmountText, out var amount))
        {
            StatusMessage = "Введите корректную сумму пополнения.";
            return;
        }

        await RunOperationAsync(
            async () =>
            {
                await _bankingService.DepositAsync(CurrentAccountId, amount);
                DepositAmountText = "";
            },
            () => ShowReceipt(
                "Чек о пополнении",
                amount,
                "Пополнение баланса",
                "Демо-счёт № 1"));
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

        await RunOperationAsync(
            async () =>
            {
                await _bankingService.TransferAsync(
                    CurrentAccountId,
                    recipientId,
                    amount);

                TransferAmountText = "";
            },
            () => ShowReceipt(
                "Чек о переводе",
                -amount,
                $"Перевод на счёт № {recipientId}",
                "Демо-счёт № 1"));
    }

    [RelayCommand]
    private void CloseReceipt()
    {
        IsReceiptVisible = false;
    }

    private void ShowReceipt(
        string title,
        decimal signedAmount,
        string description,
        string account)
    {
        var now = DateTime.Now;

        ReceiptTitle = title;
        ReceiptAmount = FormatSignedAmount(signedAmount);
        ReceiptDescription = description;
        ReceiptAccount = account;
        ReceiptDate = now.ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture);
        ReceiptNumber = now.ToString("yyMMdd-HHmmss", CultureInfo.InvariantCulture);
        IsReceiptVisible = true;
    }

    private static string FormatSignedAmount(decimal signedAmount)
    {
        var sign = signedAmount > 0 ? "+" : "";
        return sign + signedAmount.ToString("C", CultureInfo.CurrentCulture);
    }

    private async Task RunOperationAsync(Func<Task> operation, Action? onSuccess = null)
    {
        try
        {
            await operation();
            onSuccess?.Invoke();
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
