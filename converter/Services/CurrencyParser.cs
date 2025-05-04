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
                    LogMessage("Успешно получены данные от ЦБ РФ");
                    LogDebugData("ЦБ РФ XML", xml);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);

                    // Парсинг USD
                    var usdNode = xmlDoc.SelectSingleNode("//Valute[CharCode='USD']");
                    int usdNominal = int.Parse(usdNode.SelectSingleNode("Nominal").InnerText);
                    double usdValue = Convert.ToDouble(usdNode.SelectSingleNode("Value").InnerText.Replace(",", "."));
                    double usdRate = usdValue / usdNominal;

                    // Парсинг EUR
                    var eurNode = xmlDoc.SelectSingleNode("//Valute[CharCode='EUR']");
                    int eurNominal = int.Parse(eurNode.SelectSingleNode("Nominal").InnerText);
                    double eurValue = Convert.ToDouble(eurNode.SelectSingleNode("Value").InnerText.Replace(",", "."));
                    double eurRate = eurValue / eurNominal;

                    var result = new ExchangeRate
                    {
                        BankName = "Центральный Банк РФ",
                        UsdBuy = Math.Round(usdRate, 2),
                        UsdSell = Math.Round(usdRate * 1.02, 2), // +2% для продажи
                        EurBuy = Math.Round(eurRate, 2),
                        EurSell = Math.Round(eurRate * 1.02, 2)  // +2% для продажи
                    };

                    LogMessage($"Курсы ЦБ: USD {result.UsdBuy}/{result.UsdSell}, EUR {result.EurBuy}/{result.EurSell}");
                    return result;
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

                            if (fromCurrency == "USD" && toCurrency == "RUB")
                            {
                                usdBuy = rate.buy;
                                LogMessage($"Найден курс покупки USD: {usdBuy}");
                            }
                            else if (fromCurrency == "RUB" && toCurrency == "USD")
                            {
                                usdSell = rate.sell;
                                LogMessage($"Найден курс продажи USD: {usdSell}");
                            }
                            else if (fromCurrency == "EUR" && toCurrency == "RUB")
                            {
                                eurBuy = rate.buy;
                                LogMessage($"Найден курс покупки EUR: {eurBuy}");
                            }
                            else if (fromCurrency == "RUB" && toCurrency == "EUR")
                            {
                                eurSell = rate.sell;
                                LogMessage($"Найден курс продажи EUR: {eurSell}");
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
                        UsdBuy = Math.Round(usdBuy, 2),
                        UsdSell = Math.Round(1 / usdSell, 2), // Инвертируем курс продажи
                        EurBuy = Math.Round(eurBuy, 2),
                        EurSell = Math.Round(1 / eurSell, 2)  // Инвертируем курс продажи
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