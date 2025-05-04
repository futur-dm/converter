using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CurrencyConverter.Models;

namespace CurrencyConverter.Views
{
    public partial class CurrencyCalculatorWindow : Window
    {
        private readonly ExchangeRate _bestRate;

        public CurrencyCalculatorWindow(ExchangeRate bestRate)
        {
            InitializeComponent();
            _bestRate = bestRate;
            UpdateRateInfo();
        }

        private void UpdateRateInfo()
        {
            RateInfoText.Text = $"Курсы {_bestRate.BankName}:\n" +
                              $"USD: покупка {_bestRate.UsdBuy:N2}, продажа {_bestRate.UsdSell:N2}\n" +
                              $"EUR: покупка {_bestRate.EurBuy:N2}, продажа {_bestRate.EurSell:N2}";
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

                double result = ConvertCurrency(amount, fromCurrency, toCurrency);
                ResultText.Text = $"{amount:N2} {fromCurrency} = {result:N2} {toCurrency}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double ConvertCurrency(double amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == "RUB" && toCurrency == "USD")
                return amount / _bestRate.UsdSell;

            if (fromCurrency == "RUB" && toCurrency == "EUR")
                return amount / _bestRate.EurSell;

            if (fromCurrency == "USD" && toCurrency == "RUB")
                return amount * _bestRate.UsdBuy;

            if (fromCurrency == "EUR" && toCurrency == "RUB")
                return amount * _bestRate.EurBuy;

            if (fromCurrency == "USD" && toCurrency == "EUR")
                return (amount * _bestRate.UsdBuy) / _bestRate.EurSell;

            if (fromCurrency == "EUR" && toCurrency == "USD")
                return (amount * _bestRate.EurBuy) / _bestRate.UsdSell;

            throw new ArgumentException("Неподдерживаемая валютная пара");
        }
    }
}