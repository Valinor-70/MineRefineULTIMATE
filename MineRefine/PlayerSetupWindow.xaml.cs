using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using MineRefine.Services;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace MineRefine
{
    public sealed partial class PlayerSetupWindow : Window
    {
        // Properties for main window communication
        public bool PlayerCreated { get; private set; } = false;
        public Player? CreatedPlayer { get; private set; }
        public bool Visible { get; private set; } = true;

        // Services and state
        private readonly DataService _dataService;
        private Difficulty _selectedDifficulty = Difficulty.EXPERT;
        private string _selectedAvatar = "classic_miner";

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-06 21:34:25";
        private const string CURRENT_USER = "Valinor-70";

        public PlayerSetupWindow()
        {
            this.InitializeComponent();
            _dataService = new DataService();

            // Set window properties
            this.Title = "Mine & Refine - Player Setup";
            this.Activate();

            // Initialize form
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                // Set default values
                if (PlayerNameTextBox != null)
                {
                    PlayerNameTextBox.Text = CURRENT_USER;
                }

                // Set default difficulty description
                UpdateDifficultyDescription();

                // Set default avatar
                if (ClassicAvatarRadio != null)
                {
                    ClassicAvatarRadio.IsChecked = true;
                }

                // Enable create button by default
                if (CreatePlayerButton != null)
                {
                    CreatePlayerButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeForm error: {ex.Message}");
            }
        }

        #region Event Handlers

        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var textBox = sender as TextBox;
                if (textBox != null && CreatePlayerButton != null)
                {
                    var isValid = IsValidPlayerName(textBox.Text);
                    CreatePlayerButton.IsEnabled = isValid;

                    if (!isValid && !string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        textBox.BorderBrush = new SolidColorBrush(Colors.Gold);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PlayerNameTextBox_TextChanged error: {ex.Message}");
            }
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DifficultyComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private async void CreatePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var playerName = PlayerNameTextBox?.Text?.Trim() ?? CURRENT_USER;

                if (!IsValidPlayerName(playerName))
                {
                    ShowError("Please enter a valid player name (3-20 characters, no special characters).");
                    return;
                }

                // Show loading
                ShowLoading();

                // Get selected avatar
                _selectedAvatar = GetSelectedAvatar();

                // Create player
                var newPlayer = CreatePlayerFromForm(playerName);

                // Save player
                var allPlayers = await _dataService.LoadPlayersAsync();

                // Check if player name already exists
                if (allPlayers.Exists(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                {
                    HideLoading();
                    ShowError($"A miner named '{playerName}' already exists. Please choose a different name.");
                    return;
                }

                allPlayers.Add(newPlayer);
                await _dataService.SavePlayersAsync(allPlayers);

                // Set success properties
                CreatedPlayer = newPlayer;
                PlayerCreated = true;

                // Close window
                await Task.Delay(1000); // Show success briefly
                CloseWindow();
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowError($"Failed to create player: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"CreatePlayerButton_Click error: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerCreated = false;
                CreatedPlayer = null;
                CloseWindow();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelButton_Click error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private bool IsValidPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length < 3 || name.Length > 20)
                return false;

            // Check for valid characters (letters, numbers, hyphens, underscores)
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    return false;
            }

            return true;
        }

        private void UpdateDifficultyDescription()
        {
            try
            {
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

                if (DifficultyDescriptionTextBlock != null)
                {
                    DifficultyDescriptionTextBlock.Text = description;
                    DifficultyDescriptionTextBlock.Foreground = new SolidColorBrush(color);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDifficultyDescription error: {ex.Message}");
            }
        }

        private string GetSelectedAvatar()
        {
            try
            {
                if (ClassicAvatarRadio?.IsChecked == true) return "classic_miner";
                if (FemaleAvatarRadio?.IsChecked == true) return "female_miner";
                if (ScientistAvatarRadio?.IsChecked == true) return "quantum_scientist";
                if (CyberpunkAvatarRadio?.IsChecked == true) return "cyberpunk_miner";
                if (QuantumAvatarRadio?.IsChecked == true) return "quantum_explorer";

                return "classic_miner"; // Default fallback
            }
            catch
            {
                return "classic_miner";
            }
        }

        private Player CreatePlayerFromForm(string playerName)
        {
            var startingMoney = _selectedDifficulty switch
            {
                Difficulty.NOVICE => 25000,
                Difficulty.EXPERIENCED => 15000,
                Difficulty.EXPERT => 10000,
                Difficulty.LEGENDARY => 5000,
                _ => 10000
            };

            var startingStamina = _selectedDifficulty switch
            {
                Difficulty.NOVICE => 120,
                Difficulty.EXPERIENCED => 110,
                Difficulty.EXPERT => 100,
                Difficulty.LEGENDARY => 90,
                _ => 100
            };

            var player = new Player
            {
                Name = playerName,
                Difficulty = _selectedDifficulty,
                Rank = Rank.NOVICE_MINER,
                Level = 1,
                TotalMoney = startingMoney,
                Debt = 0,
                TotalEarnings = 0,
                TotalMinesCount = 0,
                Stamina = startingStamina,
                MaxStamina = startingStamina,
                ExperiencePoints = 0,
                SkillPoints = 2, // Starting skill points
                Multiplier = 1.0,
                CreatedDate = DateTime.Parse(CURRENT_DATETIME),
                LastPlayed = DateTime.Parse(CURRENT_DATETIME),
                LastLogin = DateTime.Parse(CURRENT_DATETIME),
                CurrentLocationId = "surface_mine",
                Avatar = _selectedAvatar,
                TutorialCompleted = false,
                PreferredDifficulty = _selectedDifficulty.ToString()
            };

            // Add starting location
            player.UnlockedLocations.Add("surface_mine");

            // Add starting equipment
            player.Equipment = _dataService.GetStartingEquipment();

            // Add tutorial achievement for novice players
            if (_selectedDifficulty == Difficulty.NOVICE)
            {
                player.CompletedTutorials.Add("basic_setup");
            }

            return player;
        }

        private void ShowLoading()
        {
            if (LoadingOverlay != null)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
            }
        }

        private void HideLoading()
        {
            if (LoadingOverlay != null)
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            // For now, just debug output. Could implement proper error dialog later.
            System.Diagnostics.Debug.WriteLine($"Player Setup Error: {message}");
        }

        private void CloseWindow()
        {
            try
            {
                Visible = false;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseWindow error: {ex.Message}");
            }
        }

        #endregion
    }
}