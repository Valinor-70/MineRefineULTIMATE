using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
        private readonly DataService _dataService;
        private readonly GameService _gameService;
        private readonly MarketService _marketService;
        private readonly GameSettings _gameSettings;
        
        // Phase 2 Services
        private readonly SkillsService _skillsService;
        private readonly AchievementsService _achievementsService;
        private readonly WeatherService _weatherService;

        // Game State
        private Player _currentPlayer;
        private List<MiningLocation> _locations = new();
        private MiningLocation? _currentLocation;
        private double _currentRiskMultiplier = 1.5;
        private bool _isInitialized = false;
        private bool _autoMiningActive = false;

        // Animation and UI
        private readonly Random _random = new();
        private DispatcherTimer? _autoMiningTimer;
        private DispatcherTimer? _particleTimer;
        private DispatcherTimer? _loadingTimer;
        private DispatcherTimer? _weatherTimer;
        private List<FrameworkElement> _particles = new();

        // Phase 2 UI State
        private string _currentSkillCategory = "Mining";
        private string _currentAchievementCategory = "All";
        private bool _showHiddenAchievements = false;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-07-31 13:29:22";

        public MainWindow(Player player, GameSettings gameSettings)
        {
            this.InitializeComponent();

            _currentPlayer = player;
            _gameSettings = gameSettings;
            _dataService = new DataService();
            _gameService = new GameService();
            _marketService = new MarketService();
            
            // Initialize Phase 2 services
            _skillsService = new SkillsService();
            _achievementsService = new AchievementsService();
            _weatherService = new WeatherService();
            
            // Hook up event handlers for Phase 2 features
            _achievementsService.AchievementUnlocked += OnAchievementUnlocked;
            _weatherService.WeatherChanged += OnWeatherChanged;

            this.Title = "Mine & Refine Ultimate Edition - Alpha v1.0.0";

            // Improved: Add proper cleanup event handlers
            this.Closed += MainWindow_Closed;

            // Initialize game immediately since we have player data
            _ = Task.Run(InitializeGameAsync);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            try
            {
                // Cleanup resources to prevent memory leaks
                StopAutoMining();
                
                // Stop all timers
                _particleTimer?.Stop();
                _autoMiningTimer?.Stop();
                _loadingTimer?.Stop();
                _weatherTimer?.Stop();
                
                // Clear particle system
                if (ParticleCanvas != null)
                {
                    ParticleCanvas.Children.Clear();
                    _particles.Clear();
                }

                // Save current state
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _dataService.SavePlayerAsync(_currentPlayer);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"MainWindow_Closed save error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow_Closed error: {ex.Message}");
            }
        }

        // Improved: Add keyboard navigation support for better accessibility
        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            try
            {
                args.Handled = true;
                
                switch (sender.Key)
                {
                    case Windows.System.VirtualKey.Number1:
                        SwitchToTab("Mine");
                        break;
                    case Windows.System.VirtualKey.Number2:
                        SwitchToTab("Locations");
                        break;
                    case Windows.System.VirtualKey.Number3:
                        SwitchToTab("Skills");
                        break;
                    case Windows.System.VirtualKey.Number4:
                        SwitchToTab("Achievements");
                        break;
                    case Windows.System.VirtualKey.Number5:
                        SwitchToTab("Market");
                        break;
                    case Windows.System.VirtualKey.Number6:
                        SwitchToTab("Menu");
                        break;
                    case Windows.System.VirtualKey.M when sender.Modifiers == Windows.System.VirtualKeyModifiers.Control:
                        MineButton_Click(MineButton, null);
                        break;
                    case Windows.System.VirtualKey.Space:
                        MineButton_Click(MineButton, null);
                        break;
                    case Windows.System.VirtualKey.R when sender.Modifiers == Windows.System.VirtualKeyModifiers.Control:
                        RestButton_Click(RestButton, null);
                        break;
                    default:
                        args.Handled = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"KeyboardAccelerator_Invoked error: {ex.Message}");
                args.Handled = false;
            }
        }

        #region Initialization

        private async Task InitializeGameAsync()
        {
            try
            {
                this.DispatcherQueue.TryEnqueue(() => ShowLoading("Initializing quantum mining systems..."));

                // Load game data
                _locations = _dataService.GetMiningLocations();
                _currentLocation = _locations.FirstOrDefault(l => l.Id == _currentPlayer.CurrentLocationId) ?? _locations.First();

                // Initialize UI on UI thread
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        UpdatePlayerDisplay();
                        PopulateLocationComboBox();
                        UpdateLocationDescription();
                        InitializeGameLog();
                        InitializeParticleSystem();
                        
                        // Initialize Phase 2 systems
                        InitializePhase2Systems();
                        
                        ShowWelcomeTab();
                        HideLoading();

                        _isInitialized = true;

                        ShowNotification("🚀 Systems Online", $"Welcome back, {_currentPlayer.Name}! Ready for mining operations.");

                        // Start background systems
                        StartBackgroundSystems();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UI initialization error: {ex.Message}");
                        HideLoading();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeGameAsync error: {ex.Message}");
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    HideLoading();
                    ShowNotification("⚠️ Initialization Error", $"Failed to initialize: {ex.Message}");
                });
            }
        }

        private void StartBackgroundSystems()
        {
            // Auto-save every 30 seconds
            var autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            autoSaveTimer.Tick += async (s, e) => await AutoSave();
            autoSaveTimer.Start();

            // Equipment wear simulation
            var wearTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            wearTimer.Tick += (s, e) => SimulateEquipmentWear();
            wearTimer.Start();

            // Weather updates
            var weatherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            weatherTimer.Tick += (s, e) => UpdateWeatherConditions();
            weatherTimer.Start();
        }

        #endregion

        #region Phase 1: Mining Enhancements

        private void InitializeParticleSystem()
        {
            if (!_gameSettings.AnimationSpeed.Equals(0.0) && !_gameSettings.ReducedAnimations)
            {
                _particleTimer = new DispatcherTimer
                {
                    // Improved: Reduced frequency for better performance
                    Interval = TimeSpan.FromMilliseconds(Math.Max(200, 300 / _gameSettings.AnimationSpeed))
                };
                _particleTimer.Tick += UpdateParticles;
                _particleTimer.Start();
            }
        }

        private void UpdateParticles(object? sender, object e)
        {
            try
            {
                if (ParticleCanvas == null || _gameSettings.ReducedAnimations) return;

                // Improved: Reduced particle count for better performance
                var maxParticles = _gameSettings.ReducedAnimations ? 5 : 10;
                
                // Create new particles during mining (throttled)
                if (_autoMiningActive && _particles.Count < maxParticles && _random.Next(0, 3) == 0)
                {
                    CreateMiningParticle();
                }

                // Update existing particles in batches
                var particlesToRemove = new List<int>();
                for (int i = 0; i < _particles.Count; i++)
                {
                    var particle = _particles[i];
                    var currentTop = Canvas.GetTop(particle);
                    var currentLeft = Canvas.GetLeft(particle);

                    // Move particle with reduced movement calculation
                    var newTop = currentTop + 3;
                    var newLeft = currentLeft + (_random.NextDouble() - 0.5) * 1.5;
                    
                    Canvas.SetTop(particle, newTop);
                    Canvas.SetLeft(particle, newLeft);

                    // Mark for removal if off-screen
                    if (newTop > ParticleCanvas.ActualHeight + 20)
                    {
                        particlesToRemove.Add(i);
                    }
                }

                // Remove particles in reverse order to maintain indices
                for (int i = particlesToRemove.Count - 1; i >= 0; i--)
                {
                    var index = particlesToRemove[i];
                    if (index < _particles.Count)
                    {
                        ParticleCanvas.Children.Remove(_particles[index]);
                        _particles.RemoveAt(index);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateParticles error: {ex.Message}");
                // Improved: Stop particle system on repeated errors
                if (_particleTimer != null && ex.Message.Contains("InvalidOperation"))
                {
                    _particleTimer.Stop();
                }
            }
        }

        private void CreateMiningParticle()
        {
            try
            {
                var particle = new Border
                {
                    Width = _random.Next(3, 8),
                    Height = _random.Next(3, 8),
                    CornerRadius = new CornerRadius(_random.Next(1, 4)),
                    Background = GetParticleColor(),
                    Opacity = 0.7
                };

                Canvas.SetLeft(particle, _random.Next(0, (int)ParticleCanvas.ActualWidth));
                Canvas.SetTop(particle, -10);

                ParticleCanvas.Children.Add(particle);
                _particles.Add(particle);

                // WinUI 3 Compatible Fade In Animation
                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 0.7,
                    Duration = TimeSpan.FromMilliseconds(300 / _gameSettings.AnimationSpeed)
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(fadeIn);
                Storyboard.SetTarget(fadeIn, particle);
                Storyboard.SetTargetProperty(fadeIn, "Opacity");
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateMiningParticle error: {ex.Message}");
            }
        }

        private SolidColorBrush GetParticleColor()
        {
            if (_currentLocation == null) return new SolidColorBrush(Colors.Gray);

            return _currentLocation.Id switch
            {
                "surface_mine" => new SolidColorBrush(Colors.Brown),
                "deep_caves" => new SolidColorBrush(Colors.Silver),
                "volcanic_depths" => new SolidColorBrush(Colors.Orange),
                "quantum_realm" => new SolidColorBrush(Colors.Purple),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        private void SimulateEquipmentWear()
        {
            try
            {
                if (_currentPlayer?.Equipment == null) return;

                foreach (var equipment in _currentPlayer.Equipment.Where(e => e.IsEquipped))
                {
                    // Reduce durability slightly over time
                    var wearRate = _currentLocation?.DangerLevel ?? 1;
                    equipment.Durability = Math.Max(0, equipment.Durability - wearRate);
                    equipment.TimesUsed++;

                    // Warn about low durability
                    if (equipment.Durability <= 20 && equipment.Durability > 0)
                    {
                        AddLogEntry($"⚠️ {equipment.Name} durability low: {equipment.Durability}/{equipment.MaxDurability}");
                    }
                    else if (equipment.Durability == 0)
                    {
                        AddLogEntry($"💥 {equipment.Name} has broken and needs repair!");
                        equipment.IsEquipped = false;
                    }
                }

                UpdatePlayerDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SimulateEquipmentWear error: {ex.Message}");
            }
        }

        private void UpdateWeatherConditions()
        {
            try
            {
                if (_currentLocation == null) return;

                // Random weather changes
                var weatherOptions = _currentLocation.Id switch
                {
                    "surface_mine" => new[] { WeatherCondition.Clear, WeatherCondition.Cloudy, WeatherCondition.Rainy, WeatherCondition.Windy },
                    "deep_caves" => new[] { WeatherCondition.Foggy, WeatherCondition.Clear, WeatherCondition.Stormy },
                    "volcanic_depths" => new[] { WeatherCondition.Clear, WeatherCondition.Stormy, WeatherCondition.Foggy },
                    "quantum_realm" => new[] { WeatherCondition.QuantumFlux, WeatherCondition.TemporalStorm, WeatherCondition.RealityDistortion },
                    _ => new[] { WeatherCondition.Clear }
                };

                var newWeather = weatherOptions[_random.Next(weatherOptions.Length)];
                if (newWeather != _currentLocation.CurrentWeather)
                {
                    _currentLocation.CurrentWeather = newWeather;
                    AddLogEntry($"🌤️ Weather changed to: {newWeather}");
                    UpdateLocationDescription();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateWeatherConditions error: {ex.Message}");
            }
        }

        private async Task AutoSave()
        {
            try
            {
                if (_currentPlayer != null)
                {
                    _currentPlayer.LastPlayed = DateTime.Parse(CURRENT_DATETIME);
                    var players = new List<Player> { _currentPlayer };
                    await _dataService.SavePlayersAsync(players);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoSave error: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Mining Operations

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

                // Show mining animation
                await PerformMiningAnimation();

                ShowLoading("Mining operation in progress...");

                var result = await _gameService.PerformMiningOperationAsync(_currentPlayer, _currentLocation, _currentRiskMultiplier);

                HideLoading();

                // Record in mining history
                var session = new MiningSession
                {
                    Id = Guid.NewGuid().ToString(),
                    PlayerId = _currentPlayer.Name,
                    LocationId = _currentLocation.Id,
                    StartTime = DateTime.Parse(CURRENT_DATETIME),
                    EndTime = DateTime.Parse(CURRENT_DATETIME).AddMinutes(1),
                    Duration = TimeSpan.FromMinutes(1),
                    RiskMultiplier = _currentRiskMultiplier,
                    StaminaCost = 10,
                    Weather = _currentLocation.CurrentWeather,
                    Results = new() { result }
                };

                _currentPlayer.MiningHistory.Add(session);

                if (result.IsSuccess && result.Mineral != null)
                {
                    var message = $"⛏️ {result.Mineral.GetTypeEmoji()} Found {result.Mineral.Name}! Value: £{result.Value:N0}";
                    if (!string.IsNullOrEmpty(result.BonusDiscovery))
                    {
                        message += $" ✨ {result.BonusDiscovery}";
                    }

                    AddLogEntry(message);
                    ShowNotification($"{result.Mineral.GetTypeEmoji()} {result.Mineral.Name} Discovered!",
                                   $"Successfully extracted {result.Mineral.Name} worth £{result.Value:N0}!");

                    // Update mineral stats
                    if (!_currentPlayer.MineralStats.ContainsKey(result.Mineral.Id))
                    {
                        _currentPlayer.MineralStats[result.Mineral.Id] = 0;
                    }
                    _currentPlayer.MineralStats[result.Mineral.Id]++;

                    // Phase 2: Track achievements for successful mining
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mining_success", result);
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mining_completed", result);
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "money_earned", result);
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mineral_found", result.Mineral.Name);
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "location_mined", _currentLocation.Id);
                    
                    // Check for quantum materials
                    if (IsQuantumMaterial(result.Mineral.Name))
                    {
                        _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "quantum_material_found", result.Mineral.Name);
                    }
                    
                    // Check for high-risk mining achievement
                    if (_currentRiskMultiplier >= 2.5)
                    {
                        _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mining_completed", _currentRiskMultiplier);
                    }
                }
                else
                {
                    AddLogEntry($"❌ Mining failed: {result.Message}");

                    // Weather impact message
                    var weatherImpact = GetWeatherImpactMessage();
                    if (!string.IsNullOrEmpty(weatherImpact))
                    {
                        AddLogEntry(weatherImpact);
                    }

                    // Phase 2: Track failure for consecutive success achievements
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mining_failure", result);
                    _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "mining_completed", result);
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

        private async Task PerformMiningAnimation()
        {
            try
            {
                if (_gameSettings.ReducedAnimations) return;

                // Create mining effect particles
                for (int i = 0; i < 10; i++)
                {
                    CreateMiningParticle();
                    await Task.Delay((int)(50 / _gameSettings.AnimationSpeed));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PerformMiningAnimation error: {ex.Message}");
            }
        }

        private string GetWeatherImpactMessage()
        {
            if (_currentLocation == null) return "";

            return _currentLocation.CurrentWeather switch
            {
                WeatherCondition.Rainy => "🌧️ Rain made the terrain slippery, affecting mining efficiency",
                WeatherCondition.Stormy => "⛈️ Storm interference disrupted equipment operation",
                WeatherCondition.Foggy => "🌫️ Poor visibility made precision mining difficult",
                WeatherCondition.QuantumFlux => "🌌 Quantum fluctuations destabilized the mining field",
                WeatherCondition.TemporalStorm => "⏰ Temporal distortions affected the mining timeline",
                WeatherCondition.RealityDistortion => "🌀 Reality warping made normal physics unreliable",
                _ => ""
            };
        }

        private void ToggleAutoMining_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_autoMiningActive)
                {
                    StopAutoMining();
                }
                else
                {
                    StartAutoMining();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToggleAutoMining_Click error: {ex.Message}");
            }
        }

        private void StartAutoMining()
        {
            try
            {
                _autoMiningActive = true;
                _autoMiningTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _autoMiningTimer.Tick += async (s, e) => await AutoMineOperation();
                _autoMiningTimer.Start();

                // Improved: Thread-safe UI update
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    if (AutoMiningButton != null)
                    {
                        AutoMiningButton.Background = new SolidColorBrush(Colors.Red);
                        AutoMiningButton.Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Children = 
                            {
                                new TextBlock { Text = "⏹️", FontSize = 14 },
                                new TextBlock { Text = "Stop Auto", FontSize = 11 }
                            }
                        };
                    }
                });

                AddLogEntry("🤖 Auto-mining activated. Mining will continue automatically while you have stamina.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartAutoMining error: {ex.Message}");
            }
        }

        private void StopAutoMining()
        {
            try
            {
                _autoMiningActive = false;
                _autoMiningTimer?.Stop();
                _autoMiningTimer = null;

                // Improved: Thread-safe UI update
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    if (AutoMiningButton != null)
                    {
                        AutoMiningButton.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x8B, 0x45, 0x13));
                        AutoMiningButton.Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Children = 
                            {
                                new TextBlock { Text = "🤖", FontSize = 14 },
                                new TextBlock { Text = "Auto-Mine", FontSize = 11 }
                            }
                        };
                    }
                });

                AddLogEntry("⏹️ Auto-mining stopped.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StopAutoMining error: {ex.Message}");
            }
        }

        private async Task AutoMineOperation()
        {
            try
            {
                if (_currentPlayer == null || _currentPlayer.Stamina < 10)
                {
                    StopAutoMining();
                    AddLogEntry("😴 Auto-mining stopped: Insufficient stamina. Rest to continue.");
                    return;
                }

                // Perform mining operation silently
                var result = await _gameService.PerformMiningOperationAsync(_currentPlayer, _currentLocation, _currentRiskMultiplier);

                // Improved: Thread-safe UI updates
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    if (result.IsSuccess && result.Mineral != null)
                    {
                        AddLogEntry($"🤖 Auto-mined: {result.Mineral.Name} (£{result.Value:N0})");

                        // Update mineral stats
                        if (!_currentPlayer.MineralStats.ContainsKey(result.Mineral.Id))
                        {
                            _currentPlayer.MineralStats[result.Mineral.Id] = 0;
                        }
                        _currentPlayer.MineralStats[result.Mineral.Id]++;
                    }
                    else
                    {
                        AddLogEntry($"🤖 Auto-mining failed: {result.Message}");
                    }

                    UpdatePlayerDisplay();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoMineOperation error: {ex.Message}");
                StopAutoMining();
                
                // Improved: Thread-safe error notification
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    ShowNotification("⚠️ Auto-Mining Error", "Auto-mining encountered an error and has been stopped.");
                });
            }
        }

        #endregion

        #region Phase 1: Locations Tab Implementation

        private void LocationsTabButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToTab("Locations");
            PopulateLocationsTab();
        }

        private void PopulateLocationsTab()
        {
            try
            {
                if (LocationsContent == null) return;

                LocationsContent.Children.Clear();

                // Header
                var headerPanel = new StackPanel { Spacing = 10, Margin = new Thickness(0, 0, 0, 20) };
                headerPanel.Children.Add(new TextBlock
                {
                    Text = "🗺️ MINING LOCATIONS",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.Gold),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                headerPanel.Children.Add(new TextBlock
                {
                    Text = "Explore different mining regions and unlock new locations through your adventures",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                LocationsContent.Children.Add(headerPanel);

                // Location Grid
                var locationsGrid = new Grid();
                locationsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                locationsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                int row = 0;
                int col = 0;

                foreach (var location in _locations)
                {
                    locationsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var locationCard = CreateLocationCard(location);
                    Grid.SetRow(locationCard, row);
                    Grid.SetColumn(locationCard, col);
                    locationsGrid.Children.Add(locationCard);

                    col++;
                    if (col > 1)
                    {
                        col = 0;
                        row++;
                    }
                }

                LocationsContent.Children.Add(locationsGrid);

                // Location Statistics
                var statsPanel = CreateLocationStatistics();
                LocationsContent.Children.Add(statsPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateLocationsTab error: {ex.Message}");
            }
        }

        private Border CreateLocationCard(MiningLocation location)
        {
            var isUnlocked = _currentPlayer.UnlockedLocations.Contains(location.Id);
            var isCurrent = location.Id == _currentPlayer.CurrentLocationId;

            var card = new Border
            {
                Background = new SolidColorBrush(isUnlocked ?
                    Color.FromArgb(40, 45, 45, 45) : Color.FromArgb(20, 100, 100, 100)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(10, 10, 10, 10),
                BorderBrush = new SolidColorBrush(isCurrent ? Colors.Gold :
                    isUnlocked ? Colors.LightGreen : Colors.Gray),
                BorderThickness = new Thickness(2, 2, 2, 2)
            };

            var content = new StackPanel { Spacing = 10 };

            // Header
            var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            header.Children.Add(new TextBlock
            {
                Text = location.Icon,
                FontSize = 32,
                VerticalAlignment = VerticalAlignment.Center
            });

            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock
            {
                Text = location.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(isUnlocked ? Colors.White : Colors.Gray)
            });

            var statusText = isCurrent ? "📍 Current Location" :
                           isUnlocked ? "✅ Unlocked" : "🔒 Locked";
            titleStack.Children.Add(new TextBlock
            {
                Text = statusText,
                FontSize = 10,
                Foreground = new SolidColorBrush(isCurrent ? Colors.Gold :
                           isUnlocked ? Colors.LightGreen : Colors.Red)
            });

            header.Children.Add(titleStack);
            content.Children.Add(header);

            // Description
            content.Children.Add(new TextBlock
            {
                Text = location.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap
            });

            // Stats
            if (isUnlocked)
            {
                var statsGrid = new Grid();
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var leftStats = new StackPanel { Spacing = 3 };
                leftStats.Children.Add(new TextBlock
                {
                    Text = $"Danger: {location.DangerLevel}/5",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Orange)
                });
                leftStats.Children.Add(new TextBlock
                {
                    Text = $"Depth: {location.Depth}m",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.LightBlue)
                });
                leftStats.Children.Add(new TextBlock
                {
                    Text = $"Weather: {location.CurrentWeather}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.LightGreen)
                });

                var rightStats = new StackPanel { Spacing = 3 };
                rightStats.Children.Add(new TextBlock
                {
                    Text = $"Visits: {location.TimesVisited}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White)
                });
                rightStats.Children.Add(new TextBlock
                {
                    Text = $"Stamina: {location.StaminaCost}x",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Yellow)
                });

                Grid.SetColumn(leftStats, 0);
                Grid.SetColumn(rightStats, 1);
                statsGrid.Children.Add(leftStats);
                statsGrid.Children.Add(rightStats);
                content.Children.Add(statsGrid);

                // Action Button
                if (!isCurrent)
                {
                    var travelButton = new Button
                    {
                        Content = "🚀 Fast Travel",
                        Background = new SolidColorBrush(Colors.DarkGreen),
                        Foreground = new SolidColorBrush(Colors.White),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(10, 5, 10, 5),
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = location.Id
                    };
                    travelButton.Click += FastTravelButton_Click;
                    content.Children.Add(travelButton);
                }
            }
            else
            {
                // Unlock requirements
                var reqPanel = new StackPanel { Spacing = 5 };
                reqPanel.Children.Add(new TextBlock
                {
                    Text = "🔓 Unlock Requirements:",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.Orange)
                });
                reqPanel.Children.Add(new TextBlock
                {
                    Text = $"• Rank: {location.RequiredRank}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.LightGray)
                });
                reqPanel.Children.Add(new TextBlock
                {
                    Text = $"• Level: {location.RequiredLevel}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.LightGray)
                });
                reqPanel.Children.Add(new TextBlock
                {
                    Text = $"• Cost: £{location.UnlockCost:N0}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gold)
                });

                content.Children.Add(reqPanel);

                // Unlock button if eligible
                if (CanUnlockLocation(location))
                {
                    var unlockButton = new Button
                    {
                        Content = $"🔓 Unlock (£{location.UnlockCost:N0})",
                        Background = new SolidColorBrush(Colors.DarkBlue),
                        Foreground = new SolidColorBrush(Colors.White),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(10, 5, 10, 5),
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = location.Id
                    };
                    unlockButton.Click += UnlockLocationButton_Click;
                    content.Children.Add(unlockButton);
                }
            }

            card.Child = content;
            return card;
        }

        private StackPanel CreateLocationStatistics()
        {
            var statsPanel = new StackPanel { Spacing = 15, Margin = new Thickness(0, 20, 0, 0) };

            var header = new TextBlock
            {
                Text = "📊 Mining Statistics",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Gold),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            statsPanel.Children.Add(header);

            // Overall stats
            var overallGrid = new Grid();
            overallGrid.ColumnDefinitions.Add(new ColumnDefinition());
            overallGrid.ColumnDefinitions.Add(new ColumnDefinition());
            overallGrid.ColumnDefinitions.Add(new ColumnDefinition());

            var totalMines = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            totalMines.Children.Add(new TextBlock
            {
                Text = _currentPlayer.TotalMinesCount.ToString("N0"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            totalMines.Children.Add(new TextBlock
            {
                Text = "Total Mines",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var successRate = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            successRate.Children.Add(new TextBlock
            {
                Text = $"{_currentPlayer.GetSuccessRate():F1}%",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.LightGreen),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            successRate.Children.Add(new TextBlock
            {
                Text = "Success Rate",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var totalValue = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            totalValue.Children.Add(new TextBlock
            {
                Text = $"£{_currentPlayer.TotalEarnings / 1000000.0:F1}M",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Gold),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            totalValue.Children.Add(new TextBlock
            {
                Text = "Total Earnings",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            Grid.SetColumn(totalMines, 0);
            Grid.SetColumn(successRate, 1);
            Grid.SetColumn(totalValue, 2);
            overallGrid.Children.Add(totalMines);
            overallGrid.Children.Add(successRate);
            overallGrid.Children.Add(totalValue);

            statsPanel.Children.Add(overallGrid);

            return statsPanel;
        }

        private bool CanUnlockLocation(MiningLocation location)
        {
            if (_currentPlayer.Level < location.RequiredLevel)
                return false;

            if (_currentPlayer.TotalMoney < location.UnlockCost)
                return false;

            // Check rank requirement (simplified)
            return true;
        }

        private void FastTravelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var locationId = button?.Tag as string;

                if (string.IsNullOrEmpty(locationId)) return;

                var location = _locations.FirstOrDefault(l => l.Id == locationId);
                if (location != null)
                {
                    _currentPlayer.CurrentLocationId = locationId;
                    _currentLocation = location;
                    location.TimesVisited++;
                    location.LastVisited = DateTime.Parse("2025-07-31 13:29:22");

                    AddLogEntry($"🚀 Fast traveled to: {location.Name}");
                    UpdateLocationDescription();
                    PopulateLocationComboBox();
                    PopulateLocationsTab(); // Refresh the tab
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FastTravelButton_Click error: {ex.Message}");
            }
        }

        private void UnlockLocationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var locationId = button?.Tag as string;

                if (string.IsNullOrEmpty(locationId)) return;

                var location = _locations.FirstOrDefault(l => l.Id == locationId);
                if (location != null && CanUnlockLocation(location))
                {
                    _currentPlayer.TotalMoney -= location.UnlockCost;
                    _currentPlayer.UnlockedLocations.Add(locationId);
                    location.IsUnlocked = true;

                    AddLogEntry($"🔓 Unlocked new location: {location.Name}!");
                    ShowNotification("🗺️ Location Unlocked!", $"You can now mine at {location.Name}");

                    UpdatePlayerDisplay();
                    PopulateLocationComboBox();
                    PopulateLocationsTab(); // Refresh the tab
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnlockLocationButton_Click error: {ex.Message}");
            }
        }

        #endregion

        #region Existing Methods (Updated for Phase 1)

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

                // Update progress bars with equipment effects
                if (StaminaProgressBar != null)
                {
                    StaminaProgressBar.Value = _currentPlayer.Stamina;
                    StaminaProgressBar.Maximum = _currentPlayer.MaxStamina;

                    // Color based on stamina level
                    if (_currentPlayer.Stamina < 20)
                        StaminaProgressBar.Foreground = new SolidColorBrush(Colors.Red);
                    else if (_currentPlayer.Stamina < 50)
                        StaminaProgressBar.Foreground = new SolidColorBrush(Colors.Orange);
                    else
                        StaminaProgressBar.Foreground = new SolidColorBrush(Colors.LightGreen);
                }

                if (ExperienceProgressBar != null)
                {
                    var progressPercent = (_currentPlayer.ExperiencePoints % 1000) / 10.0;
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
                    var weatherIcon = GetWeatherIcon(location.CurrentWeather);
                    var item = new ComboBoxItem
                    {
                        Content = $"{location.Icon} {location.Name} {weatherIcon}",
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

        private string GetWeatherIcon(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Clear => "☀️",
                WeatherCondition.Cloudy => "☁️",
                WeatherCondition.Rainy => "🌧️",
                WeatherCondition.Stormy => "⛈️",
                WeatherCondition.Foggy => "🌫️",
                WeatherCondition.Snowy => "❄️",
                WeatherCondition.Windy => "💨",
                WeatherCondition.QuantumFlux => "🌌",
                WeatherCondition.TemporalStorm => "⏰",
                WeatherCondition.RealityDistortion => "🌀",
                WeatherCondition.CosmicRadiation => "☢️",
                WeatherCondition.DimensionalRift => "🌀",
                _ => ""
            };
        }

        private void UpdateLocationDescription()
        {
            try
            {
                if (LocationDescriptionTextBlock == null || _currentLocation == null) return;

                var weatherEffect = GetWeatherEffectDescription(_currentLocation.CurrentWeather);
                var description = $"{_currentLocation.Description} " +
                    $"(Danger: {_currentLocation.DangerLevel}/5, Depth: {_currentLocation.Depth}m)";

                if (!string.IsNullOrEmpty(weatherEffect))
                {
                    description += $"\n🌤️ {weatherEffect}";
                }

                LocationDescriptionTextBlock.Text = description;

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

        private string GetWeatherEffectDescription(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Clear => "Perfect mining conditions",
                WeatherCondition.Rainy => "Wet conditions reduce efficiency by 10%",
                WeatherCondition.Stormy => "Dangerous storms reduce success rate by 15%",
                WeatherCondition.Foggy => "Limited visibility affects precision by 5%",
                WeatherCondition.QuantumFlux => "Reality fluctuations create unpredictable results",
                WeatherCondition.TemporalStorm => "Time distortions may duplicate or erase mining attempts",
                WeatherCondition.RealityDistortion => "Physics laws are unreliable - expect the impossible",
                _ => ""
            };
        }

        private void InitializeGameLog()
        {
            try
            {
                if (GameLogPanel == null) return;

                AddLogEntry("🌟 Welcome to Mine & Refine Ultimate Edition!");
                AddLogEntry($"👤 Logged in as: {_currentPlayer.Name}");
                AddLogEntry($"🎯 Current Rank: {_currentPlayer.Rank.ToString().Replace("_", " ")}");
                AddLogEntry($"📍 Location: {_currentLocation?.Name ?? "Unknown"}");
                AddLogEntry($"🌤️ Weather: {_currentLocation?.CurrentWeather}");
                AddLogEntry($"⚡ Systems initialized at 2025-07-31 13:29:22 UTC");
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
                    _currentLocation.TimesVisited++;
                    _currentLocation.LastVisited = DateTime.Parse("2025-07-31 13:29:22");

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

        private void RestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _currentPlayer == null) return;

                var restAmount = Math.Min(25, _currentPlayer.MaxStamina - _currentPlayer.Stamina);
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

                AddLogEntry("📊 === MINING STATISTICS ===");
                AddLogEntry($"💰 Funds: £{_currentPlayer.TotalMoney:N0} | Lifetime: £{_currentPlayer.TotalEarnings:N0}");
                AddLogEntry($"⛏️ Total Mines: {_currentPlayer.TotalMinesCount:N0} | Success: {_currentPlayer.GetSuccessRate():F1}%");
                AddLogEntry($"🏆 Current Streak: {_currentPlayer.GetMiningStreak()} | Best: £{_currentPlayer.SingleBestMineValue:N0}");
                AddLogEntry($"⚡ Energy: {_currentPlayer.Stamina}/{_currentPlayer.MaxStamina} | Skills: {_currentPlayer.SkillPoints}");
                AddLogEntry($"🌍 Location: {_currentLocation?.Name} | Weather: {_currentLocation?.CurrentWeather}");

                // Equipment status
                var equippedItems = _currentPlayer.Equipment?.Where(e => e.IsEquipped).ToList() ?? new List<Equipment>();
                if (equippedItems.Any())
                {
                    AddLogEntry($"⚒️ Equipment: {string.Join(", ", equippedItems.Select(e => $"{e.Name} ({e.Durability}/{e.MaxDurability})"))}");
                }
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
                AddLogEntry($"🧹 Mining log cleared at {DateTime.Parse("2025-07-31 13:29:22"):HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearLogButton_Click error: {ex.Message}");
            }
        }

        // Navigation handlers - Updated for TabView
        private void MineTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Mine");
        private void SkillsTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Skills");
        private void AchievementsTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Achievements");
        private void MarketTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Market");
        private void MenuTabButton_Click(object sender, RoutedEventArgs e) => SwitchToTab("Menu");

        // Quick action handlers
        private void StartNewGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogEntry("🚀 New game creation handled at startup through settings window.");
        }

        private void LoadExistingGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogEntry("📁 Game loading handled at startup through settings window.");
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
            AddLogEntry("🎯 Daily challenges system coming in Phase 2...");
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            HideNotification();
        }

        #endregion

        #region Utility Methods (Enhanced for Phase 1)

        // Enhanced TabView navigation handler
        private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MainTabView?.SelectedItem is TabViewItem selectedTab)
                {
                    // Handle tab-specific logic based on selected tab
                    var tabName = selectedTab.Name?.Replace("TabItem", "") ?? "";
                    
                    switch (tabName)
                    {
                        case "Mine":
                            // Mine tab is always visible, no special action needed
                            break;
                        case "Locations":
                            PopulateLocationsTab();
                            break;
                        case "Skills":
                        case "Achievements":
                        case "Market":
                        case "Menu":
                            // These tabs have static content in the XAML
                            break;
                    }
                    
                    AddLogEntry($"🗂️ Switched to {tabName} tab");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainTabView_SelectionChanged error: {ex.Message}");
            }
        }

        // Legacy tab button handlers (for compatibility)
        private void LocationsTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabView != null) MainTabView.SelectedItem = LocationsTabItem;
        }

        // Simplified navigation - no longer needed with TabView
        private void SwitchToTab(string tabName)
        {
            try
            {
                switch (tabName)
                {
                    case "Mine":
                        if (MainTabView != null) MainTabView.SelectedItem = MineTabItem;
                        break;
                    case "Locations":
                        if (MainTabView != null) MainTabView.SelectedItem = LocationsTabItem;
                        break;
                    case "Skills":
                        if (MainTabView != null) MainTabView.SelectedItem = SkillsTabItem;
                        break;
                    case "Achievements":
                        if (MainTabView != null) MainTabView.SelectedItem = AchievementsTabItem;
                        break;
                    case "Market":
                        if (MainTabView != null) MainTabView.SelectedItem = MarketTabItem;
                        break;
                    case "Menu":
                        if (MainTabView != null) MainTabView.SelectedItem = MenuTabItem;
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

                var timestamp = DateTime.Parse("2025-07-31 13:29:22").ToString("HH:mm:ss");
                var logEntry = new TextBlock
                {
                    Text = $"[{timestamp}] {message}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 1, 0, 1)
                };

                GameLogPanel.Children.Add(logEntry);

                // Keep log manageable - increased limit for better history
                while (GameLogPanel.Children.Count > 100)
                {
                    GameLogPanel.Children.RemoveAt(0);
                }

                // Auto-scroll to bottom using ScrollViewer parent
                var scrollViewer = FindParentScrollViewer(GameLogPanel);
                if (scrollViewer != null)
                {
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLogEntry error: {ex.Message}");
            }
        }

        private ScrollViewer? FindParentScrollViewer(FrameworkElement element)
        {
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is ScrollViewer scrollViewer)
                    return scrollViewer;
                parent = (parent as FrameworkElement)?.Parent;
            }
            return null;
        }

        private void ShowLoading(string message = "Loading...")
        {
            try
            {
                if (LoadingOverlay != null) LoadingOverlay.Visibility = Visibility.Visible;
                if (LoadingTitle != null) LoadingTitle.Text = "Processing...";
                if (LoadingMessage != null) LoadingMessage.Text = message;

                // Improved: Add timeout protection to prevent infinite loading
                _loadingTimer?.Stop();
                _loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) }; // 30 second timeout
                _loadingTimer.Tick += (s, e) =>
                {
                    _loadingTimer.Stop();
                    HideLoading();
                    ShowNotification("⚠️ Operation Timeout", "The operation took too long and was cancelled.");
                    System.Diagnostics.Debug.WriteLine("Loading timeout reached - hiding loading overlay");
                };
                _loadingTimer.Start();
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
                // Stop timeout timer
                _loadingTimer?.Stop();
                _loadingTimer = null;

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

                // Improved: Visual feedback based on notification type
                if (NotificationBorder != null)
                {
                    if (title.Contains("⚠️") || title.Contains("Error"))
                    {
                        NotificationBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                        NotificationBorder.Background = new SolidColorBrush(Color.FromArgb(0xDD, 0x4A, 0x1A, 0x1A));
                    }
                    else if (title.Contains("✅") || title.Contains("Success"))
                    {
                        NotificationBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                        NotificationBorder.Background = new SolidColorBrush(Color.FromArgb(0xDD, 0x1A, 0x4A, 0x1A));
                    }
                    else if (title.Contains("⚡") || title.Contains("Auto-Mining"))
                    {
                        NotificationBorder.BorderBrush = new SolidColorBrush(Colors.Orange);
                        NotificationBorder.Background = new SolidColorBrush(Color.FromArgb(0xDD, 0x4A, 0x3A, 0x1A));
                    }
                    else
                    {
                        // Default styling
                        NotificationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xD7, 0x00));
                        NotificationBorder.Background = new SolidColorBrush(Color.FromArgb(0xDD, 0x2D, 0x2D, 0x30));
                    }
                }

                // Auto-hide after 6 seconds (or longer for errors)
                var hideDelay = title.Contains("⚠️") || title.Contains("Error") ? 10 : 6;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(hideDelay) };
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

        #region Phase 2: Skills System

        private void SkillCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string category)
            {
                _currentSkillCategory = category;
                UpdateSkillCategoryButtons();
                LoadSkillsForCategory(category);
            }
        }

        private void UpdateSkillCategoryButtons()
        {
            // Update button styles to show active category
            var buttons = new[] { MiningSkillsCategoryButton, EquipmentSkillsCategoryButton, ExplorationSkillsCategoryButton, EconomicSkillsCategoryButton, QuantumSkillsCategoryButton };
            
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    if (button.Tag?.ToString() == _currentSkillCategory)
                    {
                        button.Opacity = 1.0;
                        button.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        button.Opacity = 0.7;
                        button.BorderThickness = new Thickness(2);
                    }
                }
            }
        }

        private void LoadSkillsForCategory(string category)
        {
            try
            {
                if (SkillTreeGrid == null) return;

                SkillTreeGrid.Children.Clear();
                SkillTreeGrid.RowDefinitions.Clear();
                SkillTreeGrid.ColumnDefinitions.Clear();

                if (!Enum.TryParse<SkillCategory>(category, out var skillCategory)) return;

                var skills = _skillsService.GetSkillsByCategory(skillCategory);
                if (!skills.Any()) return;

                // Create grid layout for skills
                var rows = (int)Math.Ceiling(skills.Count / 3.0);
                for (int i = 0; i < rows; i++)
                {
                    SkillTreeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                for (int i = 0; i < 3; i++)
                {
                    SkillTreeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Add skills to grid
                for (int i = 0; i < skills.Count; i++)
                {
                    var skill = skills[i];
                    var skillCard = CreateSkillCard(skill);
                    
                    Grid.SetRow(skillCard, i / 3);
                    Grid.SetColumn(skillCard, i % 3);
                    SkillTreeGrid.Children.Add(skillCard);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSkillsForCategory error: {ex.Message}");
            }
        }

        private Border CreateSkillCard(Skill skill)
        {
            var isUnlocked = _skillsService.GetSkillLevel(_currentPlayer, skill.Id) > 0;
            var canUnlock = _skillsService.CanUnlockSkill(_currentPlayer, skill.Id);
            var canUpgrade = _skillsService.CanUpgradeSkill(_currentPlayer, skill.Id);
            var currentLevel = _skillsService.GetSkillLevel(_currentPlayer, skill.Id);

            var card = new Border
            {
                Style = this.Resources["CardStyle"] as Style,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(isUnlocked ? Colors.DarkGreen : (canUnlock ? Colors.DarkOrange : Colors.DarkGray))
            };

            var stackPanel = new StackPanel { Spacing = 8 };

            // Header
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            headerPanel.Children.Add(new TextBlock { Text = skill.Icon, FontSize = 20 });
            headerPanel.Children.Add(new TextBlock { Text = skill.Name, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.White) });
            stackPanel.Children.Add(headerPanel);

            // Level info
            if (isUnlocked)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Level {currentLevel}/{skill.MaxLevel}", 
                    FontSize = 12, 
                    Foreground = new SolidColorBrush(Colors.LightBlue) 
                });
            }

            // Description
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = skill.Description, 
                FontSize = 11, 
                Foreground = new SolidColorBrush(Colors.LightGray), 
                TextWrapping = TextWrapping.Wrap 
            });

            // Cost
            if (!isUnlocked && canUnlock)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Cost: {skill.SkillPointCost} SP", 
                    FontSize = 11, 
                    Foreground = new SolidColorBrush(Colors.Gold) 
                });
            }
            else if (canUpgrade)
            {
                var upgradeCost = skill.SkillPointCost * (currentLevel + 1);
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Upgrade Cost: {upgradeCost} SP", 
                    FontSize = 11, 
                    Foreground = new SolidColorBrush(Colors.Gold) 
                });
            }

            // Action button
            if (canUnlock || canUpgrade)
            {
                var button = new Button
                {
                    Content = canUnlock ? "Unlock" : "Upgrade",
                    Background = new SolidColorBrush(Colors.Green),
                    Foreground = new SolidColorBrush(Colors.White),
                    Padding = new Thickness(8, 4, 8, 4),
                    CornerRadius = new CornerRadius(4),
                    Tag = skill.Id
                };
                button.Click += SkillActionButton_Click;
                stackPanel.Children.Add(button);
            }

            card.Child = stackPanel;
            return card;
        }

        private void SkillActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string skillId)
            {
                var canUnlock = _skillsService.CanUnlockSkill(_currentPlayer, skillId);
                var canUpgrade = _skillsService.CanUpgradeSkill(_currentPlayer, skillId);

                if (canUnlock)
                {
                    if (_skillsService.UnlockSkill(_currentPlayer, skillId))
                    {
                        ShowNotification("Skill Unlocked!", $"You've unlocked a new skill!", NotificationType.Success);
                        _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "skill_purchased", skillId);
                        UpdateSkillsUI();
                        UpdatePlayerUI();
                    }
                }
                else if (canUpgrade)
                {
                    if (_skillsService.UpgradeSkill(_currentPlayer, skillId))
                    {
                        ShowNotification("Skill Upgraded!", $"Skill has been upgraded!", NotificationType.Success);
                        _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "skill_upgraded", skillId);
                        UpdateSkillsUI();
                        UpdatePlayerUI();
                    }
                }
            }
        }

        private void UpdateSkillsUI()
        {
            try
            {
                // Update skills stats
                if (SkillsStatsTextBlock != null)
                {
                    var totalSkills = _skillsService.GetTotalSkillsUnlocked(_currentPlayer);
                    SkillsStatsTextBlock.Text = $"Available Skill Points: {_currentPlayer.SkillPoints} | Total Skills: {totalSkills}/25";
                }

                // Update active skills panel
                UpdateActiveSkillsPanel();

                // Reload current category
                LoadSkillsForCategory(_currentSkillCategory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSkillsUI error: {ex.Message}");
            }
        }

        private void UpdateActiveSkillsPanel()
        {
            try
            {
                if (ActiveSkillsPanel == null) return;

                ActiveSkillsPanel.Children.Clear();
                var bonuses = _skillsService.GetAllSkillBonuses(_currentPlayer);

                if (!bonuses.Any())
                {
                    ActiveSkillsPanel.Children.Add(new TextBlock 
                    { 
                        Text = "No active skill bonuses", 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.Gray) 
                    });
                    return;
                }

                foreach (var bonus in bonuses)
                {
                    var bonusPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
                    bonusPanel.Children.Add(new TextBlock 
                    { 
                        Text = FormatBonusName(bonus.Key), 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.White) 
                    });
                    bonusPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"+{bonus.Value:P0}", 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.LightGreen) 
                    });
                    ActiveSkillsPanel.Children.Add(bonusPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateActiveSkillsPanel error: {ex.Message}");
            }
        }

        private string FormatBonusName(string bonusKey)
        {
            return bonusKey switch
            {
                "mining_efficiency" => "⛏️ Mining Efficiency",
                "max_stamina" => "💪 Max Stamina",
                "critical_chance" => "💥 Critical Chance",
                "rare_mineral_chance" => "💎 Rare Find Rate",
                "safety_bonus" => "🛡️ Safety Bonus",
                "equipment_bonus" => "⚒️ Equipment Bonus",
                "stamina_efficiency" => "⚡ Stamina Efficiency",
                "weather_resistance" => "🌤️ Weather Resistance",
                "quantum_discovery" => "🌌 Quantum Discovery",
                _ => bonusKey.Replace("_", " ")
            };
        }

        private void ResetSkillsButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, just show a message - could implement skill reset functionality
            ShowNotification("Reset Skills", "Skill reset functionality coming soon!", NotificationType.Info);
        }

        private void SkillsHelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("Skills Help", "Skills improve your mining efficiency and unlock new abilities. Earn skill points by leveling up!", NotificationType.Info);
        }

        #endregion

        #region Phase 2: Achievements System

        private void AchievementCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string category)
            {
                _currentAchievementCategory = category;
                UpdateAchievementCategoryButtons();
                LoadAchievementsForCategory(category);
            }
        }

        private void UpdateAchievementCategoryButtons()
        {
            var buttons = new[] { AllAchievementsCategoryButton, MiningAchievementsCategoryButton, FinancialAchievementsCategoryButton, 
                                 DiscoveryAchievementsCategoryButton, ExplorationAchievementsCategoryButton, SkillsAchievementsCategoryButton, 
                                 SpecialAchievementsCategoryButton };
            
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    if (button.Tag?.ToString() == _currentAchievementCategory)
                    {
                        button.Opacity = 1.0;
                        button.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        button.Opacity = 0.7;
                        button.BorderThickness = new Thickness(2);
                    }
                }
            }
        }

        private void LoadAchievementsForCategory(string category)
        {
            try
            {
                if (AchievementsPanel == null) return;

                AchievementsPanel.Children.Clear();

                var achievements = category == "All" 
                    ? _achievementsService.GetAllAchievements().Where(a => !a.IsHidden || _showHiddenAchievements).ToList()
                    : _achievementsService.GetAchievementsByCategory(category);

                if (!achievements.Any())
                {
                    AchievementsPanel.Children.Add(new TextBlock 
                    { 
                        Text = "No achievements in this category", 
                        FontSize = 14, 
                        Foreground = new SolidColorBrush(Colors.Gray),
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                    return;
                }

                foreach (var achievement in achievements.OrderByDescending(a => a.IsCompleted).ThenBy(a => a.Name))
                {
                    var achievementCard = CreateAchievementCard(achievement);
                    AchievementsPanel.Children.Add(achievementCard);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAchievementsForCategory error: {ex.Message}");
            }
        }

        private Border CreateAchievementCard(Achievement achievement)
        {
            var card = new Border
            {
                Style = this.Resources["CardStyle"] as Style,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(achievement.IsCompleted ? Colors.DarkGreen : Colors.DarkSlateGray)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            iconPanel.Children.Add(new TextBlock { Text = achievement.Icon, FontSize = 24 });
            if (achievement.IsCompleted)
            {
                iconPanel.Children.Add(new TextBlock { Text = "✅", FontSize = 16 });
            }
            Grid.SetColumn(iconPanel, 0);

            // Content
            var contentPanel = new StackPanel { Spacing = 5, Margin = new Thickness(15, 0, 15, 0) };
            
            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            titlePanel.Children.Add(new TextBlock 
            { 
                Text = achievement.Name, 
                FontSize = 14, 
                FontWeight = FontWeights.Bold, 
                Foreground = new SolidColorBrush(achievement.IsCompleted ? Colors.Gold : Colors.White) 
            });
            
            if (achievement.IsHidden && !achievement.IsCompleted)
            {
                titlePanel.Children.Add(new TextBlock { Text = "🔒", FontSize = 12 });
            }
            
            contentPanel.Children.Add(titlePanel);
            
            contentPanel.Children.Add(new TextBlock 
            { 
                Text = achievement.Description, 
                FontSize = 12, 
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap
            });

            // Progress
            if (!achievement.IsCompleted && achievement.RequiredValue > 0)
            {
                var progressText = $"Progress: {achievement.CurrentProgress}/{achievement.RequiredValue}";
                contentPanel.Children.Add(new TextBlock 
                { 
                    Text = progressText, 
                    FontSize = 11, 
                    Foreground = new SolidColorBrush(Colors.LightBlue) 
                });

                var progressBar = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = achievement.RequiredValue,
                    Value = achievement.CurrentProgress,
                    Height = 6,
                    Foreground = new SolidColorBrush(Colors.Gold)
                };
                contentPanel.Children.Add(progressBar);
            }

            Grid.SetColumn(contentPanel, 1);

            // Points
            var pointsPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            pointsPanel.Children.Add(new TextBlock 
            { 
                Text = $"{achievement.PointValue}", 
                FontSize = 16, 
                FontWeight = FontWeights.Bold, 
                Foreground = new SolidColorBrush(Colors.Gold),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            pointsPanel.Children.Add(new TextBlock 
            { 
                Text = "pts", 
                FontSize = 10, 
                Foreground = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            Grid.SetColumn(pointsPanel, 2);

            grid.Children.Add(iconPanel);
            grid.Children.Add(contentPanel);
            grid.Children.Add(pointsPanel);

            card.Child = grid;
            return card;
        }

        private void UpdateAchievementsUI()
        {
            try
            {
                // Update achievements stats
                if (AchievementsStatsTextBlock != null)
                {
                    var completed = _achievementsService.GetCompletedAchievements().Count;
                    var total = _achievementsService.GetAllAchievements().Count(a => !a.IsHidden);
                    var percentage = total > 0 ? (double)completed / total * 100 : 0;
                    var points = _achievementsService.GetTotalAchievementPoints();
                    AchievementsStatsTextBlock.Text = $"Progress: {completed}/{total} ({percentage:F0}%) | Points: {points}";
                }

                // Update category progress
                UpdateCategoryProgressPanel();

                // Update recent achievements
                UpdateRecentAchievementsPanel();

                // Reload current category
                LoadAchievementsForCategory(_currentAchievementCategory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateAchievementsUI error: {ex.Message}");
            }
        }

        private void UpdateCategoryProgressPanel()
        {
            try
            {
                if (CategoryProgressPanel == null) return;

                CategoryProgressPanel.Children.Clear();
                var categoryProgress = _achievementsService.GetCategoryProgress();

                foreach (var category in categoryProgress)
                {
                    var categoryPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
                    categoryPanel.Children.Add(new TextBlock 
                    { 
                        Text = category.Key, 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.White),
                        Width = 80
                    });
                    categoryPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"{category.Value}%", 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.LightGreen) 
                    });
                    CategoryProgressPanel.Children.Add(categoryPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCategoryProgressPanel error: {ex.Message}");
            }
        }

        private void UpdateRecentAchievementsPanel()
        {
            try
            {
                if (RecentAchievementsPanel == null) return;

                RecentAchievementsPanel.Children.Clear();
                var recentAchievements = _achievementsService.GetCompletedAchievements()
                    .OrderByDescending(a => a.CompletedDate)
                    .Take(5);

                if (!recentAchievements.Any())
                {
                    RecentAchievementsPanel.Children.Add(new TextBlock 
                    { 
                        Text = "No achievements yet", 
                        FontSize = 12, 
                        Foreground = new SolidColorBrush(Colors.Gray) 
                    });
                    return;
                }

                foreach (var achievement in recentAchievements)
                {
                    var achievementPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                    achievementPanel.Children.Add(new TextBlock { Text = achievement.Icon, FontSize = 14 });
                    achievementPanel.Children.Add(new TextBlock 
                    { 
                        Text = achievement.Name, 
                        FontSize = 11, 
                        Foreground = new SolidColorBrush(Colors.White) 
                    });
                    RecentAchievementsPanel.Children.Add(achievementPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateRecentAchievementsPanel error: {ex.Message}");
            }
        }

        private void ShowHiddenAchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            _showHiddenAchievements = !_showHiddenAchievements;
            if (ShowHiddenAchievementsButton != null)
            {
                ShowHiddenAchievementsButton.Content = _showHiddenAchievements ? "👁️ Visible" : "👁️ Hidden";
            }
            LoadAchievementsForCategory(_currentAchievementCategory);
        }

        private void AchievementsHelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("Achievements Help", "Complete achievements to earn points and unlock rewards. Some achievements are hidden until discovered!", NotificationType.Info);
        }

        private void OnAchievementUnlocked(object sender, Achievement achievement)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ShowNotification("🏆 Achievement Unlocked!", $"{achievement.Name}: {achievement.Description}", NotificationType.Achievement);
                UpdateAchievementsUI();
            });
        }

        #endregion

        #region Phase 2: Weather System

        private void InitializeWeatherSystem()
        {
            try
            {
                _weatherService.InitializeWeatherForPlayer(_currentPlayer);
                
                // Start weather update timer
                _weatherTimer = new DispatcherTimer();
                _weatherTimer.Interval = TimeSpan.FromMinutes(1); // Check weather every minute
                _weatherTimer.Tick += WeatherTimer_Tick;
                _weatherTimer.Start();

                UpdateWeatherDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeWeatherSystem error: {ex.Message}");
            }
        }

        private void WeatherTimer_Tick(object sender, object e)
        {
            _weatherService.UpdateWeather();
            UpdateWeatherDisplay();
        }

        private void UpdateWeatherDisplay()
        {
            try
            {
                if (WeatherTextBlock != null)
                {
                    var weather = _weatherService.GetCurrentWeather();
                    var effect = _weatherService.GetCurrentWeatherEffect();
                    WeatherTextBlock.Text = $"{effect.Icon} {effect.Name}";
                    
                    // Update tooltip with detailed weather info
                    ToolTipService.SetToolTip(WeatherTextBlock, 
                        $"{effect.Description}\n{_weatherService.GetWeatherImpactDescription()}\n{_weatherService.GetWeatherForecast()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateWeatherDisplay error: {ex.Message}");
            }
        }

        private void OnWeatherChanged(object sender, WeatherCondition newWeather)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var effect = _weatherService.GetCurrentWeatherEffect();
                ShowNotification("🌤️ Weather Changed", $"Weather is now {effect.Name}: {effect.Description}", NotificationType.Info);
                UpdateWeatherDisplay();
                
                // Track weather encounter for achievements
                _achievementsService.CheckAndUpdateAchievements(_currentPlayer, "weather_survived", newWeather);
            });
        }

        #endregion

        #region Phase 2: Enhanced Mining with Skills and Weather

        private bool IsQuantumMaterial(string mineralName)
        {
            var quantumMinerals = new[] { "Void Crystal", "Temporal Gem", "Antimatter Fragment", "Reality Shard", "Quantum Dust", "Dimensional Ore" };
            return quantumMinerals.Contains(mineralName);
        }

        private void ApplyPhase2Bonuses(ref double efficiency, ref double safety, ref double staminaCost, ref double quantumBonus, ref double rareBonus)
        {
            try
            {
                // Apply skill bonuses
                _skillsService.ApplySkillBonusesToMining(_currentPlayer, ref efficiency, ref safety, ref staminaCost);
                
                // Apply weather effects
                _weatherService.ApplyWeatherEffects(ref efficiency, ref safety, ref staminaCost, ref quantumBonus, ref rareBonus);
                
                // Apply skill bonuses to weather resistance
                var weatherPenalty = 1.0 - safety;
                _skillsService.ApplySkillBonusesToWeather(_currentPlayer, ref weatherPenalty);
                safety = 1.0 - weatherPenalty;
                
                // Apply quantum bonuses from skills
                quantumBonus += _skillsService.GetQuantumBonus(_currentPlayer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyPhase2Bonuses error: {ex.Message}");
            }
        }

        private void InitializePhase2Systems()
        {
            try
            {
                // Initialize skills for player
                _skillsService.InitializePlayerSkills(_currentPlayer);
                
                // Initialize achievements for player
                _achievementsService.InitializePlayerAchievements(_currentPlayer);
                
                // Initialize weather system
                InitializeWeatherSystem();
                
                // Update UI
                UpdateSkillsUI();
                UpdateAchievementsUI();
                
                // Set default categories
                UpdateSkillCategoryButtons();
                LoadSkillsForCategory(_currentSkillCategory);
                
                UpdateAchievementCategoryButtons();
                LoadAchievementsForCategory(_currentAchievementCategory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializePhase2Systems error: {ex.Message}");
            }
        }

        #endregion
    }
}