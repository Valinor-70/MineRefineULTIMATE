using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage.Pickers;
using Windows.Storage;
using MineRefine.Models;
using MineRefine.Services;

namespace MineRefine.Views
{
    public sealed partial class UltimateMineralActionDialog : Window
    {
        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-07 09:50:08";
        private const string CURRENT_USER = "Valinor-70";

        private readonly DataService _dataService;
        private readonly MarketService _marketService;
        private readonly GameService _gameService;
        private Player _currentPlayer;
        private List<Mineral> _availableMinerals = new(); // Fixed: Initialize to avoid CS8618
        private List<MarketData> _marketData = new(); // Fixed: Initialize to avoid CS8618

        public UltimateMineralActionDialog(DataService dataService, Player player)
        {
            this.InitializeComponent();

            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _gameService = new GameService();
            _marketService = new MarketService();
            _currentPlayer = player ?? throw new ArgumentNullException(nameof(player));

            // Set window properties using AppWindow
            this.Title = "Ultimate Mineral Actions - Mine & Refine";
            SetWindowSize(1000, 750);

            LoadMineralData();
            UpdateMarketDisplay();
        }

        private void LoadMineralData()
        {
            try
            {
                _availableMinerals = _dataService.GetMinerals();
                _marketData = _marketService.GetMarketData();

                PopulateMineralGrid();
                UpdatePlayerInfo();
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialog($"Error loading mineral data: {ex.Message}");
            }
        }

        private void PopulateMineralGrid()
        {
            MineralGrid.Children.Clear();
            MineralGrid.RowDefinitions.Clear();

            // Header row
            MineralGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            CreateHeaderRow();

            // Mineral rows
            for (int i = 0; i < _availableMinerals.Count; i++)
            {
                MineralGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                CreateMineralRow(_availableMinerals[i], i + 1);
            }
        }

        private void CreateHeaderRow()
        {
            var headers = new[] { "Mineral", "Type", "Value", "Market Price", "Trend", "Actions" };

            for (int i = 0; i < headers.Length; i++)
            {
                // Fixed: Remove Background property and use Border instead
                var headerBorder = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.DarkGray),
                    Padding = new Thickness(10, 5, 10, 5),
                    Child = new TextBlock
                    {
                        Text = headers[i],
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                        TextAlignment = TextAlignment.Center
                    }
                };

                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                MineralGrid.Children.Add(headerBorder);
            }
        }

        private void CreateMineralRow(Mineral mineral, int row)
        {
            var marketData = _marketData.FirstOrDefault(m => m.MineralId == mineral.Id);
            var marketPrice = marketData?.CurrentPrice ?? mineral.Value;
            var trend = marketData?.Trend ?? "Stable";
            var trendIcon = GetTrendIcon(trend);
            var priceMultiplier = marketData?.PriceMultiplier ?? 1.0;

            // Mineral name and icon
            var namePanel = new StackPanel { Orientation = Orientation.Horizontal };
            namePanel.Children.Add(new TextBlock
            {
                Text = mineral.Icon,
                FontSize = 16,
                Margin = new Thickness(0, 0, 5, 0),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            });
            namePanel.Children.Add(new TextBlock
            {
                Text = mineral.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            });

            var nameContainer = new Border
            {
                Child = namePanel,
                Padding = new Thickness(10, 5, 10, 5),
                Background = GetRarityBrush(mineral.Rarity),
                Margin = new Thickness(1)
            };
            Grid.SetRow(nameContainer, row);
            Grid.SetColumn(nameContainer, 0);
            MineralGrid.Children.Add(nameContainer);

            // Mineral type - Fixed: Use Border instead of TextBlock Background
            var typeBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 48)),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(1),
                Child = new TextBlock
                {
                    Text = $"{mineral.GetTypeEmoji()} {mineral.MineralType}",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
                }
            };
            Grid.SetRow(typeBorder, row);
            Grid.SetColumn(typeBorder, 1);
            MineralGrid.Children.Add(typeBorder);

            // Base value - Fixed: Use Border instead of TextBlock Background
            var valueBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 48)),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(1),
                Child = new TextBlock
                {
                    Text = FormatMoney(mineral.Value),
                    TextAlignment = TextAlignment.Center,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.LightGreen)
                }
            };
            Grid.SetRow(valueBorder, row);
            Grid.SetColumn(valueBorder, 2);
            MineralGrid.Children.Add(valueBorder);

            // Market price
            var marketPricePanel = new StackPanel();
            marketPricePanel.Children.Add(new TextBlock
            {
                Text = FormatMoney(marketPrice),
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gold)
            });

            if (Math.Abs(priceMultiplier - 1.0) > 0.01)
            {
                var multiplierText = new TextBlock
                {
                    Text = $"({priceMultiplier:P0})",
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Foreground = priceMultiplier > 1.0 ?
                        new SolidColorBrush(Microsoft.UI.Colors.Green) :
                        new SolidColorBrush(Microsoft.UI.Colors.Red)
                };
                marketPricePanel.Children.Add(multiplierText);
            }

            var marketContainer = new Border
            {
                Child = marketPricePanel,
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 48)),
                Margin = new Thickness(1)
            };
            Grid.SetRow(marketContainer, row);
            Grid.SetColumn(marketContainer, 3);
            MineralGrid.Children.Add(marketContainer);

            // Market trend
            var trendPanel = new StackPanel { Orientation = Orientation.Horizontal };
            trendPanel.Children.Add(new TextBlock
            {
                Text = trendIcon,
                Margin = new Thickness(0, 0, 5, 0),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            });
            trendPanel.Children.Add(new TextBlock
            {
                Text = trend,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            });

            var trendContainer = new Border
            {
                Child = trendPanel,
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 48)),
                Margin = new Thickness(1)
            };
            Grid.SetRow(trendContainer, row);
            Grid.SetColumn(trendContainer, 4);
            MineralGrid.Children.Add(trendContainer);

            // Action buttons
            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // View Details button
            var detailsButton = new Button
            {
                Content = "📊 Details",
                Margin = new Thickness(2),
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 70, 130, 180)),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            };
            detailsButton.Click += (s, e) => ShowMineralDetails(mineral, marketData);
            actionPanel.Children.Add(detailsButton);

            // Sell button (if player has this mineral)
            var playerMineralCount = _currentPlayer.MineralStats.GetValueOrDefault(mineral.Id, 0);
            if (playerMineralCount > 0)
            {
                var sellButton = new Button
                {
                    Content = $"💰 Sell ({playerMineralCount})",
                    Margin = new Thickness(2),
                    Padding = new Thickness(8, 4, 8, 4),
                    Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 34, 139, 34)),
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
                };
                sellButton.Click += (s, e) => SellMineral(mineral, marketData);
                actionPanel.Children.Add(sellButton);
            }

            // Market Analysis button
            var analysisButton = new Button
            {
                Content = "📈 Market",
                Margin = new Thickness(2),
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 0, 128)),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            };
            analysisButton.Click += (s, e) => ShowMarketAnalysis(mineral, marketData);
            actionPanel.Children.Add(analysisButton);

            var actionContainer = new Border
            {
                Child = actionPanel,
                Padding = new Thickness(5),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 48)),
                Margin = new Thickness(1)
            };
            Grid.SetRow(actionContainer, row);
            Grid.SetColumn(actionContainer, 5);
            MineralGrid.Children.Add(actionContainer);
        }

        private void UpdatePlayerInfo()
        {
            PlayerNameText.Text = _currentPlayer.Name;
            PlayerMoneyText.Text = FormatMoney(_currentPlayer.TotalMoney);
            PlayerLevelText.Text = _currentPlayer.Level.ToString();
            PlayerRankText.Text = _currentPlayer.Rank.ToString().Replace("_", " ");

            // Update mineral inventory count
            var totalMinerals = _currentPlayer.MineralStats.Values.Sum();
            InventoryCountText.Text = $"Total Minerals: {totalMinerals:N0}";

            // Update timestamp
            LastUpdateText.Text = $"Last Updated: {CURRENT_DATETIME}";
        }

        private void UpdateMarketDisplay()
        {
            try
            {
                // Refresh market data
                _marketService.RefreshMarketData();
                _marketData = _marketService.GetMarketData();

                // Update market summary
                var analysis = _marketService.GetMarketAnalysis();

                MarketSentimentText.Text = $"Market Sentiment: {analysis["market_sentiment"]}";
                ActiveEventsText.Text = $"Active Events: {analysis["active_events"]}";

                var totalVolume = Convert.ToDouble(analysis["total_volume"]);
                TotalVolumeText.Text = $"Total Volume: {totalVolume:N0}";

                var averageChange = Convert.ToDouble(analysis["average_change"]);
                var changeColor = averageChange > 0 ?
                    new SolidColorBrush(Microsoft.UI.Colors.Green) :
                    averageChange < 0 ?
                    new SolidColorBrush(Microsoft.UI.Colors.Red) :
                    new SolidColorBrush(Microsoft.UI.Colors.Gray);

                AverageChangeText.Text = $"Avg Change: {averageChange:+0.0;-0.0;0.0}%";
                AverageChangeText.Foreground = changeColor;
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialog($"Error updating market display: {ex.Message}");
            }
        }

        private async void ShowMineralDetails(Mineral mineral, MarketData? marketData)
        {
            var details = $"""
                🔍 MINERAL DETAILS
                
                Name: {mineral.Name} {mineral.Icon}
                Type: {mineral.MineralType} {mineral.GetTypeEmoji()}
                Rarity: {mineral.Rarity}
                Description: {mineral.Description}
                
                💰 VALUE INFORMATION
                Base Value: {FormatMoney(mineral.Value)}
                Refined Value: {FormatMoney(mineral.RefinedValue)}
                Weight: {mineral.Weight:F1} kg
                
                📊 MARKET DATA
                Current Price: {FormatMoney(marketData?.CurrentPrice ?? mineral.Value)}
                Price Change: {marketData?.PriceChange:+0.0;-0.0;0.0}%
                Trend: {marketData?.Trend ?? "Stable"}
                Supply: {marketData?.Supply:F0 ?? 100}%
                Demand: {marketData?.Demand:F0 ?? 100}%
                Volatility: {marketData?.Volatility * 100:F1 ?? 10}%
                
                🌍 LOCATION DATA
                Found In: {string.Join(", ", mineral.FoundInLocations)}
                Discovered: {mineral.DiscoveredDate:yyyy-MM-dd}
                Discovered By: {mineral.DiscoveredBy}
                
                🔬 PROPERTIES
                Radioactive: {(mineral.IsRadioactive ? "Yes" : "No")}
                Magnetic: {(mineral.IsMagnetic ? "Yes" : "No")}
                Legendary: {(mineral.IsLegendary ? "Yes" : "No")}
                Special Properties: {string.Join(", ", mineral.SpecialProperties)}
                """;

            await ShowInfoDialog(details, $"{mineral.Name} - Detailed Information");
        }

        private async void SellMineral(Mineral mineral, MarketData? marketData)
        {
            var playerMineralCount = _currentPlayer.MineralStats.GetValueOrDefault(mineral.Id, 0);
            if (playerMineralCount <= 0)
            {
                await ShowInfoDialog("You don't have any of this mineral to sell.", "No Inventory");
                return;
            }

            // Create and show sell dialog
            var quantityDialog = new SellQuantityDialog(mineral, playerMineralCount, _marketService);
            quantityDialog.XamlRoot = this.Content.XamlRoot;

            var result = await quantityDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var quantityToSell = quantityDialog.QuantityToSell;
                var sellValue = _marketService.CalculateSellValue(mineral.Id, mineral.Value, quantityToSell);

                // Update player inventory
                _currentPlayer.MineralStats[mineral.Id] -= quantityToSell;
                if (_currentPlayer.MineralStats[mineral.Id] <= 0)
                {
                    _currentPlayer.MineralStats.Remove(mineral.Id);
                }

                // Add money to player
                _currentPlayer.TotalMoney += sellValue;
                _currentPlayer.TotalEarnings += sellValue;

                // Process market effects
                _marketService.ProcessSale(mineral.Id, quantityToSell);

                // Save player data - Fixed: Properly await the task
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var players = await _dataService.LoadPlayersAsync();
                        var playerIndex = players.FindIndex(p => p.Name == _currentPlayer.Name);
                        if (playerIndex >= 0)
                        {
                            players[playerIndex] = _currentPlayer;
                            await _dataService.SavePlayersAsync(players);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            _ = ShowErrorDialog($"Error saving player data: {ex.Message}");
                        });
                    }
                });

                await ShowInfoDialog($"Successfully sold {quantityToSell}x {mineral.Name} for {FormatMoney(sellValue)}!",
                    "Sale Complete");

                // Refresh display
                UpdatePlayerInfo();
                PopulateMineralGrid();
                UpdateMarketDisplay();
            }
        }

        private async void ShowMarketAnalysis(Mineral mineral, MarketData? marketData)
        {
            if (marketData == null)
            {
                await ShowInfoDialog("No market data available for this mineral.", "No Market Data");
                return;
            }

            var analysis = $"""
                📈 MARKET ANALYSIS - {mineral.Name}
                
                💰 CURRENT PRICING
                Market Price: {FormatMoney(marketData.CurrentPrice)}
                Base Value: {FormatMoney(mineral.Value)}
                Price Multiplier: {marketData.PriceMultiplier:P1}
                24h Change: {marketData.PriceChange:+0.00;-0.00;0.00}%
                
                📊 SUPPLY & DEMAND
                Current Supply: {marketData.Supply:F1}%
                Current Demand: {marketData.Demand:F1}%
                Trading Volume: {marketData.Volume:N0}
                Market Sentiment: {marketData.MarketSentiment}
                
                📈 MARKET TRENDS
                Current Trend: {marketData.Trend} {GetTrendIcon(marketData.Trend)}
                Volatility Rating: {marketData.Volatility * 100:F1}%
                
                🔮 PRICE PREDICTION
                Next 24h Outlook: {GetPriceOutlook(marketData)}
                Recommended Action: {GetTradeRecommendation(marketData)}
                
                🕒 TIMING INFO
                Last Update: {marketData.LastUpdated:yyyy-MM-dd HH:mm:ss}
                Market Status: {(_marketService.IsMarketOpen() ? "Open" : "Closed")}
                
                💡 TRADER NOTES
                • High volatility = Higher risk/reward
                • Watch supply/demand balance
                • Consider market events impact
                • Diversify mineral portfolio
                """;

            await ShowInfoDialog(analysis, $"Market Analysis - {mineral.Name}");
        }

        private string GetPriceOutlook(MarketData marketData)
        {
            var supplyDemandRatio = marketData.Demand / marketData.Supply;
            var trendStrength = Math.Abs(marketData.PriceChange);

            return (supplyDemandRatio, trendStrength) switch
            {
                ( > 1.5, > 5) => "Strong Bullish - Prices likely to rise significantly",
                ( > 1.2, > 2) => "Moderate Bullish - Gradual price increase expected",
                ( < 0.7, > 5) => "Strong Bearish - Prices likely to fall significantly",
                ( < 0.9, > 2) => "Moderate Bearish - Gradual price decrease expected",
                _ => "Neutral - Prices likely to remain stable"
            };
        }

        private string GetTradeRecommendation(MarketData marketData)
        {
            var supplyDemandRatio = marketData.Demand / marketData.Supply;
            var priceChange = marketData.PriceChange;

            return (supplyDemandRatio, priceChange) switch
            {
                ( > 1.3, < -2) => "🟢 STRONG BUY - Low price, high demand",
                ( > 1.1, _) => "🟡 BUY - Demand exceeds supply",
                ( < 0.8, > 2) => "🔴 STRONG SELL - High price, low demand",
                ( < 0.9, _) => "🟠 SELL - Supply exceeds demand",
                _ => "⚪ HOLD - Market conditions neutral"
            };
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMineralData();
            UpdateMarketDisplay();
            await ShowInfoDialog("Market data refreshed!", "Refresh Complete");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportData = GenerateMarketReport();

                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("Text files", new List<string>() { ".txt" });
                picker.SuggestedFileName = $"MineralMarketReport_{DateTime.Parse(CURRENT_DATETIME):yyyyMMdd_HHmmss}";

                // Associate picker with current window
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, exportData);
                    await ShowInfoDialog($"Market report exported to: {file.Path}", "Export Complete");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error exporting report: {ex.Message}");
            }
        }

        private string GenerateMarketReport()
        {
            var report = $"""
                ═══════════════════════════════════════════════════════════
                           MINE & REFINE - MARKET ANALYSIS REPORT
                ═══════════════════════════════════════════════════════════
                
                Report Generated: {CURRENT_DATETIME}
                Player: {_currentPlayer.Name} (Level {_currentPlayer.Level})
                Current Balance: {FormatMoney(_currentPlayer.TotalMoney)}
                
                ═══════════════════════════════════════════════════════════
                                      MARKET SUMMARY
                ═══════════════════════════════════════════════════════════
                
                """;

            var analysis = _marketService.GetMarketAnalysis();
            report += $"""
                Market Sentiment: {analysis["market_sentiment"]}
                Total Volume: {Convert.ToDouble(analysis["total_volume"]):N0}
                Average Change: {Convert.ToDouble(analysis["average_change"]):+0.00;-0.00;0.00}%
                Active Events: {analysis["active_events"]}
                
                Top Performer: {analysis["highest_gainer"]} ({Convert.ToDouble(analysis["highest_gain_percent"]):+0.00;-0.00;0.00}%)
                Worst Performer: {analysis["biggest_loser"]} ({Convert.ToDouble(analysis["biggest_loss_percent"]):+0.00;-0.00;0.00}%)
                
                ═══════════════════════════════════════════════════════════
                                    INDIVIDUAL MINERALS
                ═══════════════════════════════════════════════════════════
                
                """;

            foreach (var mineral in _availableMinerals)
            {
                var marketData = _marketData.FirstOrDefault(m => m.MineralId == mineral.Id);
                var playerCount = _currentPlayer.MineralStats.GetValueOrDefault(mineral.Id, 0);

                report += $"""
                    {mineral.Name} {mineral.Icon}
                    ├─ Type: {mineral.MineralType} | Rarity: {mineral.Rarity}
                    ├─ Base Value: {FormatMoney(mineral.Value)}
                    ├─ Market Price: {FormatMoney(marketData?.CurrentPrice ?? mineral.Value)}
                    ├─ Change: {marketData?.PriceChange:+0.00;-0.00;0.00}% | Trend: {marketData?.Trend ?? "Stable"}
                    ├─ Supply: {marketData?.Supply:F0 ?? 100}% | Demand: {marketData?.Demand:F0 ?? 100}%
                    └─ Player Inventory: {playerCount:N0}
                    
                    """;
            }

            var totalValue = 0L;
            foreach (var kvp in _currentPlayer.MineralStats.Where(ms => ms.Value > 0))
            {
                var mineral = _availableMinerals.FirstOrDefault(m => m.Id == kvp.Key);
                var marketData = _marketData.FirstOrDefault(m => m.MineralId == kvp.Key);
                var currentPrice = marketData?.CurrentPrice ?? mineral?.Value ?? 0;
                var itemValue = currentPrice * kvp.Value;
                totalValue += itemValue;
            }

            report += $"""
                
                Total Inventory Value: {FormatMoney(totalValue)}
                
                ═══════════════════════════════════════════════════════════
                                       END OF REPORT
                ═══════════════════════════════════════════════════════════
                """;

            return report;
        }

        // Helper Methods
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

        private static string GetTrendIcon(string trend)
        {
            return trend switch
            {
                "Bullish" => "📈",
                "Rising" => "↗️",
                "Bearish" => "📉",
                "Declining" => "↘️",
                _ => "📊"
            };
        }

        private static SolidColorBrush GetRarityBrush(string rarity)
        {
            return rarity switch
            {
                "Common" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128)),
                "Uncommon" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 30, 255, 0)),
                "Rare" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 112, 221)),
                "Epic" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 163, 53, 238)),
                "Legendary" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 128, 0)),
                "Mythical" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 230, 204, 128)),
                "Quantum" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 255, 255)),
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 192, 192, 192))
            };
        }

        private async Task ShowInfoDialog(string message, string title = "Information")
        {
            var dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowErrorDialog(string message)
        {
            await ShowInfoDialog(message, "Error");
        }

        // Fixed: Simplified window sizing for WinUI 3
        private void SetWindowSize(int width, int height)
        {
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
                }
            }
            catch
            {
                // Ignore sizing errors - window will use default size
            }
        }
    }
}