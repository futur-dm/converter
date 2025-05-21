using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CurrencyConverter.Models;
using CurrencyConverter.Services;

namespace CurrencyConverter.Views
{
    public partial class ConverterWindow : Window, INotifyPropertyChanged
    {
        private List<BestCurrencyRate> _bestRates = new List<BestCurrencyRate>();
        public List<BestCurrencyRate> BestRates
        {
            get => _bestRates;
            set
            {
                if (_bestRates != value)
                {
                    _bestRates = value;
                    OnPropertyChanged(nameof(BestRates));
                }
            }
        }

        private List<ExchangeRate> _allRates;
        public List<ExchangeRate> AllRates
        {
            get => _allRates;
            set
            {
                if (_allRates != value)
                {
                    _allRates = value;
                    OnPropertyChanged(nameof(AllRates));
                }
            }
        }

        public ConverterWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += (s, e) => LoadRates();
        }

        private async void LoadRates()
        {
            try
            {
                var tasks = new List<Task<ExchangeRate>>
                {
                    Task.Run(() => CurrencyParser.GetCbrRates()),
                    Task.Run(() => CurrencyParser.GetTinkoffRates()),
                    Task.Run(() => CurrencyParser.GetRaiffeisenRates())
                };

                await Task.WhenAll(tasks);

                var rates = tasks.Select(t => t.Result).Where(r => r != null).ToList();

                if (rates.Any())
                {
                    AllRates = rates;
                    BestRates = CalculateBestRates(rates);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<BestCurrencyRate> CalculateBestRates(List<ExchangeRate> allRates)
        {
            var bestRates = new List<BestCurrencyRate>();
            var currencyCodes = allRates.SelectMany(r => r.CurrencyRates.Keys).Distinct().ToList();

            foreach (var currencyCode in currencyCodes)
            {
                var bestBuy = allRates
                    .Where(r => r.CurrencyRates.ContainsKey(currencyCode))
                    .OrderBy(r => r.CurrencyRates[currencyCode].BuyRate)
                    .FirstOrDefault();

                var bestSell = allRates
                    .Where(r => r.CurrencyRates.ContainsKey(currencyCode))
                    .OrderByDescending(r => r.CurrencyRates[currencyCode].SellRate)
                    .FirstOrDefault();

                if (bestBuy != null && bestSell != null)
                {
                    bestRates.Add(new BestCurrencyRate
                    {
                        CurrencyCode = currencyCode,
                        CurrencyName = bestBuy.CurrencyRates[currencyCode].CurrencyName,
                        BuyRate = bestBuy.CurrencyRates[currencyCode].BuyRate,
                        SellRate = bestSell.CurrencyRates[currencyCode].SellRate,
                        BuyBank = bestBuy.BankName,
                        SellBank = bestSell.BankName
                    });
                }
            }

            return bestRates;
        }

        private ExchangeRate CreateBestRatesExchangeRate(List<ExchangeRate> allRates)
        {
            var bestRates = new ExchangeRate
            {
                BankName = "Лучшие курсы всех банков",
                CurrencyRates = new Dictionary<string, CurrencyRate>()
            };

            foreach (var rate in CalculateBestRates(allRates))
            {
                bestRates.CurrencyRates[rate.CurrencyCode] = new CurrencyRate
                {
                    CurrencyCode = rate.CurrencyCode,
                    CurrencyName = rate.CurrencyName,
                    BuyRate = rate.BuyRate,
                    SellRate = rate.SellRate,
                    BuyBank = rate.BuyBank,
                    SellBank = rate.SellBank
                };
            }

            return bestRates;
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllRates == null || !AllRates.Any())
            {
                MessageBox.Show("Сначала загрузите курсы валют", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var bestRates = CreateBestRatesExchangeRate(AllRates);
            new CurrencyCalculatorWindow(bestRates).Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BestCurrencyRate
    {
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public double BuyRate { get; set; }
        public double SellRate { get; set; }
        public string BuyBank { get; set; }
        public string SellBank { get; set; }
    }
}