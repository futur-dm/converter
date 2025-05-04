using System.Windows;
using CurrencyConverter.Views;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var converterWindow = new ConverterWindow();
            converterWindow.Show();
            this.Close();
        }

        private readonly ILogger<MainWindow> logger;

        public MainWindow(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MainWindow>();
            InitializeComponent();
        }
    }
}