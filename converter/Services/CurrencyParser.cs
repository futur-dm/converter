using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using CurrencyConverter.Models;

namespace CurrencyConverter.Services
{
    public static class CurrencyParser
    {
        private static readonly string LogPath = "currency_parser.log";
        private static readonly string ErrorLogPath = "currency_parser_errors.log";
        private static readonly string DebugLogPath = "currency_parser_debug.log";

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

                    // ЦБ не указывает отдельно покупку/продажу, используем один курс
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

                    // Сначала ищем в основной категории
                    foreach (var rate in data.payload.rates)
                    {
                        if (rate.category == "DebitCardsTransfers")
                        {
                            string fromCurrency = rate.fromCurrency.name;
                            string toCurrency = rate.toCurrency.name;

                            //if (fromCurrency == "USD" && toCurrency == "RUB")
                            //{
                            //    usdBuy = rate.buy;
                            //    LogMessage($"Найден курс покупки USD: {usdBuy}");
                            //}
                            //else if (fromCurrency == "RUB" && toCurrency == "USD")
                            //{
                            //    usdSell = rate.sell;
                            //    LogMessage($"Найден курс продажи USD: {usdSell}");
                            //}
                            //else if (fromCurrency == "EUR" && toCurrency == "RUB")
                            //{
                            //    eurBuy = rate.buy;
                            //    LogMessage($"Найден курс покупки EUR: {eurBuy}");
                            //}
                            //else if (fromCurrency == "RUB" && toCurrency == "EUR")
                            //{
                            //    eurSell = rate.sell;
                            //    LogMessage($"Найден курс продажи EUR: {eurSell}");
                            //}
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

                    // Проверяем, все ли курсы найдены
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

        public static List<BankAddress> GetBankAddresses()
        {
            return new List<BankAddress>
            {
                new BankAddress
                {
                    BankName = "Центральный Банк РФ",
                    Addresses = new List<string>
                    {
                        "г. Брянск, ул. Красноармейская, 12",
                        "г. Брянск, пр-т Ленина, 67"
                    }
                },
                new BankAddress
                {
                    BankName = "Тинькофф Банк",
                    Addresses = new List<string>
                    {
                        "г. Брянск, ул. Дуки, 58 (партнерский пункт)",
                        "г. Брянск, пр-т Станке Димитрова, 77 (партнерский пункт)",
                        "г. Брянск, ул. Красноармейская, 100 (партнерский пункт)"
                    }
                }
            };
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