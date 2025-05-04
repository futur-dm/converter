using System;
using System.Windows;
using System.Windows.Controls;
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
            if (fromCurrency == toCurrency)
                return amount;

            // Конвертация в RUB
            if (fromCurrency == "RUB")
            {
                return toCurrency switch
                {
                    "USD" => amount / _bestRate.UsdSell,
                    "EUR" => amount / _bestRate.EurSell,
                    _ => throw new Exception("Неподдерживаемая валюта")
                };
            }
            // Конвертация из RUB
            else if (toCurrency == "RUB")
            {
                return fromCurrency switch
                {
                    "USD" => amount * _bestRate.UsdBuy,
                    "EUR" => amount * _bestRate.EurBuy,
                    _ => throw new Exception("Неподдерживаемая валюта")
                };
            }
            // Конвертация между USD и EUR через RUB
            else
            {
                double rubAmount = ConvertCurrency(amount, fromCurrency, "RUB");
                return ConvertCurrency(rubAmount, "RUB", toCurrency);
            }
        }
    }
}