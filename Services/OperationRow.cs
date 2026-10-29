using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AvaloniaApplication29.Services
{
    public class OperationRow
    {
        public string Description { get; }
        public string DateText { get; }
        public string AmountText { get; }

        public OperationRow(string description, DateTime createdAtUtc, decimal signedAmount)
        {
            Description = description;
            DateText = createdAtUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);

            var sign = signedAmount > 0 ? "+" : "";
            AmountText = sign + signedAmount.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}
