using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MineRefine.Models;

namespace MineRefine.Services
{
    public class GameService
    {
        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-06-07 09:10:45";
        private const string CURRENT_USER = "Valinor-70";

        private readonly DataService _dataService;
        private readonly Random _random;
        private List<Mineral> _availableMinerals;
        private List<MiningLocation> _miningLocations;

        public GameService()
        {
            _dataService = new DataService();
            _random = new Random();
            _availableMinerals = _dataService.GetMinerals();
            _miningLocations = _dataService.GetMiningLocations();
        }

        #region Mining Operations

        public async Task<MineResult> PerformMiningOperationAsync(Player player, MiningLocation? location, double riskMultiplier)
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

            // Check stamina
            if (player.Stamina < 10)
            {
                return new MineResult
                {
                    IsSuccess = false,
                    Message = "Insufficient stamina for mining operation",
                    FailureReason = "Low Energy",
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Consume stamina
            var staminaCost = (int)(location.StaminaCost * 10);
            player.ConsumeStamina(staminaCost);

            // Calculate success rate
            var baseSuccessRate = location.BaseSuccessRate;
            var weatherModifier = location.GetWeatherModifier(location.CurrentWeather);
            var equipmentBonus = player.GetEquipmentBonus("success_rate");
            var riskModifier = Math.Max(0.5, 2.0 - riskMultiplier);

            var finalSuccessRate = baseSuccessRate * weatherModifier * riskModifier + equipmentBonus;
            finalSuccessRate = Math.Max(0.1, Math.Min(0.95, finalSuccessRate));

            // Perform mining attempt
            var success = _random.NextDouble() < finalSuccessRate;

            if (!success)
            {
                // Failed mining attempt
                player.TotalMinesCount++;

                var failureReasons = new[]
                {
                    "Equipment malfunction",
                    "Poor mineral deposit",
                    "Environmental interference",
                    "Technical difficulties",
                    "Unstable terrain"
                };

                return new MineResult
                {
                    IsSuccess = false,
                    Message = "Mining operation unsuccessful",
                    FailureReason = failureReasons[_random.Next(failureReasons.Length)],
                    Location = location,
                    WeatherAtTime = location.CurrentWeather,
                    OperationDifficulty = riskMultiplier,
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Successful mining - determine mineral
            var mineral = SelectMineralForLocation(location, riskMultiplier);
            if (mineral == null)
            {
                return new MineResult
                {
                    IsSuccess = false,
                    Message = "No viable minerals found",
                    FailureReason = "Empty Deposit",
                    Timestamp = DateTime.Parse(CURRENT_DATETIME)
                };
            }

            // Calculate value with bonuses
            var baseValue = mineral.Value;
            var locationBonus = location.GetMineralBonus(mineral.Id);
            var equipmentValueBonus = player.GetEquipmentBonus("value_multiplier");
            var riskBonus = riskMultiplier;

            var finalValue = (long)(baseValue * locationBonus * riskBonus * (1.0 + equipmentValueBonus));

            // Add money to player
            player.TotalMoney += finalValue;
            player.TotalEarnings += finalValue;
            player.TotalMinesCount++;
            player.RecordMineValue(finalValue);

            // Add experience
            var experience = CalculateExperience(mineral, location, riskMultiplier);
            player.AddExperience(experience);

            // Use equipment
            foreach (var equipment in player.GetEquippedItems())
            {
                equipment.Use();
            }

            // Update location stats
            location.Visit();
            if (!location.MineralDiscoveryCount.ContainsKey(mineral.Id))
            {
                location.MineralDiscoveryCount[mineral.Id] = 0;
            }
            location.MineralDiscoveryCount[mineral.Id]++;

            // Check for bonus discoveries
            string? bonusDiscovery = null;
            if (_random.NextDouble() < 0.1 * riskMultiplier) // 10% base chance, modified by risk
            {
                bonusDiscovery = GenerateBonusDiscovery(mineral, location);
            }

            return new MineResult
            {
                IsSuccess = true,
                Mineral = mineral,
                Value = finalValue,
                Location = location,
                Message = $"Successfully extracted {mineral.Name}!",
                ExperienceGained = experience,
                BonusDiscovery = bonusDiscovery,
                WeatherAtTime = location.CurrentWeather,
                OperationDifficulty = riskMultiplier,
                AppliedBonuses = new Dictionary<string, double>
                {
                    { "location_bonus", locationBonus },
                    { "equipment_bonus", equipmentValueBonus },
                    { "risk_bonus", riskBonus },
                    { "weather_modifier", weatherModifier }
                },
                Timestamp = DateTime.Parse(CURRENT_DATETIME)
            };
        }

        private Mineral? SelectMineralForLocation(MiningLocation location, double riskMultiplier)
        {
            var locationMinerals = _availableMinerals
                .Where(m => m.FoundInLocations.Contains(location.Id) || location.UniqueMinerals.Contains(m.Id))
                .ToList();

            if (!locationMinerals.Any())
            {
                // Fallback to common minerals
                locationMinerals = _availableMinerals.Where(m => m.Rarity == "Common").ToList();
            }

            // Apply rarity weighting based on risk multiplier
            var weightedMinerals = new List<Mineral>();

            foreach (var mineral in locationMinerals)
            {
                var rarityLevel = mineral.GetRarityLevel();
                var baseWeight = Math.Max(1, 8 - rarityLevel); // Common = 7, Quantum = 1
                var riskWeight = Math.Pow(riskMultiplier, rarityLevel - 1); // Higher risk = better rare mineral chances
                var finalWeight = (int)(baseWeight * riskWeight);

                for (int i = 0; i < finalWeight; i++)
                {
                    weightedMinerals.Add(mineral);
                }
            }

            return weightedMinerals.Any() ? weightedMinerals[_random.Next(weightedMinerals.Count)] : null;
        }

        private int CalculateExperience(Mineral mineral, MiningLocation location, double riskMultiplier)
        {
            var baseExp = 10;
            var rarityBonus = mineral.GetRarityLevel() * 5;
            var locationBonus = location.DangerLevel * 2;
            var riskBonus = (int)(riskMultiplier * 5);

            return baseExp + rarityBonus + locationBonus + riskBonus;
        }

        private string GenerateBonusDiscovery(Mineral mineral, MiningLocation location)
        {
            var discoveries = new[]
            {
                $"Found a rare {mineral.Name} specimen with unusual crystal formation",
                $"Discovered ancient tool marks near the {mineral.Name} deposit",
                $"Located additional {mineral.Name} veins in the area",
                $"Found {mineral.Name} with exceptional purity levels",
                $"Discovered {mineral.Name} formation that suggests larger deposits nearby"
            };

            return discoveries[_random.Next(discoveries.Length)];
        }

        #endregion

        #region Equipment Management

        public List<Equipment> GetUpgradeableEquipment(Player player)
        {
            return player.Equipment?.Where(e => e.CanUpgrade).ToList() ?? new List<Equipment>();
        }

        public bool CanUpgradeEquipment(Player player, Equipment equipment)
        {
            return equipment.CanUpgrade && player.TotalMoney >= equipment.UpgradeCost;
        }

        public bool UpgradeEquipment(Player player, Equipment equipment)
        {
            if (!CanUpgradeEquipment(player, equipment))
                return false;

            player.TotalMoney -= equipment.UpgradeCost;
            equipment.Upgrade();

            return true;
        }

        public bool RepairEquipment(Player player, Equipment equipment)
        {
            var repairCost = equipment.GetMaintenanceCost();
            if (player.TotalMoney < repairCost)
                return false;

            player.TotalMoney -= repairCost;
            equipment.Repair();

            return true;
        }

        public List<Equipment> GetAvailableEquipmentForPurchase()
        {
            return _dataService.GetAvailableEquipment();
        }

        public bool PurchaseEquipment(Player player, Equipment equipment)
        {
            if (player.TotalMoney < equipment.PurchaseCost)
                return false;

            player.TotalMoney -= equipment.PurchaseCost;

            // Clone equipment for player
            var playerEquipment = new Equipment
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Description = equipment.Description,
                Type = equipment.Type,
                Durability = equipment.MaxDurability,
                MaxDurability = equipment.MaxDurability,
                Level = equipment.Level,
                Icon = equipment.Icon,
                Rarity = equipment.Rarity,
                Bonuses = new Dictionary<string, double>(equipment.Bonuses),
                UpgradeCost = equipment.UpgradeCost,
                PurchaseCost = equipment.PurchaseCost,
                IsEquipped = false,
                SpecialAbilities = new List<string>(equipment.SpecialAbilities),
                AcquiredDate = DateTime.Parse(CURRENT_DATETIME),
                Manufacturer = equipment.Manufacturer,
                EfficiencyRating = equipment.EfficiencyRating
            };

            player.Equipment.Add(playerEquipment);
            return true;
        }

        #endregion

        #region Player Progression

        public bool CanUnlockLocation(Player player, MiningLocation location)
        {
            return player.Level >= location.RequiredLevel &&
                   player.TotalMoney >= location.UnlockCost &&
                   !player.UnlockedLocations.Contains(location.Id);
        }

        public bool UnlockLocation(Player player, MiningLocation location)
        {
            if (!CanUnlockLocation(player, location))
                return false;

            player.TotalMoney -= location.UnlockCost;
            player.UnlockedLocations.Add(location.Id);
            location.IsUnlocked = true;

            return true;
        }

        public List<Achievement> CheckAchievements(Player player)
        {
            var achievements = _dataService.GetAchievements();
            var unlockedAchievements = new List<Achievement>();

            foreach (var achievement in achievements.Where(a => !player.CompletedAchievements.Contains(a.Id)))
            {
                bool shouldUnlock = achievement.Type switch
                {
                    AchievementType.TotalMoney => player.TotalEarnings >= achievement.Target,
                    AchievementType.TotalMines => player.TotalMinesCount >= achievement.Target,
                    AchievementType.LocationUnlock => player.UnlockedLocations.Count >= achievement.Target,
                    AchievementType.SkillPoints => player.SkillPoints >= achievement.Target,
                    AchievementType.ConsecutiveSuccess => player.GetMiningStreak() >= achievement.Target,
                    AchievementType.SpecificMineral => CheckSpecificMineralAchievement(player, achievement),
                    _ => false
                };

                if (shouldUnlock)
                {
                    achievement.Complete();
                    player.CompletedAchievements.Add(achievement.Id);
                    player.TotalMoney += achievement.RewardMoney;
                    player.SkillPoints += achievement.RewardSkillPoints;
                    unlockedAchievements.Add(achievement);
                }
            }

            return unlockedAchievements;
        }

        private bool CheckSpecificMineralAchievement(Player player, Achievement achievement)
        {
            if (!achievement.Conditions.ContainsKey("mineral_id"))
                return false;

            var mineralId = achievement.Conditions["mineral_id"].ToString();
            return player.MineralStats.GetValueOrDefault(mineralId, 0) >= achievement.Target;
        }

        #endregion

        #region Game Statistics

        public Dictionary<string, object> GetPlayerStatistics(Player player)
        {
            return new Dictionary<string, object>
            {
                { "total_money", player.TotalMoney },
                { "total_earnings", player.TotalEarnings },
                { "total_mines", player.TotalMinesCount },
                { "success_rate", player.GetSuccessRate() },
                { "mining_streak", player.GetMiningStreak() },
                { "best_streak", player.BestMiningStreak },
                { "best_single_mine", player.SingleBestMineValue },
                { "locations_unlocked", player.UnlockedLocations.Count },
                { "achievements_completed", player.CompletedAchievements.Count },
                { "equipment_count", player.Equipment?.Count ?? 0 },
                { "equipped_items", player.GetEquippedItems().Count },
                { "level", player.Level },
                { "experience", player.ExperiencePoints },
                { "skill_points", player.SkillPoints },
                { "play_time", player.TotalPlayTime },
                { "session_count", player.SessionCount }
            };
        }

        public Dictionary<string, long> GetMineralStatistics(Player player)
        {
            return player.MineralStats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public List<MiningSession> GetRecentMiningSessions(Player player, int count = 10)
        {
            return player.MiningHistory
                .OrderByDescending(s => s.StartTime)
                .Take(count)
                .ToList();
        }

        #endregion

        #region Utility Methods

        public void RestorePlayerStamina(Player player, int amount)
        {
            player.RestoreStamina(amount);
        }

        public void RestorePlayerStaminaFull(Player player)
        {
            player.Stamina = player.MaxStamina;
        }

        public bool ValidateGameState(Player player)
        {
            try
            {
                // Basic validation checks
                if (player == null) return false;
                if (string.IsNullOrEmpty(player.Name)) return false;
                if (player.Level < 1 || player.Level > 100) return false;
                if (player.Stamina < 0 || player.Stamina > player.MaxStamina) return false;
                if (player.TotalMoney < 0) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateLocationWeather(MiningLocation location)
        {
            location.UpdateWeather();
        }

        public void UpdateAllLocationWeather()
        {
            foreach (var location in _miningLocations)
            {
                location.UpdateWeather();
            }
        }

        #endregion
    }
}