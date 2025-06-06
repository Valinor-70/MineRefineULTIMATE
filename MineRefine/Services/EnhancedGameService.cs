using MineRefine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MineRefine.Services
{
    public class GameService
    {
        private readonly DataService _dataService;
        private readonly MarketService _marketService;
        private readonly Random _random;

        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-06 21:18:13";
        private const string CURRENT_USER = "Valinor-70";

        public GameService()
        {
            _dataService = new DataService();
            _marketService = new MarketService();
            _random = new Random();
        }

        #region Mining Operations

        public async Task<MineResult> PerformMiningOperationAsync(Player player, MiningLocation location, double riskMultiplier = 1.0)
        {
            if (player == null || location == null)
            {
                return new MineResult
                {
                    IsSuccess = false,
                    Message = "Invalid mining parameters",
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Calculate stamina cost
            var staminaCost = (int)(location.StaminaCost * 10 * riskMultiplier);
            if (player.Stamina < staminaCost)
            {
                return new MineResult
                {
                    IsSuccess = false,
                    Message = "Insufficient energy for mining operation",
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Deduct stamina
            player.Stamina = Math.Max(0, player.Stamina - staminaCost);

            // Calculate success probability based on multiple factors
            var baseSuccessRate = 0.75; // 75% base success rate
            var locationModifier = (6 - location.DangerLevel) * 0.05; // Safer locations = higher success
            var playerModifier = GetPlayerSkillModifier(player);
            var equipmentModifier = GetEquipmentModifier(player);
            var weatherModifier = GetWeatherModifier(location.CurrentWeather);

            var totalSuccessRate = Math.Min(0.95, baseSuccessRate + locationModifier + playerModifier + equipmentModifier + weatherModifier);

            // Apply risk multiplier effect
            if (riskMultiplier > 1.5)
            {
                totalSuccessRate *= 0.9; // Higher risk = lower success rate
            }

            var isSuccess = _random.NextDouble() < totalSuccessRate;

            if (!isSuccess)
            {
                var failureReasons = new[]
                {
                    "Equipment malfunction detected",
                    "Unstable geological conditions",
                    "Environmental hazard encountered",
                    "Resource vein exhausted",
                    "Quantum interference detected",
                    "Temporal anomaly disrupted operation"
                };

                return new MineResult
                {
                    IsSuccess = false,
                    Message = failureReasons[_random.Next(failureReasons.Length)],
                    Location = location,
                    ExperienceGained = _random.Next(1, 5), // Small XP for failed attempts
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Success - determine mineral discovery
            var discoveredMineral = SelectMineralForLocation(location, riskMultiplier);
            if (discoveredMineral == null)
            {
                return new MineResult
                {
                    IsSuccess = false,
                    Message = "No valuable minerals found in this operation",
                    Location = location,
                    ExperienceGained = _random.Next(5, 10),
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Calculate value with all modifiers
            var baseValue = discoveredMineral.Value;
            var marketMultiplier = _marketService.GetMarketMultiplier(discoveredMineral.Name);
            var locationBonus = location.MineralBonuses.GetValueOrDefault(discoveredMineral.Name, 1.0);
            var finalValue = (long)(baseValue * marketMultiplier * locationBonus * riskMultiplier);

            // Apply bonuses and special discoveries
            var bonusDiscovery = CheckForBonusDiscovery(player, discoveredMineral, riskMultiplier);
            if (!string.IsNullOrEmpty(bonusDiscovery))
            {
                finalValue = (long)(finalValue * 1.5); // 50% bonus for special discoveries
            }

            // Update player statistics
            player.TotalMoney += finalValue;
            player.TotalEarnings += finalValue;
            player.TotalMinesCount++;
            player.RecordMineValue(finalValue);

            // Update mineral statistics
            if (!player.MineralStats.ContainsKey(discoveredMineral.Id))
            {
                player.MineralStats[discoveredMineral.Id] = 0;
            }
            player.MineralStats[discoveredMineral.Id]++;

            // Calculate experience gained
            var experienceGained = CalculateExperienceGained(discoveredMineral, finalValue, riskMultiplier);
            player.AddExperience(experienceGained);

            // Update location visit tracking
            player.VisitLocation(location.Id);
            location.TimesVisited++;
            location.LastVisited = DateTime.Parse(CURRENT_DATETIME);

            return new MineResult
            {
                IsSuccess = true,
                Mineral = discoveredMineral,
                Value = finalValue,
                Location = location,
                Message = GenerateSuccessMessage(discoveredMineral, finalValue, bonusDiscovery),
                ExperienceGained = experienceGained,
                BonusDiscovery = bonusDiscovery,
                Timestamp = DateTime.Parse(CURRENT_DATETIME)
            };
        }

        private Mineral? SelectMineralForLocation(MiningLocation location, double riskMultiplier)
        {
            var minerals = _dataService.GetMinerals();
            var availableMinerals = minerals.Where(m =>
                location.UniqueMinerals.Contains(m.Id) ||
                m.FoundInLocations.Contains(location.Id)).ToList();

            if (!availableMinerals.Any())
            {
                // Fallback to common minerals
                availableMinerals = minerals.Where(m => m.Rarity == "Common").ToList();
            }

            // Weight selection based on rarity and risk
            var weightedMinerals = new List<(Mineral mineral, double weight)>();

            foreach (var mineral in availableMinerals)
            {
                var baseWeight = mineral.Rarity switch
                {
                    "Common" => 40.0,
                    "Uncommon" => 25.0,
                    "Rare" => 15.0,
                    "Epic" => 8.0,
                    "Legendary" => 2.0,
                    _ => 30.0
                };

                // Higher risk = better chance at rare minerals
                if (riskMultiplier > 2.0 && mineral.Rarity != "Common")
                {
                    baseWeight *= 1.5;
                }

                weightedMinerals.Add((mineral, baseWeight));
            }

            // Select based on weighted probability
            var totalWeight = weightedMinerals.Sum(w => w.weight);
            var randomValue = _random.NextDouble() * totalWeight;
            var cumulativeWeight = 0.0;

            foreach (var (mineral, weight) in weightedMinerals)
            {
                cumulativeWeight += weight;
                if (randomValue <= cumulativeWeight)
                {
                    return mineral;
                }
            }

            return weightedMinerals.FirstOrDefault().mineral;
        }

        private string? CheckForBonusDiscovery(Player player, Mineral mineral, double riskMultiplier)
        {
            var bonusChance = 0.05; // 5% base chance

            // Increase chance based on player skills
            if (player.UnlockedSkills.Contains("lucky_strike"))
            {
                bonusChance += 0.10; // +10% with lucky strike
            }

            // Risk multiplier effect
            bonusChance *= riskMultiplier;

            if (_random.NextDouble() < bonusChance)
            {
                var bonusTypes = new[]
                {
                    "Perfect Crystal Formation",
                    "Quantum Resonance Amplification",
                    "Temporal Stability Enhancement",
                    "Dimensional Purity Bonus",
                    "Market Synchronization Event"
                };

                return bonusTypes[_random.Next(bonusTypes.Length)];
            }

            return null;
        }

        private string GenerateSuccessMessage(Mineral mineral, long value, string? bonusDiscovery)
        {
            var baseMessage = $"Successfully extracted {mineral.Icon} {mineral.Name} worth £{value:N0}!";

            if (!string.IsNullOrEmpty(bonusDiscovery))
            {
                baseMessage += $" Bonus: {bonusDiscovery}!";
            }

            if (value > 1000000) // Over 1M
            {
                baseMessage += " 🌟 EXCEPTIONAL DISCOVERY!";
            }
            else if (value > 100000) // Over 100K
            {
                baseMessage += " ⭐ Excellent find!";
            }

            return baseMessage;
        }

        #endregion

        #region Player Progression

        public void CheckLevelProgression(Player player)
        {
            var requiredXP = CalculateRequiredExperience(player.Level);

            while (player.ExperiencePoints >= requiredXP)
            {
                player.Level++;
                player.SkillPoints += GetSkillPointsForLevel(player.Level);
                player.ExperiencePoints -= requiredXP;
                requiredXP = CalculateRequiredExperience(player.Level);

                // Check for rank progression
                CheckRankProgression(player);
            }
        }

        public void CheckRankProgression(Player player)
        {
            var newRank = DetermineRankFromLevel(player.Level);
            if (newRank != player.Rank)
            {
                player.Rank = newRank;

                // Unlock new locations based on rank
                UnlockLocationsByRank(player, newRank);
            }
        }

        private Rank DetermineRankFromLevel(int level)
        {
            return level switch
            {
                >= 50 => Rank.ASCENDED_MINER,
                >= 40 => Rank.LEGENDARY_MINER,
                >= 30 => Rank.MASTER_MINER,
                >= 20 => Rank.EXPERT_MINER,
                >= 10 => Rank.EXPERIENCED_MINER,
                >= 5 => Rank.INTERMEDIATE,
                _ => Rank.NOVICE_MINER
            };
        }

        private void UnlockLocationsByRank(Player player, Rank newRank)
        {
            var locations = _dataService.GetMiningLocations();
            var rankString = newRank.ToString();

            foreach (var location in locations)
            {
                if (location.RequiredRank == rankString && !player.UnlockedLocations.Contains(location.Id))
                {
                    player.UnlockedLocations.Add(location.Id);
                }
            }
        }

        #endregion

        #region Achievement System

        public List<Achievement> CheckAchievements(Player player)
        {
            var achievements = _dataService.GetAchievements();
            var newlyCompleted = new List<Achievement>();

            foreach (var achievement in achievements)
            {
                if (achievement.IsCompleted || player.CompletedAchievements.Contains(achievement.Id))
                    continue;

                var isCompleted = achievement.Type switch
                {
                    AchievementType.TotalMoney => player.TotalMoney >= achievement.Target,
                    AchievementType.TotalMines => player.TotalMinesCount >= achievement.Target,
                    AchievementType.SpecificMineral => CheckSpecificMineralAchievement(player, achievement),
                    AchievementType.LocationUnlock => player.UnlockedLocations.Count >= achievement.Target,
                    AchievementType.ConsecutiveSuccess => player.ConsecutiveSuccessfulMines >= achievement.Target,
                    AchievementType.SkillPoints => player.SkillPoints >= achievement.Target,
                    AchievementType.RankAdvancement => (int)player.Rank >= achievement.Target,
                    _ => false
                };

                if (isCompleted)
                {
                    achievement.IsCompleted = true;
                    achievement.CompletedDate = DateTime.Parse(CURRENT_DATETIME);
                    achievement.CompletedBy = player.Name;
                    player.CompletedAchievements.Add(achievement.Id);

                    // Award rewards
                    player.TotalMoney += achievement.RewardMoney;
                    player.SkillPoints += achievement.RewardSkillPoints;

                    newlyCompleted.Add(achievement);
                }
            }

            return newlyCompleted;
        }

        private bool CheckSpecificMineralAchievement(Player player, Achievement achievement)
        {
            // Extract mineral name from achievement description or use a mapping
            var mineralMappings = new Dictionary<string, string>
            {
                { "first_diamond", "diamond" },
                { "platinum_finder", "platinum" },
                { "void_crystal_master", "void_crystal" },
                { "antimatter_collector", "antimatter_fragment" },
                { "temporal_gem_finder", "temporal_gem" }
            };

            if (mineralMappings.TryGetValue(achievement.Id, out var mineralId))
            {
                return player.MineralStats.GetValueOrDefault(mineralId, 0) >= achievement.Target;
            }

            return false;
        }

        #endregion

        #region Utility Methods

        private double GetPlayerSkillModifier(Player player)
        {
            var modifier = 0.0;

            if (player.UnlockedSkills.Contains("efficient_mining"))
                modifier += 0.05;

            if (player.UnlockedSkills.Contains("deep_mining"))
                modifier += 0.08;

            if (player.UnlockedSkills.Contains("master_miner"))
                modifier += 0.12;

            // Level-based modifier
            modifier += player.Level * 0.002; // +0.2% per level

            return modifier;
        }

        private double GetEquipmentModifier(Player player)
        {
            var modifier = 0.0;

            foreach (var equipment in player.Equipment.Where(e => e.IsEquipped))
            {
                modifier += equipment.Bonuses.GetValueOrDefault("mining_efficiency", 0.0);
                modifier += equipment.Bonuses.GetValueOrDefault("success_rate", 0.0);
            }

            // Legacy equipment bonuses
            if (player.PowerDrill) modifier += 0.10;
            if (player.LuckyCharm) modifier += 0.05;
            if (player.Gpr) modifier += 0.08;

            return modifier;
        }

        private double GetWeatherModifier(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Clear => 0.05,
                WeatherCondition.Cloudy => 0.0,
                WeatherCondition.Rainy => -0.05,
                WeatherCondition.Stormy => -0.10,
                WeatherCondition.QuantumFlux => 0.15, // Bonus for quantum weather
                WeatherCondition.TemporalStorm => 0.20, // High risk, high reward
                _ => 0.0
            };
        }

        private int CalculateExperienceGained(Mineral mineral, long value, double riskMultiplier)
        {
            var baseXP = mineral.Rarity switch
            {
                "Common" => 10,
                "Uncommon" => 25,
                "Rare" => 50,
                "Epic" => 100,
                "Legendary" => 250,
                _ => 15
            };

            // Value-based bonus
            var valueBonus = (int)(value / 10000); // 1 XP per 10K value

            // Risk multiplier bonus
            var riskBonus = (int)(baseXP * (riskMultiplier - 1.0) * 0.5);

            return baseXP + valueBonus + riskBonus;
        }

        private long CalculateRequiredExperience(int level)
        {
            return (long)(1000 * Math.Pow(1.1, level - 1));
        }

        private int GetSkillPointsForLevel(int level)
        {
            if (level % 10 == 0) return 3; // Bonus at milestone levels
            if (level % 5 == 0) return 2;  // Bonus at every 5 levels
            return 1; // Standard skill point per level
        }

        #endregion

        #region Special Events and Sessions

        public async Task<MiningSession> StartMiningSessionAsync(Player player, MiningLocation location)
        {
            var session = new MiningSession
            {
                Id = Guid.NewGuid().ToString(),
                PlayerId = player.Name,
                LocationId = location.Id,
                StartTime = DateTime.Parse(CURRENT_DATETIME),
                IsActive = true,
                Weather = GetRandomWeatherForLocation(location),
                RiskMultiplier = player.Multiplier
            };

            return session;
        }

        public List<SpecialEvent> GetActiveSpecialEvents()
        {
            var currentTime = DateTime.Parse(CURRENT_DATETIME);

            return new List<SpecialEvent>
            {
                new()
                {
                    Id = "quantum_convergence_2025",
                    Name = "Quantum Convergence Event",
                    Description = "Reality fluctuations increase quantum mineral discovery rates by 200%",
                    Icon = "🌌",
                    StartDate = currentTime.AddHours(-2),
                    EndDate = currentTime.AddHours(4),
                    IsActive = true,
                    Type = EventType.Quantum,
                    Effects = new()
                    {
                        { "quantum_ore", 3.0 },
                        { "void_crystal", 2.5 },
                        { "temporal_gem", 3.5 }
                    },
                    AffectedLocations = new() { "quantum_realm" },
                    Rarity = "Legendary"
                },
                new()
                {
                    Id = "mining_efficiency_boost_2025",
                    Name = "Global Mining Efficiency Day",
                    Description = "Advanced mining techniques reduce stamina costs by 30%",
                    Icon = "⚡",
                    StartDate = currentTime.Date,
                    EndDate = currentTime.Date.AddDays(1),
                    IsActive = true,
                    Type = EventType.Bonus,
                    Effects = new()
                    {
                        { "stamina_efficiency", 0.3 }
                    },
                    AffectedLocations = new() { "all" },
                    Rarity = "Common"
                }
            };
        }

        private WeatherCondition GetRandomWeatherForLocation(MiningLocation location)
        {
            return location.Id switch
            {
                "quantum_realm" => (WeatherCondition)_random.Next((int)WeatherCondition.QuantumFlux, (int)WeatherCondition.DimensionalRift + 1),
                "volcanic_depths" => WeatherCondition.Clear, // Volcanic areas have stable "weather"
                _ => (WeatherCondition)_random.Next(0, (int)WeatherCondition.Windy + 1)
            };
        }

        #endregion

        #region Save/Load Operations

        public async Task SaveGameAsync(Player player)
        {
            try
            {
                var allPlayers = await _dataService.LoadPlayersAsync();
                var existingPlayerIndex = allPlayers.FindIndex(p => p.Name == player.Name);

                if (existingPlayerIndex >= 0)
                {
                    allPlayers[existingPlayerIndex] = player;
                }
                else
                {
                    allPlayers.Add(player);
                }

                player.LastPlayed = DateTime.Parse(CURRENT_DATETIME);
                await _dataService.SavePlayersAsync(allPlayers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save game error: {ex.Message}");
                throw;
            }
        }

        public async Task<Player?> LoadPlayerAsync(string playerName)
        {
            try
            {
                var allPlayers = await _dataService.LoadPlayersAsync();
                return allPlayers.FirstOrDefault(p => p.Name == playerName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load player error: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}