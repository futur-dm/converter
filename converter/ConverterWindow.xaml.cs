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
        private ExchangeRate _bestRate;
        public ExchangeRate BestRate
        {
            get => _bestRate;
            set
            {
                if (_bestRate != value)
                {
                    _bestRate = value;
                    OnPropertyChanged(nameof(BestRate));
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
                    BestRate = rates.OrderByDescending(b => b.GetScore()).First();
                    BestRate.BankName += " (Лучший курс)";
                    AllRates = rates;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик события MouseDown (переименован для соответствия XAML)
        private void BestRateBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && BestRate != null)
            {
                new BankDetailsWindow(BestRate).Show();
                Close();
            }
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (BestRate == null)
            {
                MessageBox.Show("Сначала загрузите курсы валют", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            new CurrencyCalculatorWindow(BestRate).Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}