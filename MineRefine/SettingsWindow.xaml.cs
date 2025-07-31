using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using MineRefine.Services;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace MineRefine
{
    public sealed partial class SettingsWindow : Window
    {
        private readonly DataService _dataService;
        private readonly GameSettings _gameSettings;
        private Player? _currentPlayer;
        private Difficulty _selectedDifficulty = Difficulty.EXPERIENCED;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-07-31 13:29:22";

        public SettingsWindow()
        {
            this.InitializeComponent();
            _dataService = new DataService();
            _gameSettings = new GameSettings
            {
                CurrentUser = "Valinor-70",
                LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                // Ensure all critical settings have proper defaults
                MasterVolume = 0.8,
                SoundEffects = true,
                BackgroundMusic = true,
                AnimationSpeed = 1.0,
                ReducedAnimations = false,
                HighContrast = false,
                ParticleEffects = true,
                AutoSave = true,
                AutoSaveInterval = 30,
                ShowTutorials = true,
                ShowTooltips = true,
                AutoMiningEnabled = false,
                AutoMiningInterval = 3,
                NotifyOnRareFinds = true,
                ShowMiningParticles = true
            };

            this.Title = "Mine & Refine - Game Setup";

            // Initialize on activation
            this.Activated += SettingsWindow_Activated;
        }

        private async void SettingsWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != WindowActivationState.Deactivated)
            {
                this.Activated -= SettingsWindow_Activated;
                await CheckForExistingSave();
            }
        }

        private async Task CheckForExistingSave()
        {
            try
            {
                var hasSave = await _dataService.HasExistingSaveAsync();

                if (hasSave)
                {
                    SaveStatusTextBlock.Text = "Existing save file detected! Load your game or start fresh.";
                    SaveStatusTextBlock.Foreground = new SolidColorBrush(Colors.LightGreen);
                    LoadGameButton.IsEnabled = true;
                    StartGameButton.IsEnabled = true;
                }
                else
                {
                    SaveStatusTextBlock.Text = "No save file detected. Create a new miner to begin your adventure!";
                    SaveStatusTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
                    LoadGameButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckForExistingSave error: {ex.Message}");
            }
        }

        #region Event Handlers

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerCreationPanel.Visibility = Visibility.Visible;
            StartGameButton.IsEnabled = true;

            // Generate random default name
            var randomNames = new[] { "CyberMiner", "QuantumDigger", "CrystalHunter", "VoidSeeker", "StormMiner" };
            var random = new Random();
            PlayerNameTextBox.Text = $"{randomNames[random.Next(randomNames.Length)]}_{random.Next(1000, 9999)}";
        }

        private async void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Loading existing save...");

                var players = await _dataService.LoadPlayersAsync();
                if (players.Count > 0)
                {
                    _currentPlayer = players[0]; // Load first player
                    await LaunchMainGame();
                }
                else
                {
                    HideLoading();
                    SaveStatusTextBlock.Text = "Save file is corrupted or empty. Please start a new game.";
                    SaveStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                HideLoading();
                System.Diagnostics.Debug.WriteLine($"LoadGameButton_Click error: {ex.Message}");
                SaveStatusTextBlock.Text = "Failed to load save file. Please start a new game.";
                SaveStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPlayer == null)
                {
                    // Create new player
                    await CreateNewPlayer();
                }

                if (_currentPlayer != null)
                {
                    await LaunchMainGame();
                }
            }
            catch (Exception ex)
            {
                HideLoading();
                System.Diagnostics.Debug.WriteLine($"StartGameButton_Click error: {ex.Message}");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var isValid = IsValidPlayerName(PlayerNameTextBox.Text);
            StartGameButton.IsEnabled = isValid;

            if (!isValid && !string.IsNullOrEmpty(PlayerNameTextBox.Text))
            {
                PlayerNameTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                PlayerNameTextBox.BorderBrush = new SolidColorBrush(Colors.Gold);
            }
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DifficultyComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                if (Enum.TryParse<Difficulty>(selectedItem.Tag.ToString(), out var difficulty))
                {
                    _selectedDifficulty = difficulty;
                    UpdateDifficultyDescription();
                }
            }
        }

        #endregion

        #region Helper Methods

        private async Task CreateNewPlayer()
        {
            try
            {
                ShowLoading("Creating your mining profile...");

                var playerName = PlayerNameTextBox.Text?.Trim() ?? "Anonymous Miner";
                var avatar = GetSelectedAvatar();

                _currentPlayer = await _dataService.CreateNewPlayerAsync(playerName, _selectedDifficulty);
                _currentPlayer.Avatar = avatar;

                // Apply game settings
                _gameSettings.MasterVolume = VolumeSlider.Value / 100.0;
                _gameSettings.AnimationSpeed = GetAnimationSpeed();
                _gameSettings.SoundEffects = SoundEffectsCheckBox.IsChecked ?? true;
                _gameSettings.ReducedAnimations = ReducedAnimationsCheckBox.IsChecked ?? false;
                _gameSettings.HighContrast = HighContrastCheckBox.IsChecked ?? false;
                _gameSettings.CurrentUser = "Valinor-70";
                _gameSettings.LastUpdated = DateTime.Parse(CURRENT_DATETIME);

                await _dataService.SaveGameSettingsAsync(_gameSettings);
            }
            catch (Exception ex)
            {
                HideLoading();
                throw new InvalidOperationException($"Failed to create player: {ex.Message}");
            }
        }

        private async Task LaunchMainGame()
        {
            try
            {
                ShowLoading("Launching mining operations...");
                await Task.Delay(1500); // Show loading for effect

                // Validate we have required data
                if (_currentPlayer == null)
                {
                    throw new InvalidOperationException("Player data is null - cannot start game");
                }

                if (_gameSettings == null)
                {
                    throw new InvalidOperationException("Game settings are null - cannot start game");
                }

                System.Diagnostics.Debug.WriteLine($"LaunchMainGame: Starting with player {_currentPlayer.Name}, difficulty {_currentPlayer.Difficulty}");

                // Launch main window
                var mainWindow = new MainWindow(_currentPlayer, _gameSettings);
                System.Diagnostics.Debug.WriteLine("LaunchMainGame: MainWindow created successfully");
                
                mainWindow.Activate();
                System.Diagnostics.Debug.WriteLine("LaunchMainGame: MainWindow activated successfully");

                // Close settings window
                this.Close();
                System.Diagnostics.Debug.WriteLine("LaunchMainGame: SettingsWindow closed successfully");
            }
            catch (Exception ex)
            {
                HideLoading();
                System.Diagnostics.Debug.WriteLine($"LaunchMainGame error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LaunchMainGame stack trace: {ex.StackTrace}");
                
                // Show error to user
                SaveStatusTextBlock.Text = $"Failed to start game: {ex.Message}";
                SaveStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private bool IsValidPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length < 3 || name.Length > 20)
                return false;

            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    return false;
            }

            return true;
        }

        private void UpdateDifficultyDescription()
        {
            // FIXED: Add null check
            if (DifficultyDescriptionTextBlock == null) return;

            var description = _selectedDifficulty switch
            {
                Difficulty.NOVICE => "Novice mode: Perfect for learning! Higher success rates, forgiving mechanics, and helpful tutorials guide your mining journey.",
                Difficulty.EXPERIENCED => "Experienced mode: Balanced gameplay with moderate risks and rewards. Ideal for players familiar with mining concepts.",
                Difficulty.EXPERT => "Expert mode: Enhanced rewards with increased risks. Perfect for experienced players seeking quantum challenges and advanced gameplay.",
                Difficulty.LEGENDARY => "Legendary mode: Ultimate challenge! Extreme risks and extraordinary rewards. Only for master miners ready to face quantum reality!",
                _ => "Select a difficulty level to see details."
            };

            var color = _selectedDifficulty switch
            {
                Difficulty.NOVICE => Colors.LightGreen,
                Difficulty.EXPERIENCED => Colors.LightBlue,
                Difficulty.EXPERT => Colors.Orange,
                Difficulty.LEGENDARY => Colors.Red,
                _ => Colors.LightGray
            };

            DifficultyDescriptionTextBlock.Text = description;
            DifficultyDescriptionTextBlock.Foreground = new SolidColorBrush(color);
        }

        private string GetSelectedAvatar()
        {
            if (ClassicAvatarRadio?.IsChecked == true) return "classic_miner";
            if (FemaleAvatarRadio?.IsChecked == true) return "female_miner";
            if (ScientistAvatarRadio?.IsChecked == true) return "quantum_scientist";
            if (CyberpunkAvatarRadio?.IsChecked == true) return "cyberpunk_miner";
            if (QuantumAvatarRadio?.IsChecked == true) return "quantum_explorer";
            return "classic_miner";
        }

        private double GetAnimationSpeed()
        {
            return AnimationSpeedComboBox?.SelectedIndex switch
            {
                0 => 0.5, // Slow
                1 => 1.0, // Normal
                2 => 1.5, // Fast
                3 => 2.0, // Ultra Fast
                _ => 1.0
            };
        }

        private void ShowLoading(string message = "Loading...")
        {
            if (LoadingOverlay != null) LoadingOverlay.Visibility = Visibility.Visible;
            if (LoadingTitle != null) LoadingTitle.Text = "Loading...";
            if (LoadingMessage != null) LoadingMessage.Text = message;
        }

        private void HideLoading()
        {
            if (LoadingOverlay != null) LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}