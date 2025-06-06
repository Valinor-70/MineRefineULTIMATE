using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using MineRefine.Services;
using Windows.UI;

namespace MineRefine.Views
{
    public sealed class UltimateMineralActionDialog : ContentDialog
    {
        public UltimateMineralActionDialog(Mineral mineral, Player player, MiningSession session)
        {
            Title = $"{mineral.GetTypeEmoji()} {mineral.Name} Discovered!";
            
            var stackPanel = new StackPanel { Spacing = 15 };
            
            // Mineral info
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = $"💎 Rarity: {mineral.GetRarityLevel()}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            });
            
            // Values
            var valuePanel = new StackPanel { Spacing = 5 };
            valuePanel.Children.Add(new TextBlock 
            { 
                Text = $"⛏️ Mining Value: £{mineral.Value:N0}",
                FontSize = 14
            });
            valuePanel.Children.Add(new TextBlock 
            { 
                Text = $"🔬 Refining Value: £{mineral.RefinedValue:N0}",
                FontSize = 14
            });
            
            // Apply market modifiers
            var marketService = new MarketService();
            var marketData = marketService.GetMarketData(mineral.Name);
            if (marketData.PriceMultiplier != 1.0)
            {
                var marketValue = (long)(mineral.Value * marketData.PriceMultiplier);
                valuePanel.Children.Add(new TextBlock 
                { 
                    Text = $"📈 Market Value: £{marketValue:N0} ({marketData.PriceMultiplier:P1})",
                    FontSize = 14,
                    // Fix for CS0019: Ensure 'marketData.Trend' is converted to a numeric type before comparison
                    Foreground = new SolidColorBrush(double.TryParse(marketData.Trend, out double trendValue) && trendValue > 0 ? Colors.Red : Colors.Green)
                });
            }
            
            stackPanel.Children.Add(valuePanel);
            
            // Location and risk info
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = $"📍 Location: {session.LocationId} | 🎲 Risk: {session.RiskLevel:F1}x",
                FontSize = 12,
                Opacity = 0.8
            });
            
            // Weather effects
            if (session.Weather != WeatherCondition.Clear)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"🌤️ Weather: {session.Weather} (affects stamina cost)",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.Orange)
                });
            }
            
            // Player bonuses
            var bonusPanel = new StackPanel { Spacing = 3 };
            if (player.PowerDrill)
                bonusPanel.Children.Add(new TextBlock { Text = "⚡ Power Drill: 2x mining value", FontSize = 11 });
            if (player.RefineryUpgrade)
                bonusPanel.Children.Add(new TextBlock { Text = "🏭 Refinery: Enhanced refining", FontSize = 11 });
            if (player.LuckyCharm)
                bonusPanel.Children.Add(new TextBlock { Text = "🍀 Lucky Charm: 10% chance for 3x", FontSize = 11 });
            
            if (bonusPanel.Children.Count > 0)
            {
                stackPanel.Children.Add(new TextBlock { Text = "Active Bonuses:", FontWeight = FontWeights.SemiBold, FontSize = 12 });
                stackPanel.Children.Add(bonusPanel);
            }
            
            Content = stackPanel;
            
            PrimaryButtonText = "⛏️ Mine";
            SecondaryButtonText = "🔬 Refine";
            CloseButtonText = "👁️ Ignore";
            
            DefaultButton = ContentDialogButton.Primary;
            
            // Set background based on rarity
            if (mineral.Value >= 10000000)
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 1),
                    GradientStops =
                    {
                        new GradientStop { Color = Color.FromArgb(30, 255, 215, 0), Offset = 0 },
                        new GradientStop { Color = Color.FromArgb(10, 138, 43, 226), Offset = 1 }
                    }
                };
            }
            else if (mineral.Value >= 1000000)
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 1),
                    GradientStops =
                    {
                        new GradientStop { Color = Color.FromArgb(30, 147, 0, 211), Offset = 0 },
                        new GradientStop { Color = Color.FromArgb(10, 147, 0, 211), Offset = 1 }
                    }
                };
            }
        }
    }
}