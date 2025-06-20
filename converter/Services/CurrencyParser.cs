using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using CurrencyConverter.Models;
using System.Linq;

namespace CurrencyConverter.Services
{
    public static class CurrencyParser
    {
        private static readonly string LogPath = "currency_parser.log";
        private static readonly string ErrorLogPath = "currency_parser_errors.log";
        private static readonly string DebugLogPath = "currency_parser_debug.log";

        private static readonly Dictionary<string, string> SupportedCurrencies = new Dictionary<string, string>
        {
            {"USD", "Доллар США"},
            {"EUR", "Евро"},
            {"GBP", "Фунт стерлингов"},
            {"CNY", "Китайский юань"},
            {"JPY", "Японская иена"}
        };

        public static List<BankAddress> GetBankAddresses()
        {
            return new List<BankAddress>
            {
                new BankAddress
                {
                    BankName = "Центральный Банк РФ",
                    Addresses = new List<string>
                    {
                        "г. Брянск, улица Горького, 34",
                    }
                },
                new BankAddress
                {
                    BankName = "Тинькофф Банк",
                    Addresses = new List<string>
                    {
                        "г. Брянск, просп. Ленина, 6А/1",
                        "г. Брянск, просп. Ленина, 61",
                        "г. Брянск, 2-я ул. Мичурина, 42",
                        "г. Брянск, Красноармейская ул., 100",
                        "г. Брянск, ул. Крахмалёва, 6",
                        "г. Брянск, Бежицкий район, улица 22-го съезда КПСС, 29",
                        "г. Брянск, 2-я ул. Мичурина, 42",
                        "г. Брянск, 2-я ул. Мичурина, 42/1",
                        "г. Брянск, просп. Станке Димитрова, 108Б",
                        "г. Брянск, ул. 3-го Интернационала, 8",
                        "г. Брянск, ул. Димитрова, 29А",
                        "г. Брянск, Авиационная ул., 7А",
                        "г. Брянск, ул. Брянского Фронта, 2",
                        "г. Брянск, Объездная ул., 30",
                        "г. Брянск, ул. Ульянова, 3",
                        "г. Брянск, Литейная ул., 80А",
                        "г. Брянск, ул. Дуки, 63",
                        "г. Брянск, ул. Бурова, 12А",
                        "г. Брянск, ул. Ульянова, 92",
                        "г. Брянск, ул. Брянского Фронта, 2",
                        "г. Брянск, Вокзальная ул., 120",
                        "г. Брянск, Литейная ул., 3А",
                        "г. Брянск, ул. Бурова, 12А",
                        "г. Брянск, ул. Димитрова, 84",
                        "г. Брянск, ул. Ульянова, 58А",
                        "г. Брянск, ул. Горбатова, 18",
                        "г. Брянск, Красноармейская ул., 100",
                        "г. Брянск, ул. Ульянова, 3",

                    }
                },
                new BankAddress
                {
                    BankName = "Райффайзен Банк",
                    Addresses = new List<string>
                    {
                        "г. Брянск, Красноармейская улица, 65",
                    }
                }
            };
        }

        public static ExchangeRate GetCbrRates()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string xml = webClient.DownloadString("https://www.cbr.ru/scripts/XML_daily.asp");
                    LogDebugData("CBR XML", xml);

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);

                    var rates = new ExchangeRate { BankName = "Центральный Банк РФ" };

                    foreach (var currency in SupportedCurrencies)
                    {
                        var node = xmlDoc.SelectSingleNode($"//Valute[CharCode='{currency.Key}']/Value");
                        if (node != null)
                        {
                            double rate = Convert.ToDouble(node.InnerText);
                            rates.AddOrUpdateRate(currency.Key, currency.Value, rate, rate);
                        }
                    }

                    LogMessage($"Успешно получены курсы ЦБ: {string.Join(", ", rates.CurrencyRates.Keys)}");
                    return rates;
                }
            }
            catch (Exception ex)
            {
                LogError("Ошибка при получении курсов ЦБ", ex);
                return null;
            }
        }

        public static ExchangeRate GetTinkoffRates()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string json = webClient.DownloadString("https://api.tinkoff.ru/v1/currency_rates");
                    LogDebugData("Tinkoff JSON", json);

                    dynamic data = JsonConvert.DeserializeObject(json);
                    var rates = new ExchangeRate { BankName = "Тинькофф Банк" };

                    foreach (var rate in data.payload.rates)
                    {
                        if (rate.category == "DebitCardsTransfers")
                        {
                            string fromCurrency = rate.fromCurrency.name.ToString();
                            string toCurrency = rate.toCurrency.name.ToString();

                            if (toCurrency == "RUB" && SupportedCurrencies.ContainsKey(fromCurrency))
                            {
                                rates.AddOrUpdateRate(
                                    fromCurrency,
                                    SupportedCurrencies[fromCurrency],
                                    (double)rate.buy,
                                    (double)rate.sell
                                );
                            }
                        }
                    }

                    LogMessage($"Успешно получены курсы Тинькофф: {string.Join(", ", rates.CurrencyRates.Keys)}");
                    return rates;
                }
            }
            catch (Exception ex)
            {
                LogError("Ошибка при получении курсов Тинькофф", ex);
                return null;
            }
        }

        public static ExchangeRate GetRaiffeisenRates()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string json = webClient.DownloadString(
                        $"https://www.raiffeisen.ru/oapi/currency_rate/get/?source=CASH&currencies={string.Join(",", SupportedCurrencies.Keys)}");

                    LogDebugData("Raiffeisen JSON", json);

                    dynamic data = JsonConvert.DeserializeObject(json);
                    var rates = new ExchangeRate { BankName = "Райффайзен Банк" };

                    var rubRates = data.data.rates[0];
                    foreach (var exchange in rubRates.exchange)
                    {
                        LogMessage($" exchange {exchange}");
                        string currency = exchange.code.ToString();
                        if (SupportedCurrencies.ContainsKey(currency))
                        {
                            double multiplier = exchange.rates.buy.multiplier != null
                                ? (double)exchange.rates.buy.multiplier
                                : 1.0;

                            double buyRate = (double)exchange.rates.buy.value / multiplier;
                            double sellRate = (double)exchange.rates.sell.value / multiplier;

                            rates.AddOrUpdateRate(
                                currency,
                                SupportedCurrencies[currency],
                                buyRate,
                                sellRate
                            );
                        }
                    }

                    LogMessage($"Успешно получены курсы Райффайзен: {string.Join(", ", rates.CurrencyRates.Keys)}");
                    return rates;
                }
            }
            catch (Exception ex)
            {
                LogError("Ошибка при получении курсов Райффайзен", ex);
                return null;
            }
        }

        #region Логирование
        private static void LogMessage(string message)
        {
            string logEntry = $"[{DateTime.Now}] {message}\n";
            File.AppendAllText(LogPath, logEntry);
            Trace.WriteLine(logEntry);
        }

        private static void LogError(string context, Exception ex)
        {
            string errorEntry = $"[{DateTime.Now}] ERROR in {context}: {ex.Message}\n{ex.StackTrace}\n\n";
            File.AppendAllText(ErrorLogPath, errorEntry);
            Trace.TraceError(errorEntry);
        }

        private static void LogDebugData(string title, string data)
        {
            string debugEntry = $"[{DateTime.Now}] {title}:\n{data}\n\n";
            File.AppendAllText(DebugLogPath, debugEntry);
        }
        #endregion
    }
}