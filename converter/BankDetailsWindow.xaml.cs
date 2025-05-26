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
    public partial class BankDetailsWindow : Window, INotifyPropertyChanged
    {
        private readonly string _selectedCurrencyCode;
        private readonly List<ExchangeRate> _allRates;

        public BankDetailsWindow(string currencyCode, List<ExchangeRate> allRates)
        {
            InitializeComponent();
            DataContext = this;

            _selectedCurrencyCode = currencyCode;
            _allRates = allRates;

            BackCommand = new RelayCommand(_ => GoBack());
            BankAddresses = CurrencyParser.GetBankAddresses();
            LoadCurrencyData();
        }

        private string _currencyName;
        public string CurrencyName
        {
            get => _currencyName;
            set
            {
                _currencyName = value;
                OnPropertyChanged(nameof(CurrencyName));
            }
        }

        private List<BankAddress> _bankAddresses;
        public List<BankAddress> BankAddresses
        {
            get => _bankAddresses;
            set
            {
                _bankAddresses = value;
                OnPropertyChanged(nameof(BankAddresses));
            }
        }

        private List<BankWithRates> _banksWithRates;
        public List<BankWithRates> BanksWithRates
        {
            get => _banksWithRates;
            set
            {
                _banksWithRates = value;
                OnPropertyChanged(nameof(BanksWithRates));
            }
        }

        public ICommand BackCommand { get; }

        private void LoadCurrencyData()
        {
            var firstBankWithCurrency = _allRates
                .FirstOrDefault(r => r.CurrencyRates.ContainsKey(_selectedCurrencyCode));

            CurrencyName = firstBankWithCurrency?.CurrencyRates[_selectedCurrencyCode].CurrencyName
                ?? _selectedCurrencyCode;

            var banks = _allRates
                .Where(r => r.CurrencyRates.ContainsKey(_selectedCurrencyCode))
                .Select(r => new BankWithRates
                {
                    BankName = r.BankName,
                    BuyRate = r.CurrencyRates[_selectedCurrencyCode].BuyRate,
                    SellRate = r.CurrencyRates[_selectedCurrencyCode].SellRate,
                    Addresses = BankAddresses
                        .FirstOrDefault(a => a.BankName == r.BankName)?
                        .Addresses ?? new List<string>()
                })
                .OrderBy(b => b.BuyRate)
                .ToList();

            BanksWithRates = banks;
        }

        private void GoBack()
        {
            new ConverterWindow().Show();
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BankWithRates
    {
        public string BankName { get; set; }
        public double BuyRate { get; set; }
        public double SellRate { get; set; }
        public List<string> Addresses { get; set; }
    }
}