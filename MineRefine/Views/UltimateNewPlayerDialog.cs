using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MineRefine.Models;

namespace MineRefine.Views
{
    public sealed class UltimateNewPlayerDialog : ContentDialog
    {
        private TextBox _nameTextBox;
        private ComboBox _difficultyComboBox;
        private TextBlock _difficultyDescriptionTextBlock;

        public string? PlayerName { get; private set; }
        public string? SelectedDifficulty { get; private set; }

        public UltimateNewPlayerDialog()
        {
            Title = "🎮 Create New Miner";
            PrimaryButtonText = "Start Adventure";
            CloseButtonText = "Cancel";
            DefaultButton = ContentDialogButton.Primary;

            SetupContent();

            // Handle button clicks
            PrimaryButtonClick += UltimateNewPlayerDialog_PrimaryButtonClick;
            Opened += UltimateNewPlayerDialog_Opened;
        }

        private void UltimateNewPlayerDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            _nameTextBox?.Focus(FocusState.Keyboard);
        }

        private void UltimateNewPlayerDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PlayerName = _nameTextBox?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(PlayerName) || _difficultyComboBox?.SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            var selectedItem = _difficultyComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem?.Tag != null)
            {
                var data = selectedItem.Tag as DifficultyData;
                SelectedDifficulty = data?.Value;
            }
        }

        private void SetupContent()
        {
            var stackPanel = new StackPanel { Spacing = 20 };

            // Welcome message
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Welcome to the ultimate mining experience! Create your miner and begin your journey to riches.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Opacity = 0.8,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Name input
            stackPanel.Children.Add(new TextBlock
            {
                Text = "👤 Miner Name:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14
            });

            _nameTextBox = new TextBox
            {
                PlaceholderText = "Enter your miner's name...",
                MaxLength = 20
            };
            _nameTextBox.TextChanged += NameTextBox_TextChanged;
            stackPanel.Children.Add(_nameTextBox);

            // Difficulty selection
            stackPanel.Children.Add(new TextBlock
            {
                Text = "⚖️ Difficulty Level:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 0)
            });

            _difficultyComboBox = new ComboBox
            {
                PlaceholderText = "Select difficulty...",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var difficulties = new[]
            {
                new DifficultyData { Name = "🌱 Easy", Value = "EASY", Description = "Perfect for beginners. Higher starting money, better luck, and lower penalties." },
                new DifficultyData { Name = "⚖️ Normal", Value = "NORMAL", Description = "Balanced experience. Standard progression and moderate challenges." },
                new DifficultyData { Name = "🔥 Hard", Value = "HARD", Description = "For experienced miners. Lower starting resources and higher risks." },
                new DifficultyData { Name = "💀 Expert", Value = "EXPERT", Description = "Ultimate challenge. Minimal starting resources, maximum risk and reward." }
            };

            foreach (var difficulty in difficulties)
            {
                _difficultyComboBox.Items.Add(new ComboBoxItem
                {
                    Content = difficulty.Name,
                    Tag = difficulty
                });
            }

            _difficultyComboBox.SelectionChanged += DifficultyComboBox_SelectionChanged;
            stackPanel.Children.Add(_difficultyComboBox);

            // Difficulty description
            _difficultyDescriptionTextBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Opacity = 0.7,
                Margin = new Thickness(0, 5, 0, 0)
            };
            stackPanel.Children.Add(_difficultyDescriptionTextBlock);

            // Tips section
            var tipsExpander = new Expander
            {
                Header = "💡 Beginner Tips",
                Margin = new Thickness(0, 15, 0, 0)
            };

            var tipsPanel = new StackPanel { Spacing = 8 };
            var tips = new[]
            {
                "• Start with surface mining to learn the basics",
                "• Save money to unlock better locations and equipment",
                "• Watch the market trends to maximize profits",
                "• Complete achievements for bonus rewards",
                "• Manage your stamina and take breaks when needed",
                "• Experiment with different risk levels for higher rewards"
            };

            foreach (var tip in tips)
            {
                tipsPanel.Children.Add(new TextBlock
                {
                    Text = tip,
                    FontSize = 12,
                    Opacity = 0.8
                });
            }

            tipsExpander.Content = tipsPanel;
            stackPanel.Children.Add(tipsExpander);

            Content = new ScrollViewer
            {
                Content = stackPanel,
                MaxHeight = 500
            };

            UpdateButtonState();
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_difficultyComboBox.SelectedItem is ComboBoxItem item && item.Tag is DifficultyData data)
            {
                _difficultyDescriptionTextBlock.Text = data.Description;
            }
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(_nameTextBox?.Text) &&
                                   _difficultyComboBox?.SelectedItem != null;
        }

        private class DifficultyData
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}