using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaApplication29.Models
{
    public class Operation
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Kind { get; set; } = "";
        public decimal SignedAmount { get; set; }

        public string Description { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }
}
