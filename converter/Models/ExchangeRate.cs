using System;

namespace CurrencyConverter.Models
{
    public class ExchangeRate
    {
        public string BankName { get; set; }
        public double UsdBuy { get; set; }
        public double UsdSell { get; set; }
        public double EurBuy { get; set; }
        public double EurSell { get; set; }

        public double GetScore()
        {
            const double usdWeight = 0.6;
            const double eurWeight = 0.4;

            return (UsdBuy * usdWeight) +
                   (EurBuy * eurWeight) +
                   (1 / Math.Max(UsdSell, 0.01) * usdWeight) + // Защита от деления на 0
                   (1 / Math.Max(EurSell, 0.01) * eurWeight);
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            if (fromCurrency == toCurrency) return amount;

            if (fromCurrency == "RUB" && toCurrency == "USD") return amount / UsdSell;
            if (fromCurrency == "RUB" && toCurrency == "EUR") return amount / EurSell;
            if (fromCurrency == "USD" && toCurrency == "RUB") return amount * UsdBuy;
            if (fromCurrency == "EUR" && toCurrency == "RUB") return amount * EurBuy;

            // Конвертация через RUB (USD->EUR или EUR->USD)
            if (fromCurrency == "USD" && toCurrency == "EUR")
                return (amount * UsdBuy) / EurSell;
            if (fromCurrency == "EUR" && toCurrency == "USD")
                return (amount * EurBuy) / UsdSell;

            throw new ArgumentException("Неподдерживаемые валюты");
        }
    }
}