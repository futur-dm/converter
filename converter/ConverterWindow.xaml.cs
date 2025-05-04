using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CurrencyConverter.Models;
using CurrencyConverter.Services;
using CurrencyConverter.Utilities;

namespace CurrencyConverter.Views
{
    public partial class ConverterWindow : Window, INotifyPropertyChanged
    {
        private ExchangeRate _bestRate;
        public ExchangeRate BestRate
        {
            get => _bestRate;
            set
            {
                _bestRate = value;
                OnPropertyChanged(nameof(BestRate));
            }
        }

        private List<ExchangeRate> _allRates;
        public List<ExchangeRate> AllRates
        {
            get => _allRates;
            set
            {
                _allRates = value;
                OnPropertyChanged(nameof(AllRates));
            }
        }

        public ConverterWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadRates();
        }

        private async void LoadRates()
        {
            try
            {
                var cbrTask = System.Threading.Tasks.Task.Run(() => CurrencyParser.GetCbrRates());
                var tinkoffTask = System.Threading.Tasks.Task.Run(() => CurrencyParser.GetTinkoffRates());

                var rates = new List<ExchangeRate>();

                var cbrRate = await cbrTask;
                if (cbrRate != null) rates.Add(cbrRate);

                var tinkoffRate = await tinkoffTask;
                if (tinkoffRate != null) rates.Add(tinkoffRate);

                if (rates.Count > 0)
                {
                    var bestBank = rates.OrderByDescending(b =>
                        (b.UsdBuy * 0.6) +
                        (b.EurBuy * 0.4) +
                        (1 / Math.Max(b.UsdSell, 0.01) * 0.6) +
                        (1 / Math.Max(b.EurSell, 0.01) * 0.4)
                    ).First();

                    BestRate = new ExchangeRate
                    {
                        BankName = bestBank.BankName + " (Лучший курс)",
                        UsdBuy = bestBank.UsdBuy,
                        UsdSell = bestBank.UsdSell,
                        EurBuy = bestBank.EurBuy,
                        EurSell = bestBank.EurSell
                    };

                    AllRates = rates;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BestRate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BestRate != null && e.ChangedButton == MouseButton.Left)
            {
                var bankDetailsWindow = new BankDetailsWindow(BestRate);
                bankDetailsWindow.Show();
                this.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (BestRate != null)
            {
                var calculatorWindow = new CurrencyCalculatorWindow(BestRate);
                calculatorWindow.Show();
            }
            else
            {
                MessageBox.Show("Сначала загрузите курсы валют", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}