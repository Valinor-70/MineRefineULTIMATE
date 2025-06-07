using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MineRefine.Models;
using MineRefine.Services;

namespace MineRefine.Views
{
    public sealed partial class SellQuantityDialog : ContentDialog
    {
        public int QuantityToSell { get; private set; }

        private readonly Mineral _mineral;
        private readonly long _maxQuantity;
        private readonly MarketService _marketService;
        private readonly long _unitPrice;

        public SellQuantityDialog(Mineral mineral, long maxQuantity, MarketService marketService)
        {
            this.InitializeComponent();

            _mineral = mineral;
            _maxQuantity = maxQuantity;
            _marketService = marketService;

            var marketData = _marketService.GetMineralMarketData(mineral.Id);
            _unitPrice = _marketService.CalculateSellValue(mineral.Id, mineral.Value, 1);

            SetupDialog();
            UpdateCalculations();

            this.PrimaryButtonClick += SellQuantityDialog_PrimaryButtonClick;
        }

        private void SetupDialog()
        {
            this.Title = $"💰 Sell {_mineral.Name}";
            MineralNameText.Text = $"{_mineral.Icon} {_mineral.Name}";
            AvailableQuantityText.Text = $"Available: {_maxQuantity:N0}";
            UnitPriceText.Text = FormatMoney(_unitPrice);

            QuantityTextBox.TextChanged += (s, e) => UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            if (int.TryParse(QuantityTextBox.Text, out var quantity))
            {
                quantity = Math.Max(0, Math.Min((int)_maxQuantity, quantity));
                var totalValue = _marketService.CalculateSellValue(_mineral.Id, _mineral.Value, quantity);
                TotalValueText.Text = FormatMoney(totalValue);

                var marketData = _marketService.GetMineralMarketData(_mineral.Id);
                MarketInfoText.Text = $"Market Impact: Selling {quantity} units will slightly increase supply. " +
                                     $"Current trend: {marketData?.Trend ?? "Stable"}";

                this.IsPrimaryButtonEnabled = quantity > 0;
            }
            else
            {
                TotalValueText.Text = "£0";
                this.IsPrimaryButtonEnabled = false;
            }
        }

        private void HalfButton_Click(object sender, RoutedEventArgs e)
        {
            QuantityTextBox.Text = ((int)_maxQuantity / 2).ToString();
        }

        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            QuantityTextBox.Text = _maxQuantity.ToString();
        }

        private void SellQuantityDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (int.TryParse(QuantityTextBox.Text, out var quantity) && quantity > 0 && quantity <= _maxQuantity)
            {
                QuantityToSell = quantity;
            }
            else
            {
                args.Cancel = true;
                // Show error message
                _ = ShowErrorAsync($"Please enter a valid quantity between 1 and {_maxQuantity}.");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorAsync(string message)
        {
            var errorDialog = new ContentDialog()
            {
                Title = "Invalid Quantity",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        private static string FormatMoney(long amount)
        {
            return amount switch
            {
                >= 1000000000 => $"£{amount / 1000000000.0:F1}B",
                >= 1000000 => $"£{amount / 1000000.0:F1}M",
                >= 1000 => $"£{amount / 1000.0:F1}K",
                _ => $"£{amount:N0}"
            };
        }
    }
}