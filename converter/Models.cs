// Класс для хранения курсов валют
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System;
using Newtonsoft.Json;

public class ExchangeRate
{
    public string BankName { get; set; }
    public double UsdBuy { get; set; }
    public double UsdSell { get; set; }
    public double EurBuy { get; set; }
    public double EurSell { get; set; }
}

// Класс для хранения адресов банков
public class BankAddress
{
    public string BankName { get; set; }
    public List<string> Addresses { get; set; }
}

// Класс для парсинга курсов валют
public static class CurrencyParser
{
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
                dynamic data = JsonConvert.DeserializeObject(json);

                double usdBuy = 0, usdSell = 0, eurBuy = 0, eurSell = 0;

                foreach (var rate in data.payload.rates)
                {
                    if (rate.category == "DepositPayments")
                    {
                        if (rate.fromCurrency.name == "USD" && rate.toCurrency.name == "RUB")
                            usdBuy = rate.buy;
                        else if (rate.fromCurrency.name == "RUB" && rate.toCurrency.name == "USD")
                            usdSell = rate.sell;
                        else if (rate.fromCurrency.name == "EUR" && rate.toCurrency.name == "RUB")
                            eurBuy = rate.buy;
                        else if (rate.fromCurrency.name == "RUB" && rate.toCurrency.name == "EUR")
                            eurSell = rate.sell;
                    }
                }

                return new ExchangeRate
                {
                    BankName = "Тинькофф Банк",
                    UsdBuy = usdBuy,
                    UsdSell = usdSell,
                    EurBuy = eurBuy,
                    EurSell = eurSell
                };
            }
        }
        catch
        {
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
}