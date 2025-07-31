using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using MineRefine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace MineRefine.Views
{
    public sealed class UltimateUpgradesDialog : ContentDialog, IDisposable
    {
        private readonly Player _player;
        private readonly GameService _gameService; // Fixed: Changed from EnhancedGameService to GameService
        private StackPanel _upgradesPanel;
        private TextBlock _moneyTextBlock;
        private bool _disposed = false;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-06 21:39:17";
        private const string CURRENT_USER = "Valinor-70";

        // Fixed constructor parameter
        public UltimateUpgradesDialog(Player player, GameService gameService)
        {
            _player = player;
            _gameService = gameService;

            Title = "🛠️ Mining Upgrades";
            PrimaryButtonText = "Close";
            DefaultButton = ContentDialogButton.Primary;

            // Improved: Add proper event handling for cleanup
            this.Closing += OnDialogClosing;

            SetupContent();
        }

        private void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Cleanup resources when dialog is closing
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    // Clear event handlers to prevent memory leaks
                    this.Closing -= OnDialogClosing;
                    
                    // Clear references
                    _upgradesPanel?.Children.Clear();
                    _upgradesPanel = null;
                    _moneyTextBlock = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UltimateUpgradesDialog.Dispose error: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        private void SetupContent()
        {
            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var mainPanel = new StackPanel { Spacing = 20 };

            // Header
            var headerPanel = new StackPanel { Spacing = 10 };

            headerPanel.Children.Add(new TextBlock
            {
                Text = "Enhance your mining capabilities with powerful upgrades!",
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White)
            });

            _moneyTextBlock = new TextBlock
            {
                Text = $"💰 Available Funds: £{_player.TotalMoney:N0}",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Gold)
            };
            headerPanel.Children.Add(_moneyTextBlock);

            mainPanel.Children.Add(headerPanel);

            // Upgrades
            _upgradesPanel = new StackPanel { Spacing = 12 };
            PopulateUpgrades();
            mainPanel.Children.Add(_upgradesPanel);

            // Equipment section
            var equipmentExpander = new Expander
            {
                Header = "⚒️ Equipment Management",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var equipmentPanel = new StackPanel { Spacing = 8 };

            if (_player.Equipment.Any())
            {
                foreach (var equipment in _player.Equipment)
                {
                    var equipmentBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(30, 100, 149, 237)),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(12, 12, 12, 12),
                        Margin = new Thickness(0, 2, 0, 0)
                    };

                    var equipmentGrid = new Grid();
                    equipmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    equipmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var equipmentInfo = new StackPanel();
                    equipmentInfo.Children.Add(new TextBlock
                    {
                        Text = $"{equipment.Icon} {equipment.Name} (Level {equipment.Level})",
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Colors.White)
                    });

                    var durabilityPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
                    durabilityPanel.Children.Add(new TextBlock
                    {
                        Text = "Durability:",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.LightGray)
                    });

                    var durabilityBar = new ProgressBar
                    {
                        Value = equipment.Durability,
                        Maximum = equipment.MaxDurability,
                        Width = 100,
                        Height = 6,
                        Foreground = equipment.Durability > equipment.MaxDurability * 0.5 ?
                            new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
                    };
                    durabilityPanel.Children.Add(durabilityBar);
                    durabilityPanel.Children.Add(new TextBlock
                    {
                        Text = $"{equipment.Durability}/{equipment.MaxDurability}",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Colors.LightGray)
                    });

                    equipmentInfo.Children.Add(durabilityPanel);
                    Grid.SetColumn(equipmentInfo, 0);

                    var actionPanel = new StackPanel { Spacing = 5 };

                    // Fixed: Added CanUpgrade property check with fallback
                    bool canUpgrade = equipment.Level < 10 && equipment.Durability > 0; // Simple upgrade logic
                    if (canUpgrade && _player.TotalMoney >= equipment.UpgradeCost)
                    {
                        var upgradeButton = new Button
                        {
                            Content = $"Upgrade (£{equipment.UpgradeCost:N0})",
                            FontSize = 11,
                            Tag = equipment.Id,
                            Background = new SolidColorBrush(Colors.DarkGreen),
                            Foreground = new SolidColorBrush(Colors.White)
                        };
                        upgradeButton.Click += EquipmentUpgradeButton_Click;
                        actionPanel.Children.Add(upgradeButton);
                    }

                    var toggleButton = new ToggleButton
                    {
                        Content = equipment.IsEquipped ? "Equipped" : "Equip",
                        FontSize = 11,
                        IsChecked = equipment.IsEquipped,
                        Tag = equipment.Id,
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    toggleButton.Click += EquipmentToggleButton_Click;
                    actionPanel.Children.Add(toggleButton);

                    Grid.SetColumn(actionPanel, 1);

                    equipmentGrid.Children.Add(equipmentInfo);
                    equipmentGrid.Children.Add(actionPanel);
                    equipmentBorder.Child = equipmentGrid;
                    equipmentPanel.Children.Add(equipmentBorder);
                }
            }
            else
            {
                equipmentPanel.Children.Add(new TextBlock
                {
                    Text = "No equipment available. Equipment will be unlocked as you progress!",
                    FontStyle = Windows.UI.Text.FontStyle.Italic,
                    Opacity = 0.7,
                    Foreground = new SolidColorBrush(Colors.LightGray)
                });
            }

            equipmentExpander.Content = equipmentPanel;
            mainPanel.Children.Add(equipmentExpander);

            scrollViewer.Content = mainPanel;
            Content = scrollViewer;
        }

        private void PopulateUpgrades()
        {
            _upgradesPanel.Children.Clear();
            var upgrades = GetAvailableUpgrades(); // Fixed: Use local method instead of _gameService

            foreach (var upgrade in upgrades)
            {
                var upgradeBorder = new Border
                {
                    Background = upgrade.Value.owned ?
                        new SolidColorBrush(Color.FromArgb(40, 34, 139, 34)) :
                        new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15, 15, 15, 15),
                    BorderBrush = upgrade.Value.owned ?
                        new SolidColorBrush(Colors.Green) :
                        new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
                    BorderThickness = new Thickness(1, 1, 1, 1)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Upgrade info
                var upgradeInfo = new StackPanel();

                var titlePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                titlePanel.Children.Add(new TextBlock
                {
                    Text = upgrade.Value.icon,
                    FontSize = 20,
                    VerticalAlignment = VerticalAlignment.Center
                });
                titlePanel.Children.Add(new TextBlock
                {
                    Text = upgrade.Key,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White)
                });

                if (upgrade.Value.owned)
                {
                    titlePanel.Children.Add(new TextBlock
                    {
                        Text = "✅ OWNED",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.Green),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0)
                    });
                }

                upgradeInfo.Children.Add(titlePanel);

                upgradeInfo.Children.Add(new TextBlock
                {
                    Text = upgrade.Value.description,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13,
                    Opacity = 0.8,
                    Margin = new Thickness(0, 5, 0, 0),
                    Foreground = new SolidColorBrush(Colors.LightGray)
                });

                upgradeInfo.Children.Add(new TextBlock
                {
                    Text = $"💰 Cost: £{upgrade.Value.cost:N0}",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = upgrade.Value.canAfford ?
                        new SolidColorBrush(Colors.Gold) :
                        new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(0, 8, 0, 0)
                });

                Grid.SetColumn(upgradeInfo, 0);
                grid.Children.Add(upgradeInfo);

                // Purchase button
                if (!upgrade.Value.owned)
                {
                    var purchaseButton = new Button
                    {
                        Content = upgrade.Value.canAfford ? "💳 Purchase" : "💸 Can't Afford",
                        IsEnabled = upgrade.Value.canAfford,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = upgrade.Key,
                        Background = upgrade.Value.canAfford ?
                            new SolidColorBrush(Colors.DarkGreen) :
                            new SolidColorBrush(Colors.DarkRed),
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    purchaseButton.Click += PurchaseButton_Click;
                    Grid.SetColumn(purchaseButton, 1);
                    grid.Children.Add(purchaseButton);
                }

                upgradeBorder.Child = grid;
                _upgradesPanel.Children.Add(upgradeBorder);
            }
        }

        // Added: Local method to get available upgrades
        private Dictionary<string, (string icon, string description, long cost, bool owned, bool canAfford)> GetAvailableUpgrades()
        {
            var upgrades = new Dictionary<string, (string icon, string description, long cost, bool owned, bool canAfford)>();

            // Power Drill
            var powerDrillCost = 50000L;
            upgrades["Power Drill"] = (
                "🔧",
                "Advanced drilling technology increases mining efficiency by 25% and reduces energy consumption.",
                powerDrillCost,
                _player.PowerDrill,
                _player.TotalMoney >= powerDrillCost && !_player.PowerDrill
            );

            // Lucky Charm
            var luckyCharmCost = 75000L;
            upgrades["Lucky Charm"] = (
                "🍀",
                "Mystical artifact that increases the chance of discovering rare minerals by 15%.",
                luckyCharmCost,
                _player.LuckyCharm,
                _player.TotalMoney >= luckyCharmCost && !_player.LuckyCharm
            );

            // Ground Penetrating Radar
            var gprCost = 100000L;
            upgrades["Ground Penetrating Radar"] = (
                "📡",
                "Advanced scanning technology reveals mineral deposits before mining, increasing success rates.",
                gprCost,
                _player.Gpr,
                _player.TotalMoney >= gprCost && !_player.Gpr
            );

            // Refinery Upgrade
            var refineryCost = 150000L;
            upgrades["Refinery Upgrade"] = (
                "🏭",
                "Enhanced processing facility increases mineral value by 20% through superior refinement.",
                refineryCost,
                _player.RefineryUpgrade,
                _player.TotalMoney >= refineryCost && !_player.RefineryUpgrade
            );

            // Magnetic Survey
            var magneticCost = 200000L;
            upgrades["Magnetic Survey"] = (
                "🧲",
                "Magnetic field analysis equipment detects metallic deposits with 90% accuracy.",
                magneticCost,
                _player.MagneticSurvey,
                _player.TotalMoney >= magneticCost && !_player.MagneticSurvey
            );

            // Quantum Resonator (New upgrade for Valinor-70)
            var quantumCost = 500000L;
            upgrades["Quantum Resonator"] = (
                "⚛️",
                "Quantum-enhanced mining tool that can detect and extract interdimensional minerals.",
                quantumCost,
                _player.UnlockedSkills.Contains("quantum_resonator"),
                _player.TotalMoney >= quantumCost && !_player.UnlockedSkills.Contains("quantum_resonator")
            );

            return upgrades;
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var upgradeName = button?.Tag as string;

            if (upgradeName != null && PurchaseUpgrade(upgradeName))
            {
                _moneyTextBlock.Text = $"💰 Available Funds: £{_player.TotalMoney:N0}";
                PopulateUpgrades();

                // Show success message
                ShowUpgradePurchasedMessage(upgradeName);
            }
        }

        // Added: Local method to handle upgrade purchases
        private bool PurchaseUpgrade(string upgradeName)
        {
            var upgrades = GetAvailableUpgrades();
            if (!upgrades.ContainsKey(upgradeName) || upgrades[upgradeName].owned)
                return false;

            var upgrade = upgrades[upgradeName];
            if (_player.TotalMoney < upgrade.cost)
                return false;

            _player.TotalMoney -= upgrade.cost;

            // Apply the upgrade
            switch (upgradeName)
            {
                case "Power Drill":
                    _player.PowerDrill = true;
                    break;
                case "Lucky Charm":
                    _player.LuckyCharm = true;
                    break;
                case "Ground Penetrating Radar":
                    _player.Gpr = true;
                    break;
                case "Refinery Upgrade":
                    _player.RefineryUpgrade = true;
                    break;
                case "Magnetic Survey":
                    _player.MagneticSurvey = true;
                    break;
                case "Quantum Resonator":
                    _player.UnlockedSkills.Add("quantum_resonator");
                    break;
            }

            return true;
        }

        private async void ShowUpgradePurchasedMessage(string upgradeName)
        {
            try
            {
                var successDialog = new ContentDialog
                {
                    Title = "🎉 Upgrade Purchased!",
                    Content = $"Successfully purchased {upgradeName}! Your mining efficiency has been improved.",
                    CloseButtonText = "Awesome!",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowUpgradePurchasedMessage error: {ex.Message}");
            }
        }

        private void EquipmentUpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var equipmentId = button?.Tag as string;
                var equipment = _player.Equipment.FirstOrDefault(e => e.Id == equipmentId);

                if (equipment != null && _player.TotalMoney >= equipment.UpgradeCost)
                {
                    _player.TotalMoney -= equipment.UpgradeCost;
                    equipment.Level++;
                    equipment.UpgradeCost = (long)(equipment.UpgradeCost * 1.5);
                    equipment.MaxDurability = (int)(equipment.MaxDurability * 1.2);
                    equipment.Durability = equipment.MaxDurability;

                    // Improve bonuses
                    foreach (var bonus in equipment.Bonuses.Keys.ToList())
                    {
                        equipment.Bonuses[bonus] *= 1.1;
                    }

                    _moneyTextBlock.Text = $"💰 Available Funds: £{_player.TotalMoney:N0}";
                    SetupContent(); // Refresh the entire dialog
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EquipmentUpgradeButton_Click error: {ex.Message}");
            }
        }

        private void EquipmentToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as ToggleButton;
                var equipmentId = button?.Tag as string;
                var equipment = _player.Equipment.FirstOrDefault(e => e.Id == equipmentId);

                if (equipment != null)
                {
                    // Unequip other items of the same type
                    foreach (var otherEquipment in _player.Equipment.Where(e => e.Type == equipment.Type && e.Id != equipment.Id))
                    {
                        otherEquipment.IsEquipped = false;
                    }

                    equipment.IsEquipped = button.IsChecked ?? false;
                    equipment.TimesUsed++;

                    SetupContent(); // Refresh to show updated state
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EquipmentToggleButton_Click error: {ex.Message}");
            }
        }
    }
}