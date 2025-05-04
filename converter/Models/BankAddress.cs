using System.Collections.Generic;

namespace CurrencyConverter.Models
{
    public class BankAddress
    {
        public string BankName { get; set; }
        public List<string> Addresses { get; set; }
    }
}