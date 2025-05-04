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
        public BankDetailsWindow(ExchangeRate bestRate)
        {
            InitializeComponent();
            DataContext = this;

            BackCommand = new RelayCommand(_ => GoBack());

            BestRate = bestRate;
            BankAddresses = CurrencyParser.GetBankAddresses();
        }

        private ExchangeRate _bestRate;
        public ExchangeRate BestRate
        {
            get => _bestRate;
            set
            {
                _bestRate = value;
                OnPropertyChanged(nameof(BestRate));
                FilterAddresses();
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
                FilterAddresses();
            }
        }

        private List<BankAddress> _filteredAddresses;
        public List<BankAddress> FilteredAddresses
        {
            get => _filteredAddresses;
            set
            {
                _filteredAddresses = value;
                OnPropertyChanged(nameof(FilteredAddresses));
            }
        }

        public ICommand BackCommand { get; }

        private void FilterAddresses()
        {
            if (BestRate == null || BankAddresses == null)
                return;

            string bestBankName = BestRate.BankName.Replace(" (Лучший курс)", "");
            FilteredAddresses = BankAddresses
                .Where(a => a.BankName == bestBankName)
                .ToList();
        }

        private void GoBack()
        {
            var converterWindow = new ConverterWindow();
            converterWindow.Show();
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}