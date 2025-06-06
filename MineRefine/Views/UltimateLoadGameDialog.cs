using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MineRefine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace MineRefine.Views
{
    public sealed class UltimateLoadGameDialog : ContentDialog
    {
        private ListView _playerListView;
        private readonly List<Player> _players;

        public Player? SelectedPlayer { get; private set; }

        public UltimateLoadGameDialog(List<Player> players)
        {
            _players = players;
            Title = "💾 Load Game";
            PrimaryButtonText = "Load";
            CloseButtonText = "Cancel";
            DefaultButton = ContentDialogButton.Primary;
            IsPrimaryButtonEnabled = false;

            SetupContent();

            PrimaryButtonClick += UltimateLoadGameDialog_PrimaryButtonClick;
        }

        private void UltimateLoadGameDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedPlayer = _playerListView.SelectedItem as Player;
            if (SelectedPlayer == null)
            {
                args.Cancel = true;
            }
        }

        private void SetupContent()
        {
            var stackPanel = new StackPanel { Spacing = 15 };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Select a saved game to continue your mining adventure:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            });

            _playerListView = new ListView
            {
                ItemsSource = _players.OrderByDescending(p => p.CreatedDate).ToList(),
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 400
            };

            // Create items manually instead of using DataTemplate
            _playerListView.Items.Clear();
            foreach (var player in _players.OrderByDescending(p => p.CreatedDate))
            {
                var playerItem = CreatePlayerListItem(player);
                _playerListView.Items.Add(playerItem);
            }

            _playerListView.SelectionChanged += PlayerListView_SelectionChanged;
            stackPanel.Children.Add(_playerListView);

            // Stats summary
            var statsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 20,
                Margin = new Thickness(0, 15, 0, 0)
            };

            statsPanel.Children.Add(new TextBlock
            {
                Text = $"📁 {_players.Count} saved games",
                FontSize = 12,
                Opacity = 0.7
            });

            var totalMoney = _players.Sum(p => p.TotalMoney);
            statsPanel.Children.Add(new TextBlock
            {
                Text = $"💰 £{totalMoney:N0} total wealth",
                FontSize = 12,
                Opacity = 0.7
            });

            var totalMines = _players.Sum(p => p.TotalMinesCount);
            statsPanel.Children.Add(new TextBlock
            {
                Text = $"⛏️ {totalMines:N0} total mines",
                FontSize = 12,
                Opacity = 0.7
            });

            stackPanel.Children.Add(statsPanel);

            Content = stackPanel;
        }

        private Border CreatePlayerListItem(Player player)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 2,0,0),
                Tag = player
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Player info
            var playerInfo = new StackPanel();

            var namePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            namePanel.Children.Add(new TextBlock
            {
                Text = player.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 16
            });
            namePanel.Children.Add(new TextBlock { Text = "•", Opacity = 0.5 });
            namePanel.Children.Add(new TextBlock
            {
                Text = player.Rank.ToString(),
                FontSize = 12,
                Opacity = 0.8
            });
            namePanel.Children.Add(new TextBlock { Text = "•", Opacity = 0.5 });
            namePanel.Children.Add(new TextBlock
            {
                Text = $"Level {player.Level}",
                FontSize = 12,
                Opacity = 0.8
            });

            playerInfo.Children.Add(namePanel);

            // Stats
            var statsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15, Margin = new Thickness(0, 5, 0, 0) };

            statsPanel.Children.Add(new TextBlock
            {
                Text = $"💰 £{player.TotalMoney:N0}",
                FontSize = 12
            });

            statsPanel.Children.Add(new TextBlock
            {
                Text = $"⛏️ {player.TotalMinesCount:N0} mines",
                FontSize = 12
            });

            playerInfo.Children.Add(statsPanel);

            // Created date
            playerInfo.Children.Add(new TextBlock
            {
                Text = $"📅 Created: {player.CreatedDate:MMM dd, yyyy}",
                FontSize = 11,
                Opacity = 0.6,
                Margin = new Thickness(0, 3, 0, 0)
            });

            Grid.SetColumn(playerInfo, 0);
            grid.Children.Add(playerInfo);

            // Difficulty indicator
            var difficultyTextBlock = new TextBlock
            {
                Text = player.Difficulty.ToString(),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(difficultyTextBlock, 1);
            grid.Children.Add(difficultyTextBlock);

            border.Child = grid;
            return border;
        }

        private void PlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_playerListView.SelectedItem is Border border && border.Tag is Player player)
            {
                SelectedPlayer = player;
                IsPrimaryButtonEnabled = true;
            }
            else
            {
                SelectedPlayer = null;
                IsPrimaryButtonEnabled = false;
            }
        }
    }
}