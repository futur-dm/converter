using System;
using System.Windows;
using System.Windows.Controls;
using CurrencyConverter.Models;

namespace CurrencyConverter.Views
{
    public partial class CurrencyCalculatorWindow : Window
    {
        private readonly ExchangeRate _exchangeRate;

        public CurrencyCalculatorWindow(ExchangeRate exchangeRate)
        {
            InitializeComponent();
            _exchangeRate = exchangeRate;
            InitializeCurrencies();
            UpdateRateInfo();
        }

        private void InitializeCurrencies()
        {
            foreach (var currency in _exchangeRate.CurrencyRates.Keys)
            {
                FromCurrencyCombo.Items.Add(new ComboBoxItem { Content = currency });
                ToCurrencyCombo.Items.Add(new ComboBoxItem { Content = currency });
            }

            FromCurrencyCombo.Items.Add(new ComboBoxItem { Content = "RUB" });
            ToCurrencyCombo.Items.Add(new ComboBoxItem { Content = "RUB" });

            FromCurrencyCombo.SelectedIndex = 0;
            ToCurrencyCombo.SelectedIndex = 1;
        }

        private void UpdateRateInfo()
        {
            var rateInfo = $"Курсы {_exchangeRate.BankName}:\n";

            foreach (var currency in _exchangeRate.CurrencyRates.Values)
            {
                rateInfo += $"{currency.CurrencyCode}: покупка {currency.BuyRate:N2}, продажа {currency.SellRate:N2}\n";
            }

            RateInfoText.Text = rateInfo;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(AmountTextBox.Text, out double amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fromCurrency = ((ComboBoxItem)FromCurrencyCombo.SelectedItem).Content.ToString();
                string toCurrency = ((ComboBoxItem)ToCurrencyCombo.SelectedItem).Content.ToString();

                double result = _exchangeRate.Convert(fromCurrency, toCurrency, amount);
                ResultText.Text = $"{amount:N2} {fromCurrency} = {result:N2} {toCurrency}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}