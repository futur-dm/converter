using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using CurrencyConverter.Models;
using HtmlAgilityPack;

namespace CurrencyConverter.Services
{
    public static class CurrencyParser
    {
        private static readonly string LogPath = "currency_parser.log";
        private static readonly string ErrorLogPath = "currency_parser_errors.log";
        private static readonly string DebugLogPath = "currency_parser_debug.log";

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
                    BankName = "Райфайзен Банк",
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
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);

                    double usdRate = Convert.ToDouble(xmlDoc.SelectSingleNode("//Valute[CharCode='USD']/Value").InnerText);
                    double eurRate = Convert.ToDouble(xmlDoc.SelectSingleNode("//Valute[CharCode='EUR']/Value").InnerText);

                    return new ExchangeRate
                    {
                        BankName = "Центральный Банк РФ",
                        UsdBuy = usdRate,
                        UsdSell = usdRate,
                        EurBuy = eurRate,
                        EurSell = eurRate
                    };
                }
            }
            catch
            {
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
                    LogMessage("Успешно получены данные от Тинькофф");
                    LogDebugData("Tinkoff JSON", json);

                    dynamic data = JsonConvert.DeserializeObject(json);
                    double usdBuy = 0, usdSell = 0, eurBuy = 0, eurSell = 0;
                    bool foundInPrimaryCategory = false;

                    foreach (var rate in data.payload.rates)
                    {
                        if (rate.category == "DebitCardsTransfers")
                        {
                            string fromCurrency = rate.fromCurrency.name;
                            string toCurrency = rate.toCurrency.name;
                            if (rate.fromCurrency.name == "USD" && rate.toCurrency.name == "RUB")
                            {
                                usdBuy = rate.buy;
                                usdSell = rate.sell;
                            }
                            else if (rate.fromCurrency.name == "EUR" && rate.toCurrency.name == "RUB")
                            {
                                eurBuy = rate.buy;
                                eurSell = rate.sell;
                            }
                        }
                    }

                    foundInPrimaryCategory = usdBuy > 0 && usdSell > 0 && eurBuy > 0 && eurSell > 0;

                    if (!foundInPrimaryCategory)
                    {
                        throw new Exception("Не удалось получить все необходимые курсы валют");
                    }

                    var result = new ExchangeRate
                    {
                        BankName = "Тинькофф Банк",
                        UsdBuy = usdBuy,
                        UsdSell = usdSell,
                        EurBuy = eurBuy,
                        EurSell = eurSell,
                    };

                    LogMessage($"Финальные курсы Тинькофф: USD {result.UsdBuy}/{result.UsdSell}, EUR {result.EurBuy}/{result.EurSell}");
                    return result;
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
                    string json = webClient.DownloadString("https://www.raiffeisen.ru/oapi/currency_rate/get/?source=CASH&currencies=EUR,USD");
                    LogMessage("Успешно получены данные от Райффайзен");
                    LogDebugData("Raiffeisen JSON", json);

                    dynamic data = JsonConvert.DeserializeObject(json);
                    double usdBuy = 0, usdSell = 0, eurBuy = 0, eurSell = 0;
                    bool allRatesFound = false;

                    var rubRates = data.data.rates[0];

                    foreach (var exchange in rubRates.exchange)
                    {
                        string currency = exchange.code;
                        if (currency == "USD")
                        {
                            usdBuy = exchange.rates.buy.value;
                            usdSell = exchange.rates.sell.value;
                        }
                        else if (currency == "EUR")
                        {
                            eurBuy = exchange.rates.buy.value;
                            eurSell = exchange.rates.sell.value;
                        }

                    }

                    allRatesFound = usdBuy > 0 && usdSell > 0 && eurBuy > 0 && eurSell > 0;

                    if (!allRatesFound)
                    {
                        throw new Exception("Не удалось получить все необходимые курсы валют");
                    }

                    var result = new ExchangeRate
                    {
                        BankName = "Райффайзен Банк",
                        UsdBuy = usdBuy,
                        UsdSell = usdSell,
                        EurBuy = eurBuy,
                        EurSell = eurSell,
                    };

                    LogMessage($"Финальные курсы Райффайзен: USD {result.UsdBuy}/{result.UsdSell}, EUR {result.EurBuy}/{result.EurSell}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogError("Ошибка при получении курсов Райффайзен", ex);
                return null;
            }
        }

        private static double ParseCurrencyValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Trim().Replace(" ", "").Replace(",", ".");

            if (double.TryParse(value, out double result))
                return result;

            return 0;
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