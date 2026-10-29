using System;
using System.Globalization;

namespace AvaloniaApplication29.Services
{
    public class OperationRow
    {
        public string Description { get; }
        public string DateText { get; }
        public string AmountText { get; }
        public bool IsIncome { get; }
        public string Arrow { get; }

        public OperationRow(string description, DateTime createdAtUtc, decimal signedAmount)
        {
            Description = description;
            DateText = createdAtUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
            IsIncome = signedAmount > 0;
            Arrow = IsIncome ? "↓" : "↑";

            var sign = signedAmount > 0 ? "+" : "";
            AmountText = sign + signedAmount.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}
