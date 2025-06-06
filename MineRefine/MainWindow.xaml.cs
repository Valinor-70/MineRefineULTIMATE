using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using MineRefine.Services;
using MineRefine.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace MineRefine
{
    public sealed partial class MainWindow : Window
    {
        // Services
        private DataService? _dataService;
        private GameService? _gameService;
        private MarketService? _marketService;

        // Game State
        private Player? _currentPlayer;
        private List<MiningLocation> _locations = new();
        private MiningLocation? _currentLocation;
        private double _currentRiskMultiplier = 2.3;
        private bool _isInitialized = false;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-06 21:49:39";
        private const string CURRENT_USER = "Valinor-70";

        public MainWindow()
        {
            this.InitializeComponent();

            // Set window properties
            this.Title = "Mine & Refine Ultimate Edition - Alpha v1.0.0";

            // Initialize after the window is activated - WinUI 3 compatible
            this.Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            // Only initialize once when first activated
            if (e.WindowActivationState != WindowActivationState.Deactivated && !_isInitialized)
            {
                // Unsubscribe to prevent multiple initializations
                this.Activated -= MainWindow_Activated;

                try
                {
                    ShowLoading("Initializing Quantum Mining Systems...");

                    // Initialize services first
                    await InitializeServicesAsync();

                    // Then initialize game data
                    await InitializeGameDataAsync();

                    // Finally initialize UI
                    await InitializeUIAsync();

                    HideLoading();

                    _isInitialized = true;

                    ShowNotification("🚀 System Online", "Mine & Refine Ultimate Edition is ready for quantum mining operations!");
                }
                catch (Exception ex)
                {
                    HideLoading();
                    System.Diagnostics.Debug.WriteLine($"MainWindow initialization error: {ex.Message}");
                    ShowNotification("⚠️ Initialization Error", $"Failed to initialize game systems: {ex.Message}");
                }
            }
        }

        #region Initialization Methods

        private async Task InitializeServicesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _dataService = new DataService();
                    _gameService = new GameService();
                    _marketService = new MarketService();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeServicesAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeGameDataAsync()
        {
            try
            {
                if (_dataService == null)
                    throw new InvalidOperationException("DataService not initialized");

                // Load or create player
                await LoadOrCreatePlayerAsync();

                // Load game data
                await Task.Run(() =>
                {
                    _locations = _dataService.GetMiningLocations();
                    _currentLocation = _locations.FirstOrDefault(l => l.Id == "quantum_realm") ?? _locations.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeGameDataAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeUIAsync()
        {
            try
            {
                // Initialize UI on UI thread - WinUI 3 compatible
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    UpdatePlayerDisplay();
                    PopulateLocationComboBox();
                    UpdateLocationDescription();
                    InitializeGameLog();
                    ShowWelcomeTab();
                });

                // Small delay to ensure UI updates complete
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeUIAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task LoadOrCreatePlayerAsync()
        {
            try
            {
                if (_dataService == null)
                    throw new InvalidOperationException("DataService not initialized");

                var allPlayers = await _dataService.LoadPlayersAsync();
                _currentPlayer = allPlayers.FirstOrDefault(p => p.Name == CURRENT_USER);

                if (_currentPlayer == null)
                {
                    // Create default player for Valinor-70
                    _currentPlayer = CreateDefaultPlayer();
                    allPlayers.Add(_currentPlayer);
                    await _dataService.SavePlayersAsync(allPlayers);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadOrCreatePlayerAsync error: {ex.Message}");

                // Fallback: create default player
                _currentPlayer = CreateDefaultPlayer();
            }
        }

        private Player CreateDefaultPlayer()
        {
            return new Player
            {
                Name = CURRENT_USER,
                Difficulty = Difficulty.EXPERT,
                Rank = Rank.ASCENDED_MINER,
                Level = 47,
                TotalMoney = 2847592,
                TotalEarnings = 12800000,
                TotalMinesCount = 1847,
                Stamina = 87,
                MaxStamina = 100,
                ExperiencePoints = 45000,
                SkillPoints = 23,
                Multiplier = 2.3,
                CreatedDate = DateTime.Parse(CURRENT_DATETIME),
                LastPlayed = DateTime.Parse(CURRENT_DATETIME),
                LastLogin = DateTime.Parse(CURRENT_DATETIME),
                CurrentLocationId = "quantum_realm",
                Avatar = "quantum_explorer",
                TutorialCompleted = true,
                PreferredDifficulty = "EXPERT",
                ConsecutiveSuccessfulMines = 156,
                BestMiningStreak = 234,
                SingleBestMineValue = 890000,
                UnlockedLocations = new() { "surface_mine", "deep_caves", "volcanic_depths", "quantum_realm" },
                UnlockedSkills = new() { "efficient_mining", "deep_mining", "master_miner", "quantum_resonator" },
                PowerDrill = true,
                LuckyCharm = true,
                Gpr = true,
                RefineryUpgrade = true,
                MagneticSurvey = true
            };
        }

        #endregion

        #region UI Update Methods

        private void UpdatePlayerDisplay()
        {
            try
            {
                if (_currentPlayer == null) return;

                // Update UI elements if they exist
                if (PlayerNameTextBlock != null)
                    PlayerNameTextBlock.Text = _currentPlayer.Name;

                if (PlayerRankTextBlock != null)
                    PlayerRankTextBlock.Text = _currentPlayer.Rank.ToString().Replace("_", " ");

                if (PlayerLevelTextBlock != null)
                    PlayerLevelTextBlock.Text = $"Level {_currentPlayer.Level}";

                if (PlayerMoneyTextBlock != null)
                    PlayerMoneyTextBlock.Text = $"£{_currentPlayer.TotalMoney:N0}";

                if (TotalMinesTextBlock != null)
                    TotalMinesTextBlock.Text = $"Mines: {_currentPlayer.TotalMinesCount:N0}";

                if (SuccessRateTextBlock != null)
                    SuccessRateTextBlock.Text = $"Success: {_currentPlayer.GetSuccessRate():F1}%";

                if (TotalEarningsTextBlock != null)
                    TotalEarningsTextBlock.Text = $"Lifetime: £{_currentPlayer.TotalEarnings / 1000000.0:F1}M";

                if (SkillPointsTextBlock != null)
                    SkillPointsTextBlock.Text = $"Skill Points: {_currentPlayer.SkillPoints}";

                if (PlayerNetWorthTextBlock != null)
                    PlayerNetWorthTextBlock.Text = $"Net Worth: £{(_currentPlayer.TotalMoney + _currentPlayer.TotalEarnings) / 1000000.0:F1}M";

                if (PlayerDebtTextBlock != null)
                    PlayerDebtTextBlock.Text = $"Debt: £{_currentPlayer.Debt:N0}";

                // Update progress bars
                if (StaminaProgressBar != null)
                {
                    StaminaProgressBar.Value = _currentPlayer.Stamina;
                    StaminaProgressBar.Maximum = _currentPlayer.MaxStamina;
                }

                if (ExperienceProgressBar != null)
                {
                    var progressPercent = (_currentPlayer.ExperiencePoints % 1000) / 10.0; // Simplified XP calculation
                    ExperienceProgressBar.Value = progressPercent;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePlayerDisplay error: {ex.Message}");
            }
        }

        private void PopulateLocationComboBox()
        {
            try
            {
                if (LocationComboBox == null || _currentPlayer == null) return;

                LocationComboBox.Items.Clear();

                foreach (var location in _locations.Where(l => _currentPlayer.UnlockedLocations.Contains(l.Id)))
                {
                    var item = new ComboBoxItem
                    {
                        Content = $"{location.Icon} {location.Name}",
                        Tag = location.Id
                    };

                    LocationComboBox.Items.Add(item);

                    if (location.Id == _currentPlayer.CurrentLocationId)
                    {
                        LocationComboBox.SelectedItem = item;
                        _currentLocation = location;
                    }
                }

                // Fallback selection
                if (LocationComboBox.SelectedItem == null && LocationComboBox.Items.Any())
                {
                    LocationComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateLocationComboBox error: {ex.Message}");
            }
        }

        private void UpdateLocationDescription()
        {
            try
            {
                if (LocationDescriptionTextBlock == null || _currentLocation == null) return;

                LocationDescriptionTextBlock.Text = $"{_currentLocation.Description} " +
                    $"(Danger Level: {_currentLocation.DangerLevel}/5, Depth: {_currentLocation.Depth}m)";

                if (CurrentLocationTextBlock != null)
                    CurrentLocationTextBlock.Text = _currentLocation.Name;

                if (WeatherTextBlock != null)
                    WeatherTextBlock.Text = _currentLocation.CurrentWeather.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateLocationDescription error: {ex.Message}");
            }
        }

        private void InitializeGameLog()
        {
            try
            {
                if (GameLogPanel == null) return;

                AddLogEntry("🌟 Welcome to Mine & Refine Ultimate Edition!");
                AddLogEntry($"👤 Logged in as: {CURRENT_USER}");
                AddLogEntry($"🎯 Current Rank: {_currentPlayer?.Rank.ToString().Replace("_", " ")}");
                AddLogEntry($"📍 Location: {_currentLocation?.Name ?? "Unknown"}");
                AddLogEntry($"⚡ Systems initialized at {CURRENT_DATETIME} UTC");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeGameLog error: {ex.Message}");
            }
        }

        private void ShowWelcomeTab()
        {
            try
            {
                // Show mining tab by default
                SwitchToTab("Mine");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowWelcomeTab error: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_isInitialized || LocationComboBox?.SelectedItem is not ComboBoxItem selectedItem)
                    return;

                var locationId = selectedItem.Tag?.ToString();
                if (string.IsNullOrEmpty(locationId))
                    return;

                _currentLocation = _locations.FirstOrDefault(l => l.Id == locationId);
                if (_currentLocation != null && _currentPlayer != null)
                {
                    _currentPlayer.CurrentLocationId = locationId;
                    UpdateLocationDescription();
                    AddLogEntry($"📍 Moved to: {_currentLocation.Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LocationComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private void RiskSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (!_isInitialized || RiskSlider == null || RiskDescriptionTextBlock == null)
                    return;

                _currentRiskMultiplier = RiskSlider.Value;
                if (_currentPlayer != null)
                {
                    _currentPlayer.Multiplier = _currentRiskMultiplier;
                }

                var riskDescription = _currentRiskMultiplier switch
                {
                    <= 1.2 => "Conservative Protocol (Low Risk, Standard Rewards)",
                    <= 1.6 => "Balanced Protocol (Moderate Risk, Good Rewards)",
                    <= 2.0 => "Aggressive Protocol (High Risk, Enhanced Rewards)",
                    <= 2.5 => "Extreme Protocol (Very High Risk, Premium Rewards)",
                    _ => "Quantum Protocol (Maximum Risk, Legendary Rewards)"
                };

                RiskDescriptionTextBlock.Text = $"{riskDescription} ({_currentRiskMultiplier:F1}x)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RiskSlider_ValueChanged error: {ex.Message}");
            }
        }

        private async void MineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _currentPlayer == null || _currentLocation == null || _gameService == null)
                {
                    AddLogEntry("❌ System not ready for mining operations");
                    return;
                }

                if (_currentPlayer.Stamina < 10)
                {
                    AddLogEntry("😴 Insufficient energy for mining. Please rest first.");
                    return;
                }

                ShowLoading("Mining in progress...");

                var result = await _gameService.PerformMiningOperationAsync(_currentPlayer, _currentLocation, _currentRiskMultiplier);

                HideLoading();

                if (result.IsSuccess && result.Mineral != null)
                {
                    var message = $"⛏️ {result.Mineral.GetTypeEmoji()} Found {result.Mineral.Name}! Value: £{result.Value:N0}";
                    if (!string.IsNullOrEmpty(result.BonusDiscovery))
                    {
                        message += $" ✨ {result.BonusDiscovery}";
                    }
                    AddLogEntry(message);

                    ShowNotification($"{result.Mineral.GetTypeEmoji()} {result.Mineral.Name} Discovered!",
                                   $"Extracted valuable {result.Mineral.Name} worth £{result.Value:N0}!");
                }
                else
                {
                    AddLogEntry($"❌ Mining failed: {result.Message}");
                }

                UpdatePlayerDisplay();
            }
            catch (Exception ex)
            {
                HideLoading();
                AddLogEntry($"💥 Mining error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"MineButton_Click error: {ex.Message}");
            }
        }

        private void RestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _currentPlayer == null) return;

                var restAmount = Math.Min(20, _currentPlayer.MaxStamina - _currentPlayer.Stamina);
                _currentPlayer.Stamina += restAmount;

                AddLogEntry($"😴 Rested and recovered {restAmount} energy");
                UpdatePlayerDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RestButton_Click error: {ex.Message}");
            }
        }

        private void QuickStatsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _currentPlayer == null) return;

                AddLogEntry("📊 === PLAYER STATISTICS ===");
                AddLogEntry($"💰 Money: £{_currentPlayer.TotalMoney:N0} | Lifetime Earnings: £{_currentPlayer.TotalEarnings:N0}");
                AddLogEntry($"⛏️ Total Mines: {_currentPlayer.TotalMinesCount:N0} | Success Rate: {_currentPlayer.GetSuccessRate():F1}%");
                AddLogEntry($"🏆 Mining Streak: {_currentPlayer.GetMiningStreak()} | Best Value: £{_currentPlayer.SingleBestMineValue:N0}");
                AddLogEntry($"⚡ Energy: {_currentPlayer.Stamina}/{_currentPlayer.MaxStamina} | Skill Points: {_currentPlayer.SkillPoints}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QuickStatsButton_Click error: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameLogPanel == null) return;

                GameLogPanel.Children.Clear();
                AddLogEntry($"🧹 Log cleared at {DateTime.Parse(CURRENT_DATETIME):HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearLogButton_Click error: {ex.Message}");
            }
        }

        // Navigation handlers
        private void MineTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Mine");
        private void LocationsTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Locations");
        private void SkillsTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Skills");
        private void AchievementsTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Achievements");
        private void MarketTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Market");
        private void MenuTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Menu");

        // Quick action handlers
        private async void StartNewGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var setupWindow = new PlayerSetupWindow();
                setupWindow.Activate();

                // You could handle the result here if needed
                await Task.Delay(100); // Small delay for window activation
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartNewGameButton_Click error: {ex.Message}");
            }
        }

        private void LoadExistingGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogEntry("📁 Load game functionality coming soon...");
        }

        private void UpgradesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPlayer == null || _gameService == null) return;

                var upgradesDialog = new UltimateUpgradesDialog(_currentPlayer, _gameService);
                upgradesDialog.XamlRoot = this.Content.XamlRoot;
                _ = upgradesDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpgradesButton_Click error: {ex.Message}");
            }
        }

        private void ChallengesButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogEntry("🎯 Challenges system coming soon...");
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            HideNotification();
        }

        #endregion

        #region Utility Methods

        private void SwitchToTab(string tabName)
        {
            try
            {
                // Hide all content panels
                if (MineContent != null) MineContent.Visibility = Visibility.Collapsed;
                if (LocationsContent != null) LocationsContent.Visibility = Visibility.Collapsed;
                if (SkillsContent != null) SkillsContent.Visibility = Visibility.Collapsed;
                if (AchievementsContent != null) AchievementsContent.Visibility = Visibility.Collapsed;
                if (MarketContent != null) MarketContent.Visibility = Visibility.Collapsed;
                if (MenuContent != null) MenuContent.Visibility = Visibility.Collapsed;

                // Show selected tab
                switch (tabName)
                {
                    case "Mine":
                        if (MineContent != null) MineContent.Visibility = Visibility.Visible;
                        break;
                    case "Locations":
                        if (LocationsContent != null) LocationsContent.Visibility = Visibility.Visible;
                        break;
                    case "Skills":
                        if (SkillsContent != null) SkillsContent.Visibility = Visibility.Visible;
                        break;
                    case "Achievements":
                        if (AchievementsContent != null) AchievementsContent.Visibility = Visibility.Visible;
                        break;
                    case "Market":
                        if (MarketContent != null) MarketContent.Visibility = Visibility.Visible;
                        break;
                    case "Menu":
                        if (MenuContent != null) MenuContent.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SwitchToTab error: {ex.Message}");
            }
        }

        private void AddLogEntry(string message)
        {
            try
            {
                if (GameLogPanel == null) return;

                var timestamp = DateTime.Parse(CURRENT_DATETIME).ToString("HH:mm:ss");
                var logEntry = new TextBlock
                {
                    Text = $"[{timestamp}] {message}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 1, 0, 1)
                };

                GameLogPanel.Children.Add(logEntry);

                // Keep log manageable
                while (GameLogPanel.Children.Count > 50)
                {
                    GameLogPanel.Children.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLogEntry error: {ex.Message}");
            }
        }

        private void ShowLoading(string message = "Loading...")
        {
            try
            {
                if (LoadingOverlay != null) LoadingOverlay.Visibility = Visibility.Visible;
                if (LoadingTitle != null) LoadingTitle.Text = "Loading...";
                if (LoadingMessage != null) LoadingMessage.Text = message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowLoading error: {ex.Message}");
            }
        }

        private void HideLoading()
        {
            try
            {
                if (LoadingOverlay != null) LoadingOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HideLoading error: {ex.Message}");
            }
        }

        private void ShowNotification(string title, string message)
        {
            try
            {
                if (NotificationBorder != null) NotificationBorder.Visibility = Visibility.Visible;
                if (NotificationTitle != null) NotificationTitle.Text = title;
                if (NotificationMessage != null) NotificationMessage.Text = message;

                // Auto-hide after 5 seconds
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    HideNotification();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowNotification error: {ex.Message}");
            }
        }

        private void HideNotification()
        {
            try
            {
                if (NotificationBorder != null) NotificationBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HideNotification error: {ex.Message}");
            }
        }

        #endregion
    }
}