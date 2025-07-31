using System;
using System.Collections.Generic;
using System.Linq;
using MineRefine.Models;

namespace MineRefine.Services
{
    public class AchievementsService
    {
        private readonly AchievementSystem _achievementSystem;
        private readonly DataService _dataService;

        public event EventHandler<Achievement>? AchievementUnlocked;

        public AchievementsService()
        {
            _achievementSystem = new AchievementSystem();
            _achievementSystem.InitializeAchievements();
            _dataService = new DataService();
        }

        public AchievementSystem GetAchievementSystem() => _achievementSystem;

        public List<Achievement> GetAllAchievements() => _achievementSystem.Achievements;

        public List<Achievement> GetAchievementsByCategory(string category)
        {
            return _achievementSystem.GetAchievementsByCategory(category);
        }

        public List<Achievement> GetCompletedAchievements()
        {
            return _achievementSystem.GetCompletedAchievements();
        }

        public Achievement? GetAchievement(string achievementId)
        {
            return _achievementSystem.AchievementMap.GetValueOrDefault(achievementId);
        }

        public void CheckAndUpdateAchievements(Player player, string eventType, object eventData)
        {
            foreach (var achievement in _achievementSystem.Achievements)
            {
                if (achievement.IsCompleted) continue;

                var progressUpdated = false;
                var newProgress = achievement.CurrentProgress;

                switch (achievement.Type)
                {
                    case AchievementType.TotalMoney:
                        if (eventType == "money_earned")
                        {
                            newProgress = (int)player.TotalEarnings;
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.TotalMines:
                        if (eventType == "mining_completed")
                        {
                            newProgress = (int)player.TotalMinesCount;
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.SpecificMineral:
                        if (eventType == "mineral_found" && eventData is string mineralType)
                        {
                            if (achievement.Id == "rare_finder" && IsRareMineral(mineralType))
                            {
                                newProgress = 1;
                                progressUpdated = true;
                            }
                            else if (achievement.Id == "legendary_finder" && IsLegendaryMineral(mineralType))
                            {
                                newProgress = 1;
                                progressUpdated = true;
                            }
                        }
                        break;

                    case AchievementType.RankAdvancement:
                        if (eventType == "rank_changed")
                        {
                            newProgress = (int)player.Rank;
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.LocationDiscovery:
                        if (eventType == "location_mined" && eventData is string locationId)
                        {
                            newProgress = GetLocationMiningCount(player, locationId, achievement.Id);
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.LocationUnlock:
                        if (eventType == "location_unlocked")
                        {
                            if (achievement.Id == "quantum_master" && eventData?.ToString() == "quantum_realm")
                            {
                                newProgress = 1;
                                progressUpdated = true;
                            }
                        }
                        break;

                    case AchievementType.EquipmentUpgrade:
                        if (eventType == "equipment_upgraded")
                        {
                            newProgress = player.Equipment.Sum(e => e.Level);
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.ConsecutiveMines:
                        if (eventType == "mining_completed")
                        {
                            if (achievement.Id == "risk_taker" && eventData is double riskMultiplier && riskMultiplier >= 2.5)
                            {
                                newProgress++;
                                progressUpdated = true;
                            }
                        }
                        break;

                    case AchievementType.ConsecutiveSuccess:
                        if (eventType == "mining_success")
                        {
                            newProgress = player.ConsecutiveSuccessfulMines;
                            progressUpdated = true;
                        }
                        else if (eventType == "mining_failure")
                        {
                            // Reset consecutive success achievements
                            if (achievement.Type == AchievementType.ConsecutiveSuccess)
                            {
                                newProgress = 0;
                                progressUpdated = true;
                            }
                        }
                        break;

                    case AchievementType.WeatherSurvival:
                        if (eventType == "weather_survived" && eventData is WeatherCondition weather)
                        {
                            // Track unique weather conditions survived
                            if (!player.WeatherEncounters.ContainsKey(weather.ToString()))
                            {
                                player.WeatherEncounters[weather.ToString()] = 0;
                            }
                            player.WeatherEncounters[weather.ToString()]++;
                            newProgress = player.WeatherEncounters.Count;
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.QuantumMastery:
                        if (eventType == "quantum_material_found")
                        {
                            newProgress = GetQuantumMaterialCount(player);
                            progressUpdated = true;
                        }
                        else if (eventType == "temporal_anomaly" && achievement.Id == "time_traveler")
                        {
                            newProgress = 1;
                            progressUpdated = true;
                        }
                        else if (eventType == "reality_altered" && achievement.Id == "reality_bender")
                        {
                            newProgress++;
                            progressUpdated = true;
                        }
                        break;

                    case AchievementType.SkillPoints:
                        if (eventType == "skill_purchased" || eventType == "skill_upgraded")
                        {
                            if (achievement.Id == "skill_learner")
                            {
                                newProgress = player.SkillLevels.Count > 0 ? 1 : 0;
                            }
                            else if (achievement.Id == "skill_expert")
                            {
                                newProgress = player.SkillLevels.Values.Any(level => level >= 10) ? 1 : 0;
                            }
                            else if (achievement.Id == "skill_master")
                            {
                                newProgress = player.SkillLevels.Values.Count(level => level >= 10);
                            }
                            progressUpdated = true;
                        }
                        break;
                }

                if (progressUpdated)
                {
                    var wasCompleted = achievement.IsCompleted;
                    achievement.UpdateProgress(newProgress);
                    
                    if (!wasCompleted && achievement.IsCompleted)
                    {
                        OnAchievementUnlocked(achievement);
                        player.TotalAchievementPoints += achievement.Points;
                        if (!player.CompletedAchievements.Contains(achievement.Id))
                        {
                            player.CompletedAchievements.Add(achievement.Id);
                        }
                    }
                }
            }
        }

        private bool IsRareMineral(string mineralType)
        {
            var rareMinerals = new[] { "Gold", "Ruby", "Diamond", "Emerald", "Sapphire" };
            return rareMinerals.Contains(mineralType);
        }

        private bool IsLegendaryMineral(string mineralType)
        {
            var legendaryMinerals = new[] { "Void Crystal", "Temporal Gem", "Antimatter Fragment", "Reality Shard" };
            return legendaryMinerals.Contains(mineralType);
        }

        private int GetLocationMiningCount(Player player, string locationId, string achievementId)
        {
            // This would need to be tracked in player mining history
            return player.MiningHistory?.Count(session => session.LocationId == locationId) ?? 0;
        }

        private int GetQuantumMaterialCount(Player player)
        {
            var quantumMinerals = new[] { "Void Crystal", "Temporal Gem", "Antimatter Fragment", "Reality Shard", "Quantum Dust", "Dimensional Ore" };
            return quantumMinerals.Sum(mineral => (int)player.MineralStats.GetValueOrDefault(mineral, 0));
        }

        public void SyncPlayerAchievements(Player player)
        {
            // Update achievement progress based on current player state
            foreach (var achievement in _achievementSystem.Achievements)
            {
                if (player.CompletedAchievements.Contains(achievement.Id))
                {
                    achievement.IsCompleted = true;
                    achievement.CompletedDate = DateTime.Parse("2025-07-31 13:29:22");
                    achievement.CurrentProgress = achievement.Target;
                }
            }
        }

        public double GetCompletionPercentage()
        {
            return _achievementSystem.GetCompletionPercentage();
        }

        public int GetTotalAchievementPoints()
        {
            return _achievementSystem.GetTotalAchievementPoints();
        }

        public List<string> GetAchievementCategories()
        {
            return _achievementSystem.Achievements
                .Where(a => !a.IsHidden || a.IsCompleted)
                .Select(a => a.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public Dictionary<string, int> GetCategoryProgress()
        {
            var categories = GetAchievementCategories();
            var progress = new Dictionary<string, int>();

            foreach (var category in categories)
            {
                var categoryAchievements = GetAchievementsByCategory(category);
                var completed = categoryAchievements.Count(a => a.IsCompleted);
                var total = categoryAchievements.Count;
                progress[category] = total > 0 ? (int)((double)completed / total * 100) : 0;
            }

            return progress;
        }

        private void OnAchievementUnlocked(Achievement achievement)
        {
            AchievementUnlocked?.Invoke(this, achievement);
        }

        public void InitializePlayerAchievements(Player player)
        {
            SyncPlayerAchievements(player);
            
            // Set up initial progress based on player stats
            CheckAndUpdateAchievements(player, "money_earned", null);
            CheckAndUpdateAchievements(player, "mining_completed", null);
            CheckAndUpdateAchievements(player, "rank_changed", null);
        }
    }
}