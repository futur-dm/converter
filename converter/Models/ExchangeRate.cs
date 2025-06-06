﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Models
{
    public class ExchangeRate
    {
        public string BankName { get; set; }
        public Dictionary<string, CurrencyRate> CurrencyRates { get; set; } = new Dictionary<string, CurrencyRate>();

        // Веса валют для расчета рейтинга
        private static readonly Dictionary<string, double> CurrencyWeights = new Dictionary<string, double>
        {
            {"USD", 0.6},
            {"EUR", 0.4},
            {"GBP", 0.3},
            {"CNY", 0.2},
            {"JPY", 0.1}
        };

        public void AddOrUpdateRate(string currencyCode, string currencyName, double buyRate, double sellRate, string buyBank = null, string sellBank = null)
        {
            if (CurrencyRates.ContainsKey(currencyCode))
            {
                CurrencyRates[currencyCode].BuyRate = buyRate;
                CurrencyRates[currencyCode].SellRate = sellRate;
                if (buyBank != null) CurrencyRates[currencyCode].BuyBank = buyBank;
                if (sellBank != null) CurrencyRates[currencyCode].SellBank = sellBank;
            }
            else
            {
                CurrencyRates[currencyCode] = new CurrencyRate
                {
                    CurrencyCode = currencyCode,
                    CurrencyName = currencyName,
                    BuyRate = buyRate,
                    SellRate = sellRate,
                    BuyBank = buyBank,
                    SellBank = sellBank
                };
            }
        }

        public double GetScore()
        {
            double score = 0;

            foreach (var currency in CurrencyRates)
            {
                if (CurrencyWeights.TryGetValue(currency.Key, out double weight))
                {
                    score += (currency.Value.BuyRate * weight) +
                            (1 / Math.Max(currency.Value.SellRate, 0.01) * weight);
                }
            }

            return score;
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            if (fromCurrency == toCurrency) return amount;

            if (fromCurrency == "RUB" || toCurrency == "RUB")
            {
                string foreignCurrency = fromCurrency == "RUB" ? toCurrency : fromCurrency;

                if (!CurrencyRates.ContainsKey(foreignCurrency))
                    throw new ArgumentException($"Банк {BankName} не поддерживает валюту {foreignCurrency}");

                if (fromCurrency == "RUB")
                    return amount / Math.Max(CurrencyRates[foreignCurrency].SellRate, 0.01);
                else
                    return amount * CurrencyRates[foreignCurrency].BuyRate;
            }

            if (!CurrencyRates.ContainsKey(fromCurrency) || !CurrencyRates.ContainsKey(toCurrency))
                throw new ArgumentException($"Банк {BankName} не поддерживает одну из валют: {fromCurrency} или {toCurrency}");

            double rubAmount = amount * CurrencyRates[fromCurrency].BuyRate;
            return rubAmount / Math.Max(CurrencyRates[toCurrency].SellRate, 0.01);
        }

        public double GetBuyRate(string currencyCode)
        {
            return CurrencyRates.TryGetValue(currencyCode, out var rate) ? rate.BuyRate : 0;
        }

        public double GetSellRate(string currencyCode)
        {
            return CurrencyRates.TryGetValue(currencyCode, out var rate) ? rate.SellRate : 0;
        }
    }

    public class CurrencyRate
    {
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public double BuyRate { get; set; }
        public double SellRate { get; set; }
        public string BuyBank { get; set; }
        public string SellBank { get; set; }
    }
}