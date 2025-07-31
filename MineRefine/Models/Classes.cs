using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MineRefine.Models
{
    #region Core Enums

    public enum Difficulty
    {
        NOVICE,
        EXPERIENCED,
        EXPERT,
        LEGENDARY
    }

    public enum Rank
    {
        BEGINNER,
        NOVICE_MINER,
        INTERMEDIATE,
        EXPERIENCED_MINER,
        EXPERT,
        EXPERT_MINER,
        MASTER_MINER,
        LEGENDARY_MINER,
        ASCENDED_MINER
    }

    public enum WeatherCondition
    {
        Clear,
        Cloudy,
        Rainy,
        Stormy,
        Foggy,
        Snowy,
        Windy,
        QuantumFlux,
        TemporalStorm,
        RealityDistortion,
        CosmicRadiation,
        DimensionalRift
    }

    public enum MineralType
    {
        METALLIC,
        PRECIOUS,
        GEMSTONE,
        CRYSTAL,
        FOSSIL,
        RADIOACTIVE,
        MAGNETIC,
        RARE_EARTH,
        GANGUE,
        EXOTIC_MATTER,
        BASE_METAL
    }

    public enum EquipmentType
    {
        Pickaxe,
        Helmet,
        Boots,
        Gloves,
        Scanner,
        Drill,
        Suit,
        Backpack,
        Lamp,
        Detector,
        Stabilizer,
        Shield
    }

    public enum AchievementType
    {
        TotalMoney,
        TotalMines,
        SpecificMineral,
        RankAdvancement,
        LocationDiscovery,
        LocationUnlock,
        EquipmentUpgrade,
        ConsecutiveMines,
        ConsecutiveSuccess,
        WeatherSurvival,
        QuantumMastery,
        SkillPoints
    }

    public enum EventType
    {
        Economic,
        Special,
        Seasonal,
        Emergency,
        Promotional,
        Discovery,
        Technical,
        Weather,
        Quantum
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Achievement,
        Discovery,
        System,
        Market
    }

    #endregion

    #region Core Player System

    public class Player : INotifyPropertyChanged
    {
        // Core Player Properties
        public string Name { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.EXPERIENCED;
        public Rank Rank { get; set; } = Rank.NOVICE_MINER;
        public int Level { get; set; } = 1;
        public long TotalMoney { get; set; } = 0;
        public long Debt { get; set; } = 0;
        public long TotalEarnings { get; set; } = 0;
        public long TotalMinesCount { get; set; } = 0;
        public int Stamina { get; set; } = 100;
        public int MaxStamina { get; set; } = 100;
        public long ExperiencePoints { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;
        public double Multiplier { get; set; } = 1.0;

        // Timestamps - Updated to current time
        public DateTime CreatedDate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime LastPlayed { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime LastLogin { get; set; } = DateTime.Parse("2025-07-31 13:29:22");

        // Location and Progression
        public string CurrentLocationId { get; set; } = "surface_mine";
        public List<string> UnlockedLocations { get; set; } = new();
        public List<string> UnlockedSkills { get; set; } = new();
        public List<string> CompletedAchievements { get; set; } = new();
        public List<string> CompletedTutorials { get; set; } = new();
        public List<string> UnlockedNarratives { get; set; } = new();

        // Statistics and Collections
        public Dictionary<string, long> MineralStats { get; set; } = new();
        public Dictionary<string, DateTime> LocationFirstVisit { get; set; } = new();
        public Dictionary<string, int> DailyMiningStats { get; set; } = new();
        public List<Equipment> Equipment { get; set; } = new();

        // Phase 1: Enhanced Features
        public List<MiningSession> MiningHistory { get; set; } = new();
        public List<GameSession> SessionHistory { get; set; } = new();
        public Dictionary<string, int> WeatherEncounters { get; set; } = new();
        public Dictionary<string, double> LocationSuccessRates { get; set; } = new();

        // Upgrades and Equipment
        public bool PowerDrill { get; set; } = false;
        public bool LuckyCharm { get; set; } = false;
        public bool Gpr { get; set; } = false;
        public bool RefineryUpgrade { get; set; } = false;
        public bool MagneticSurvey { get; set; } = false;

        // Enhanced Player Properties
        public string Avatar { get; set; } = "classic_miner";
        public int SessionCount { get; set; } = 0;
        public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
        public int ConsecutiveSuccessfulMines { get; set; } = 0;
        public int BestMiningStreak { get; set; } = 0;
        public long SingleBestMineValue { get; set; } = 0;
        public string PreferredDifficulty { get; set; } = "EXPERIENCED";
        public bool TutorialCompleted { get; set; } = false;

        // Auto-Mining Settings
        public bool AutoMiningEnabled { get; set; } = false;
        public int AutoMiningInterval { get; set; } = 3; // seconds
        public double AutoMiningStaminaThreshold { get; set; } = 20.0;

        // Player Methods
        public void AddExperience(int amount)
        {
            ExperiencePoints += Math.Max(0, amount);

            if (amount > 0)
            {
                ConsecutiveSuccessfulMines++;
                BestMiningStreak = Math.Max(BestMiningStreak, ConsecutiveSuccessfulMines);
            }
            else
            {
                ConsecutiveSuccessfulMines = 0;
            }

            // Check for level up
            var requiredXP = Level * 1000;
            if (ExperiencePoints >= requiredXP)
            {
                Level++;
                SkillPoints += 2;
                ExperiencePoints -= requiredXP;
            }
        }

        public void RecordMineValue(long value)
        {
            SingleBestMineValue = Math.Max(SingleBestMineValue, value);

            var today = DateTime.Parse("2025-07-31").ToString("yyyy-MM-dd");
            if (!DailyMiningStats.ContainsKey(today))
            {
                DailyMiningStats[today] = 0;
            }
            DailyMiningStats[today]++;
        }

        public bool HasVisitedLocation(string locationId)
        {
            return LocationFirstVisit.ContainsKey(locationId);
        }

        public void VisitLocation(string locationId)
        {
            if (!HasVisitedLocation(locationId))
            {
                LocationFirstVisit[locationId] = DateTime.Parse("2025-07-31 13:29:22");
            }
        }

        public double GetSuccessRate()
        {
            if (TotalMinesCount == 0) return 100.0;
            return ((double)ConsecutiveSuccessfulMines / TotalMinesCount) * 100.0;
        }

        public int GetMiningStreak()
        {
            return ConsecutiveSuccessfulMines;
        }

        public double GetLocationSuccessRate(string locationId)
        {
            if (LocationSuccessRates.ContainsKey(locationId))
            {
                return LocationSuccessRates[locationId];
            }
            return 0.0;
        }

        public void UpdateLocationSuccessRate(string locationId, bool success)
        {
            if (!LocationSuccessRates.ContainsKey(locationId))
            {
                LocationSuccessRates[locationId] = success ? 100.0 : 0.0;
            }
            else
            {
                var current = LocationSuccessRates[locationId];
                LocationSuccessRates[locationId] = (current + (success ? 100.0 : 0.0)) / 2.0;
            }
        }

        public List<Equipment> GetEquippedItems()
        {
            return Equipment?.Where(e => e.IsEquipped).ToList() ?? new List<Equipment>();
        }

        public double GetEquipmentBonus(string bonusType)
        {
            var equippedItems = GetEquippedItems();
            return equippedItems.Sum(e => e.Bonuses.GetValueOrDefault(bonusType, 0.0));
        }

        public void ConsumeStamina(int amount)
        {
            Stamina = Math.Max(0, Stamina - amount);
        }

        public void RestoreStamina(int amount)
        {
            Stamina = Math.Min(MaxStamina, Stamina + amount);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Mining System

    public class MiningSession : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = "Valinor-70";
        public string LocationId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double RiskMultiplier { get; set; } = 1.0;
        public int StaminaCost { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
        public List<MineResult> Results { get; set; } = new();
        public Dictionary<string, object> SessionData { get; set; } = new();

        // Enhanced Properties
        public List<string> EnvironmentalHazards { get; set; } = new();
        public Dictionary<string, double> EquipmentBonuses { get; set; } = new();
        public long TotalValue { get; set; } = 0;
        public int SuccessfulOperations { get; set; } = 0;
        public int FailedOperations { get; set; } = 0;
        public string SessionType { get; set; } = "Manual"; // Manual, Auto, Quantum

        public int RiskLevel
        {
            get
            {
                return RiskMultiplier switch
                {
                    <= 1.2 => 1,
                    <= 1.6 => 2,
                    <= 2.0 => 3,
                    <= 2.5 => 4,
                    _ => 5
                };
            }
        }

        public double GetSuccessRate()
        {
            var total = SuccessfulOperations + FailedOperations;
            if (total == 0) return 0.0;
            return ((double)SuccessfulOperations / total) * 100.0;
        }

        public void AddResult(MineResult result)
        {
            Results.Add(result);
            TotalValue += result.Value;

            if (result.IsSuccess)
                SuccessfulOperations++;
            else
                FailedOperations++;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MineResult : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsSuccess { get; set; }
        public Mineral? Mineral { get; set; }
        public long Value { get; set; }
        public MiningLocation? Location { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExperienceGained { get; set; }
        public string? BonusDiscovery { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        public WeatherCondition WeatherAtTime { get; set; } = WeatherCondition.Clear;

        // Enhanced Properties
        public List<string> EnvironmentalFactors { get; set; } = new();
        public Dictionary<string, double> AppliedBonuses { get; set; } = new();
        public string FailureReason { get; set; } = string.Empty;
        public int EquipmentWear { get; set; } = 0;
        public double OperationDifficulty { get; set; } = 1.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Mineral System

    public class Mineral : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Value { get; set; } = 0;
        public MineralType MineralType { get; set; } = MineralType.METALLIC;
        public long RefinedValue { get; set; } = 0;
        public double Weight { get; set; } = 1.0;
        public string Rarity { get; set; } = "Common";
        public string Icon { get; set; } = "🪨";
        public string Color { get; set; } = "Gray";
        public string UpgradeRequired { get; set; } = string.Empty;

        // Enhanced Properties
        public List<string> FoundInLocations { get; set; } = new();
        public bool IsRadioactive { get; set; } = false;
        public bool IsMagnetic { get; set; } = false;
        public bool IsLegendary { get; set; } = false;
        public Dictionary<string, double> ProcessingRequirements { get; set; } = new();
        public List<string> SpecialProperties { get; set; } = new();
        public DateTime DiscoveredDate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public string DiscoveredBy { get; set; } = "Valinor-70";

        // Market Properties
        public double CurrentMarketMultiplier { get; set; } = 1.0;
        public string MarketTrend { get; set; } = "Stable";
        public DateTime LastPriceUpdate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public List<double> PriceHistory { get; set; } = new();
        public double DailyVolume { get; set; } = 0.0;
        public double SupplyLevel { get; set; } = 100.0;
        public double DemandLevel { get; set; } = 100.0;

        public string GetTypeEmoji()
        {
            return MineralType switch
            {
                MineralType.METALLIC => "⚡",
                MineralType.PRECIOUS => "✨",
                MineralType.GEMSTONE => "💎",
                MineralType.CRYSTAL => "🔮",
                MineralType.FOSSIL => "🦕",
                MineralType.RADIOACTIVE => "☢️",
                MineralType.MAGNETIC => "🧲",
                MineralType.RARE_EARTH => "🌍",
                MineralType.GANGUE => "🪨",
                _ => "⭕"
            };
        }

        public int GetRarityLevel()
        {
            return Rarity switch
            {
                "Common" => 1,
                "Uncommon" => 2,
                "Rare" => 3,
                "Epic" => 4,
                "Legendary" => 5,
                "Mythical" => 6,
                "Quantum" => 7,
                _ => 1
            };
        }

        public string GetRarityColor()
        {
            return Rarity switch
            {
                "Common" => "#FFFFFF",
                "Uncommon" => "#1EFF00",
                "Rare" => "#0070DD",
                "Epic" => "#A335EE",
                "Legendary" => "#FF8000",
                "Mythical" => "#E6CC80",
                "Quantum" => "#00FFFF",
                _ => "#FFFFFF"
            };
        }

        public double GetMarketVolatility()
        {
            return GetRarityLevel() switch
            {
                1 => 0.1,
                2 => 0.15,
                3 => 0.25,
                4 => 0.35,
                5 => 0.45,
                6 => 0.55,
                7 => 0.65,
                _ => 0.1
            };
        }

        public bool IsQuantumMaterial()
        {
            return MineralType == MineralType.CRYSTAL &&
                   (Rarity == "Quantum" || SpecialProperties.Contains("Quantum_Entangled"));
        }

        public long GetProcessedValue()
        {
            var baseValue = RefinedValue > 0 ? RefinedValue : Value;
            var rarityMultiplier = GetRarityLevel() * 0.2 + 1.0;
            var marketMultiplier = CurrentMarketMultiplier;

            return (long)(baseValue * rarityMultiplier * marketMultiplier);
        }

        public void UpdateMarketPrice(double changePercent)
        {
            CurrentMarketMultiplier *= (1.0 + changePercent / 100.0);
            CurrentMarketMultiplier = Math.Max(0.1, Math.Min(5.0, CurrentMarketMultiplier));

            PriceHistory.Add(CurrentMarketMultiplier);
            if (PriceHistory.Count > 100)
            {
                PriceHistory.RemoveAt(0);
            }

            LastPriceUpdate = DateTime.Parse("2025-07-31 13:29:22");

            MarketTrend = changePercent switch
            {
                > 5 => "Bullish",
                > 1 => "Rising",
                < -5 => "Bearish",
                < -1 => "Declining",
                _ => "Stable"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Location System

    public class MiningLocation : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🏔️";
        public string RequiredRank { get; set; } = "NOVICE_MINER";
        public int RequiredLevel { get; set; } = 1;
        public long UnlockCost { get; set; } = 0;
        public bool IsUnlocked { get; set; } = false;
        public int DangerLevel { get; set; } = 1;
        public int Depth { get; set; } = 1;
        public double StaminaCost { get; set; } = 1.0;
        public string BackgroundColor { get; set; } = "#8B7355";
        public string Climate { get; set; } = "Temperate";

        // Enhanced Properties
        public Dictionary<string, double> MineralBonuses { get; set; } = new();
        public List<string> UniqueMinerals { get; set; } = new();
        public List<string> EnvironmentalHazards { get; set; } = new();
        public string Narrative { get; set; } = string.Empty;
        public Dictionary<string, string> EnvironmentalConditions { get; set; } = new();
        public List<string> RequiredEquipment { get; set; } = new();
        public double SuccessRateModifier { get; set; } = 1.0;
        public DateTime LastVisited { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public int TimesVisited { get; set; } = 0;
        public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;
        public Dictionary<string, int> MineralDiscoveryCount { get; set; } = new();

        // Advanced Location Features
        public Dictionary<string, double> WeatherProbabilities { get; set; } = new();
        public List<string> SeasonalEffects { get; set; } = new();
        public double BaseSuccessRate { get; set; } = 0.8;
        public Dictionary<string, double> HazardProbabilities { get; set; } = new();
        public string LocationTheme { get; set; } = "Standard";
        public bool SupportsQuantumMining { get; set; } = false;

        public double GetWeatherModifier(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Clear => 1.0,
                WeatherCondition.Cloudy => 0.95,
                WeatherCondition.Rainy => 0.85,
                WeatherCondition.Stormy => 0.7,
                WeatherCondition.Foggy => 0.9,
                WeatherCondition.Snowy => 0.8,
                WeatherCondition.Windy => 0.9,
                WeatherCondition.QuantumFlux => 1.3,
                WeatherCondition.TemporalStorm => 1.5,
                WeatherCondition.RealityDistortion => 0.6,
                WeatherCondition.CosmicRadiation => 1.2,
                WeatherCondition.DimensionalRift => 0.4,
                _ => 1.0
            };
        }

        public List<string> GetActiveHazards()
        {
            var activeHazards = new List<string>();
            var random = new Random();

            foreach (var hazard in EnvironmentalHazards)
            {
                var probability = HazardProbabilities.GetValueOrDefault(hazard, 0.1);
                if (random.NextDouble() < probability)
                {
                    activeHazards.Add(hazard);
                }
            }

            return activeHazards;
        }

        public void Visit()
        {
            TimesVisited++;
            LastVisited = DateTime.Parse("2025-07-31 13:29:22");
        }

        public double GetMineralBonus(string mineralId)
        {
            return MineralBonuses.GetValueOrDefault(mineralId, 1.0);
        }

        public void UpdateWeather()
        {
            var random = new Random();
            var weatherOptions = Id switch
            {
                "surface_mine" => new[] { WeatherCondition.Clear, WeatherCondition.Cloudy, WeatherCondition.Rainy, WeatherCondition.Windy },
                "deep_caves" => new[] { WeatherCondition.Foggy, WeatherCondition.Clear, WeatherCondition.Stormy },
                "volcanic_depths" => new[] { WeatherCondition.Clear, WeatherCondition.Stormy, WeatherCondition.Foggy },
                "quantum_realm" => new[] { WeatherCondition.QuantumFlux, WeatherCondition.TemporalStorm, WeatherCondition.RealityDistortion },
                _ => new[] { WeatherCondition.Clear }
            };

            CurrentWeather = weatherOptions[random.Next(weatherOptions.Length)];
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Equipment System

    public class Equipment : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EquipmentType Type { get; set; } = EquipmentType.Pickaxe;
        public int Durability { get; set; } = 100;
        public int MaxDurability { get; set; } = 100;
        public int Level { get; set; } = 1;
        public string Icon { get; set; } = "⛏️";
        public string Rarity { get; set; } = "Common";
        public Dictionary<string, double> Bonuses { get; set; } = new();
        public long UpgradeCost { get; set; } = 0;
        public long PurchaseCost { get; set; } = 0;
        public bool IsEquipped { get; set; } = false;
        public List<string> SpecialAbilities { get; set; } = new();

        // Enhanced Properties
        public DateTime AcquiredDate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public int TimesUsed { get; set; } = 0;
        public List<string> RequiredSkills { get; set; } = new();
        public Dictionary<string, string> UpgradeTree { get; set; } = new();
        public string Manufacturer { get; set; } = "Mining Corp";
        public double EfficiencyRating { get; set; } = 1.0;

        // Advanced Equipment Features
        public int WearRate { get; set; } = 1;
        public Dictionary<string, double> EnvironmentalResistance { get; set; } = new();
        public List<string> CompatibleLocations { get; set; } = new();
        public DateTime LastMaintenance { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public double MaintenanceCostMultiplier { get; set; } = 1.0;
        public string Condition { get; set; } = "Excellent";

        public bool CanUpgrade
        {
            get
            {
                return Level < 10 &&
                       Durability > 0 &&
                       !string.IsNullOrEmpty(Name) &&
                       UpgradeCost > 0;
            }
        }

        public double GetDurabilityPercentage()
        {
            if (MaxDurability == 0) return 0.0;
            return ((double)Durability / MaxDurability) * 100.0;
        }

        public void Use(int wearAmount = -1)
        {
            TimesUsed++;
            var actualWear = wearAmount > 0 ? wearAmount : WearRate;
            Durability = Math.Max(0, Durability - actualWear);

            UpdateCondition();
        }

        public void Repair(int amount = -1)
        {
            var repairAmount = amount > 0 ? amount : MaxDurability - Durability;
            Durability = Math.Min(MaxDurability, Durability + repairAmount);
            LastMaintenance = DateTime.Parse("2025-07-31 13:29:22");

            UpdateCondition();
        }

        public void Upgrade()
        {
            if (CanUpgrade)
            {
                Level++;
                MaxDurability = (int)(MaxDurability * 1.2);
                Durability = MaxDurability;
                UpgradeCost = (long)(UpgradeCost * 1.5);
                EfficiencyRating *= 1.1;

                // Improve bonuses
                foreach (var bonus in Bonuses.Keys.ToList())
                {
                    Bonuses[bonus] *= 1.1;
                }

                UpdateCondition();
            }
        }

        private void UpdateCondition()
        {
            var durabilityPercent = GetDurabilityPercentage();
            Condition = durabilityPercent switch
            {
                >= 90 => "Excellent",
                >= 70 => "Good",
                >= 50 => "Fair",
                >= 25 => "Poor",
                > 0 => "Critical",
                _ => "Broken"
            };
        }

        public double GetBonus(string bonusType)
        {
            var baseBonus = Bonuses.GetValueOrDefault(bonusType, 0.0);
            var conditionMultiplier = GetDurabilityPercentage() / 100.0;
            return baseBonus * conditionMultiplier;
        }

        public long GetMaintenanceCost()
        {
            var baseCost = (long)((MaxDurability - Durability) * 10);
            return (long)(baseCost * MaintenanceCostMultiplier);
        }

        public bool RequiresMaintenance()
        {
            return Durability < MaxDurability * 0.5 ||
                   DateTime.Parse("2025-07-31 13:29:22").Subtract(LastMaintenance).TotalDays > 7;
        }

        public string GetRarityColor()
        {
            return Rarity switch
            {
                "Common" => "#FFFFFF",
                "Uncommon" => "#1EFF00",
                "Rare" => "#0070DD",
                "Epic" => "#A335EE",
                "Legendary" => "#FF8000",
                "Mythical" => "#E6CC80",
                "Quantum" => "#00FFFF",
                _ => "#FFFFFF"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Achievement System

    public class Achievement : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AchievementType Type { get; set; } = AchievementType.TotalMoney;
        public long Target { get; set; } = 0;
        public string Icon { get; set; } = "🏆";
        public long RewardMoney { get; set; } = 0;
        public int RewardSkillPoints { get; set; } = 0;
        public string? RewardUpgrade { get; set; }
        public string Category { get; set; } = "General";
        public string Difficulty { get; set; } = "Normal";
        public bool IsCompleted { get; set; } = false;
        public bool IsSecret { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        public string? CompletedBy { get; set; }
        public DateTime? UnlockedDate { get; set; }
        public double ProgressPercentage { get; set; } = 0.0;

        // Enhanced Properties
        public List<string> Prerequisites { get; set; } = new();
        public Dictionary<string, object> Conditions { get; set; } = new();
        public string CompletionMessage { get; set; } = string.Empty;
        public int Points { get; set; } = 100;
        public List<string> Tags { get; set; } = new();

        public void UpdateProgress(long currentValue)
        {
            if (IsCompleted || Target == 0) return;

            ProgressPercentage = Math.Min(100.0, ((double)currentValue / Target) * 100.0);

            if (currentValue >= Target && !IsCompleted)
            {
                Complete();
            }
        }

        public void Complete()
        {
            IsCompleted = true;
            CompletedDate = DateTime.Parse("2025-07-31 13:29:22");
            CompletedBy = "Valinor-70";
            ProgressPercentage = 100.0;
        }

        public string GetDifficultyColor()
        {
            return Difficulty switch
            {
                "Easy" => "#00FF00",
                "Normal" => "#FFFF00",
                "Hard" => "#FF8000",
                "Epic" => "#A335EE",
                "Legendary" => "#FF8000",
                "Ultimate" => "#E6CC80",
                _ => "#FFFFFF"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Market System

    public class MarketData : INotifyPropertyChanged
    {
        public string MineralId { get; set; } = string.Empty;
        public string MineralName { get; set; } = string.Empty;
        public long CurrentPrice { get; set; } = 0;
        public double PriceChange { get; set; } = 0.0;
        public string Trend { get; set; } = "Stable";
        public double Volume { get; set; } = 0.0;
        public DateTime LastUpdate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public List<double> PriceHistory { get; set; } = new();
        public double Volatility { get; set; } = 0.1;

        // Enhanced Market Features
        public double Supply { get; set; } = 100.0;
        public double Demand { get; set; } = 100.0;
        public List<string> MarketEvents { get; set; } = new();
        public Dictionary<string, double> RegionalPrices { get; set; } = new();
        public double FuturesPriceEstimate { get; set; } = 0.0;

        // Additional Properties
        public double PriceMultiplier { get; set; } = 1.0;
        public string TrendIcon { get; set; } = "📈";
        public DateTime LastUpdated { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public string MarketSentiment { get; set; } = "Neutral";

        public void UpdatePrice(double changePercent)
        {
            var oldPrice = CurrentPrice;
            CurrentPrice = (long)(CurrentPrice * (1.0 + changePercent / 100.0));
            PriceChange = changePercent;
            PriceMultiplier = 1.0 + changePercent / 100.0;

            PriceHistory.Add(CurrentPrice);
            if (PriceHistory.Count > 168) // Keep 1 week of hourly data
            {
                PriceHistory.RemoveAt(0);
            }

            LastUpdate = DateTime.Parse("2025-07-31 13:29:22");
            LastUpdated = LastUpdate;

            Trend = changePercent switch
            {
                > 5 => "Bullish",
                > 1 => "Rising",
                < -5 => "Bearish",
                < -1 => "Declining",
                _ => "Stable"
            };

            TrendIcon = changePercent switch
            {
                > 0 => "📈",
                < 0 => "📉",
                _ => "📊"
            };

            MarketSentiment = changePercent switch
            {
                > 10 => "Very Bullish",
                > 5 => "Bullish",
                > 1 => "Optimistic",
                < -10 => "Very Bearish",
                < -5 => "Bearish",
                < -1 => "Pessimistic",
                _ => "Neutral"
            };
        }

        public double GetMovingAverage(int periods)
        {
            if (PriceHistory.Count < periods) return CurrentPrice;

            var recentPrices = PriceHistory.TakeLast(periods);
            return recentPrices.Average();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MarketEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime EndTime { get; set; }
        public List<string> AffectedMinerals { get; set; } = new();
        public Dictionary<string, double> PriceMultipliers { get; set; } = new();
        public string EventType { get; set; } = "Economic";
        public string Severity { get; set; } = "Minor";
        public bool IsActive { get; set; } = true;

        public bool IsCurrentlyActive()
        {
            var now = DateTime.Parse("2025-07-31 13:29:22");
            return IsActive && now >= StartTime && now <= EndTime;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Event System

    public class EconomicEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(24);
        public string EventType { get; set; } = "Market";
        public string Severity { get; set; } = "Minor";
        public bool IsActive { get; set; } = true;
        public bool IsGlobal { get; set; } = true;

        // Economic Properties
        public List<string> AffectedMinerals { get; set; } = new();
        public Dictionary<string, double> PriceMultipliers { get; set; } = new();
        public Dictionary<string, double> SupplyEffects { get; set; } = new();
        public Dictionary<string, double> DemandEffects { get; set; } = new();
        public double OverallMarketImpact { get; set; } = 0.0;

        // Event Details
        public string Trigger { get; set; } = "Random";
        public List<string> Prerequisites { get; set; } = new();
        public Dictionary<string, object> EventData { get; set; } = new();
        public List<string> NewsReports { get; set; } = new();
        public string IconEmoji { get; set; } = "📈";

        // Additional Properties
        public string Icon { get; set; } = "📈";
        public DateTime StartDate { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public int DurationDays { get; set; } = 1;
        public Dictionary<string, double> MineralEffects { get; set; } = new();

        public bool IsCurrentlyActive()
        {
            var now = DateTime.Parse("2025-07-31 13:29:22");
            return IsActive && now >= StartTime && now <= EndTime;
        }

        public void StartEvent()
        {
            StartTime = DateTime.Parse("2025-07-31 13:29:22");
            StartDate = StartTime;
            EndTime = StartTime.Add(Duration);
            IsActive = true;
        }

        public void EndEvent()
        {
            EndTime = DateTime.Parse("2025-07-31 13:29:22");
            IsActive = false;
        }

        public double GetMineralPriceMultiplier(string mineralId)
        {
            return PriceMultipliers.GetValueOrDefault(mineralId, 1.0);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SpecialEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(7);
        public string EventType { get; set; } = "Special";
        public string Rarity { get; set; } = "Rare";
        public bool IsActive { get; set; } = true;
        public bool IsRepeatable { get; set; } = false;

        // Special Event Properties
        public Dictionary<string, double> Bonuses { get; set; } = new();
        public List<string> UnlockedContent { get; set; } = new();
        public Dictionary<string, object> Rewards { get; set; } = new();
        public List<string> RequiredConditions { get; set; } = new();
        public string CompletionCriteria { get; set; } = string.Empty;

        // Event Mechanics
        public int ParticipationCount { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        public string CompletedBy { get; set; } = string.Empty;
        public Dictionary<string, int> PlayerProgress { get; set; } = new();

        // Visual Properties
        public string IconEmoji { get; set; } = "⭐";
        public string BackgroundColor { get; set; } = "#FFD700";
        public string ThemeMusic { get; set; } = "special_event.mp3";
        public List<string> VisualEffects { get; set; } = new();

        // Additional Properties - Fixed Constructor Issue
        public Dictionary<string, double> Effects { get; set; } = new();
        public string Icon { get; set; } = "⭐";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EventType Type { get; set; }
        public List<string> AffectedLocations { get; set; } = new();

        // Constructor to properly initialize fields - FIXED
        public SpecialEvent()
        {
            var currentTime = DateTime.Parse("2025-07-31 13:29:22");
            StartDate = currentTime;
            EndDate = currentTime.AddDays(7);
            Type = Models.EventType.Special; // Fixed: Use proper enum reference
        }

        public bool IsCurrentlyActive()
        {
            var now = DateTime.Parse("2025-07-31 13:29:22");
            return IsActive && now >= StartTime && now <= EndTime && !IsCompleted;
        }

        public void StartEvent()
        {
            StartTime = DateTime.Parse("2025-07-31 13:29:22");
            StartDate = StartTime;
            EndTime = StartTime.Add(Duration);
            EndDate = EndTime;
            IsActive = true;
            IsCompleted = false;
        }

        public void CompleteEvent(string completedBy)
        {
            IsCompleted = true;
            CompletedDate = DateTime.Parse("2025-07-31 13:29:22");
            CompletedBy = completedBy;
            IsActive = false;
        }

        public void UpdateProgress(string playerId, int progress)
        {
            if (!PlayerProgress.ContainsKey(playerId))
            {
                PlayerProgress[playerId] = 0;
            }
            PlayerProgress[playerId] += progress;
            ParticipationCount++;
        }

        public double GetBonus(string bonusType)
        {
            return Bonuses.GetValueOrDefault(bonusType, 0.0);
        }

        public double GetEffect(string effectType)
        {
            return Effects.GetValueOrDefault(effectType, 0.0);
        }

        public bool MeetsRequirements(Player player)
        {
            foreach (var condition in RequiredConditions)
            {
                if (!EvaluateCondition(condition, player))
                {
                    return false;
                }
            }
            return true;
        }

        private bool EvaluateCondition(string condition, Player player)
        {
            return condition switch
            {
                "level_10+" => player.Level >= 10,
                "has_quantum_access" => player.UnlockedLocations.Contains("quantum_realm"),
                "experienced_miner+" => player.Rank >= Rank.EXPERIENCED_MINER,
                _ => true
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Game Settings and Session

    public class GameSettings : INotifyPropertyChanged
    {
        // Application Settings
        public string CurrentUser { get; set; } = "Valinor-70";
        public DateTime LastUpdated { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public string Version { get; set; } = "1.0.0-alpha";
        public string Theme { get; set; } = "Dark";

        // Audio Settings
        public double MasterVolume { get; set; } = 0.8;
        public bool SoundEffects { get; set; } = true;
        public bool BackgroundMusic { get; set; } = true;
        public bool VoiceNarration { get; set; } = false;

        // Visual Settings
        public double AnimationSpeed { get; set; } = 1.0;
        public bool ReducedAnimations { get; set; } = false;
        public bool HighContrast { get; set; } = false;
        public bool ParticleEffects { get; set; } = true;
        public int TargetFrameRate { get; set; } = 60;

        // Gameplay Settings
        public bool AutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 30;
        public bool ShowTutorials { get; set; } = true;
        public bool ShowTooltips { get; set; } = true;
        public bool ConfirmActions { get; set; } = true;

        // Mining Settings
        public bool AutoMiningEnabled { get; set; } = false;
        public int AutoMiningInterval { get; set; } = 3;
        public bool NotifyOnRareFinds { get; set; } = true;
        public bool ShowMiningParticles { get; set; } = true;

        // Interface Settings
        public double UIScale { get; set; } = 1.0;
        public bool CompactMode { get; set; } = false;
        public string Language { get; set; } = "English";
        public bool ShowAdvancedStats { get; set; } = false;

        // Performance Settings
        public bool ReducedQuality { get; set; } = false;
        public bool BackgroundProcessing { get; set; } = true;
        public int MaxLogEntries { get; set; } = 100;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameSession : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = "Valinor-70";
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Version { get; set; } = "1.0.0-alpha";

        // Session Statistics
        public int MiningOperations { get; set; } = 0;
        public int SuccessfulMines { get; set; } = 0;
        public long MoneyEarned { get; set; } = 0;
        public int ExperienceGained { get; set; } = 0;
        public List<string> LocationsVisited { get; set; } = new();
        public List<string> MinersalsFound { get; set; } = new();
        public Dictionary<string, int> ActionCounts { get; set; } = new();

        // Session Events
        public List<string> Achievements { get; set; } = new();
        public List<string> Notifications { get; set; } = new();
        public Dictionary<string, object> SessionData { get; set; } = new();

        public void EndSession()
        {
            EndTime = DateTime.Parse("2025-07-31 13:29:22");
            Duration = EndTime.Value - StartTime;
        }

        public void RecordAction(string action)
        {
            if (!ActionCounts.ContainsKey(action))
            {
                ActionCounts[action] = 0;
            }
            ActionCounts[action]++;
        }

        public double GetSuccessRate()
        {
            if (MiningOperations == 0) return 0.0;
            return ((double)SuccessfulMines / MiningOperations) * 100.0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Notification System

    public class GameNotification : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public DateTime Timestamp { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public bool IsRead { get; set; } = false;
        public string Icon { get; set; } = "ℹ️";
        public Dictionary<string, object> Data { get; set; } = new();
        public int Priority { get; set; } = 1;
        public TimeSpan DisplayDuration { get; set; } = TimeSpan.FromSeconds(5);

        public string GetTypeColor()
        {
            return Type switch
            {
                NotificationType.Success => "#00FF00",
                NotificationType.Warning => "#FFA500",
                NotificationType.Error => "#FF0000",
                NotificationType.Achievement => "#FFD700",
                NotificationType.Discovery => "#00FFFF",
                _ => "#FFFFFF"
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Save Data Structure

    public class SaveDataV1
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime SavedAt { get; set; } = DateTime.Parse("2025-07-31 13:29:22");
        public string SavedBy { get; set; } = "Valinor-70";
        public List<Player> Players { get; set; } = new();
        public GameSettings Settings { get; set; } = new();
        public List<Achievement> Achievements { get; set; } = new();
        public List<MarketData> MarketHistory { get; set; } = new();
        public Dictionary<string, object> GameState { get; set; } = new();
        public List<GameSession> SessionHistory { get; set; } = new();
        public long SaveFileSize { get; set; } = 0;
        public string Checksum { get; set; } = string.Empty;

        // Enhanced Save Features
        public Dictionary<string, DateTime> LocationFirstVisits { get; set; } = new();
        public List<string> UnlockedFeatures { get; set; } = new();
        public Dictionary<string, int> GlobalStatistics { get; set; } = new();
        public List<GameNotification> NotificationHistory { get; set; } = new();
        public List<EconomicEvent> EconomicEvents { get; set; } = new();
        public List<SpecialEvent> SpecialEvents { get; set; } = new();

        public void UpdateSaveInfo()
        {
            SavedAt = DateTime.Parse("2025-07-31 13:29:22");
            SavedBy = "Valinor-70";
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Version) &&
                   Players != null &&
                   Settings != null &&
                   SavedAt != default;
        }

        public void AddGlobalStat(string key, int value)
        {
            if (!GlobalStatistics.ContainsKey(key))
            {
                GlobalStatistics[key] = 0;
            }
            GlobalStatistics[key] += value;
        }
    }

    #endregion

    #region Utility Classes

    public static class GameConstants
    {
        // Current Game Info
        public const string CURRENT_VERSION = "1.0.0-alpha";
        public const string CURRENT_USER = "Valinor-70";
        public const string CURRENT_DATETIME = "2025-07-31 13:29:22";

        // Game Balance Constants
        public const int BASE_STAMINA = 100;
        public const int BASE_EXPERIENCE_PER_LEVEL = 1000;
        public const double BASE_SUCCESS_RATE = 0.8;
        public const int MAX_EQUIPMENT_LEVEL = 10;
        public const int MAX_PLAYER_LEVEL = 100;

        // Mining Constants
        public const int MIN_MINING_STAMINA = 10;
        public const double MIN_RISK_MULTIPLIER = 1.0;
        public const double MAX_RISK_MULTIPLIER = 3.0;
        public const int AUTO_SAVE_INTERVAL = 30;

        // File System Constants
        public const string SAVE_FILE_NAME = "ntdll.sys";
        public const string SETTINGS_FILE_NAME = "kernel32.sys";
        public const string APP_DATA_FOLDER = "MineRefine";

        // UI Constants
        public const int MAX_LOG_ENTRIES = 100;
        public const int NOTIFICATION_DISPLAY_SECONDS = 6;
        public const double PARTICLE_ANIMATION_SPEED = 1.0;

        // Feature Flags
        public const bool ENABLE_ANIMATIONS = true;
        public const bool ENABLE_AUTO_MINING = true;
        public const bool ENABLE_WEATHER_SYSTEM = true;
        public const bool ENABLE_EQUIPMENT_WEAR = true;
        public const bool ENABLE_LOCATION_UNLOCKING = true;

        // Rarity Colors
        public static readonly Dictionary<string, string> RarityColors = new()
        {
            {"Common", "#FFFFFF"},
            {"Uncommon", "#1EFF00"},
            {"Rare", "#0070DD"},
            {"Epic", "#A335EE"},
            {"Legendary", "#FF8000"},
            {"Mythical", "#E6CC80"},
            {"Quantum", "#00FFFF"}
        };

        // Weather Icons
        public static readonly Dictionary<WeatherCondition, string> WeatherIcons = new()
        {
            {WeatherCondition.Clear, "☀️"},
            {WeatherCondition.Cloudy, "☁️"},
            {WeatherCondition.Rainy, "🌧️"},
            {WeatherCondition.Stormy, "⛈️"},
            {WeatherCondition.Foggy, "🌫️"},
            {WeatherCondition.Snowy, "❄️"},
            {WeatherCondition.Windy, "💨"},
            {WeatherCondition.QuantumFlux, "🌌"},
            {WeatherCondition.TemporalStorm, "⏰"},
            {WeatherCondition.RealityDistortion, "🌀"},
            {WeatherCondition.CosmicRadiation, "☢️"},
            {WeatherCondition.DimensionalRift, "🌀"}
        };
    }

    public static class FormatUtils
    {
        public static string FormatMoney(long amount)
        {
            return amount switch
            {
                >= 1000000000 => $"£{amount / 1000000000.0:F1}B",
                >= 1000000 => $"£{amount / 1000000.0:F1}M",
                >= 1000 => $"£{amount / 1000.0:F1}K",
                _ => $"£{amount:N0}"
            };
        }

        public static string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        public static string FormatPercentage(double value)
        {
            return $"{value:F1}%";
        }

        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetTimeSince(DateTime dateTime)
        {
            var now = DateTime.Parse("2025-07-31 13:29:22");
            var diff = now - dateTime;

            return diff.TotalDays switch
            {
                >= 365 => $"{(int)(diff.TotalDays / 365)} year(s) ago",
                >= 30 => $"{(int)(diff.TotalDays / 30)} month(s) ago",
                >= 7 => $"{(int)(diff.TotalDays / 7)} week(s) ago",
                >= 1 => $"{(int)diff.TotalDays} day(s) ago",
                _ => diff.TotalHours >= 1 ? $"{(int)diff.TotalHours} hour(s) ago" : "Recently"
            };
        }
    }

    public static class ValidationUtils
    {
        public static bool IsValidPlayerName(string name)
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

        public static bool IsValidMoney(long amount)
        {
            return amount >= 0 && amount <= long.MaxValue / 2;
        }

        public static bool IsValidLevel(int level)
        {
            return level >= 1 && level <= GameConstants.MAX_PLAYER_LEVEL;
        }

        public static bool IsValidStamina(int stamina, int maxStamina)
        {
            return stamina >= 0 && stamina <= maxStamina && maxStamina > 0;
        }

        public static bool IsValidRiskMultiplier(double multiplier)
        {
            return multiplier >= GameConstants.MIN_RISK_MULTIPLIER &&
                   multiplier <= GameConstants.MAX_RISK_MULTIPLIER;
        }
    }

    #endregion

    #region Extension Methods

    public static class ExtensionMethods
    {
        public static T GetRandomElement<T>(this List<T> list)
        {
            if (list.Count == 0) throw new InvalidOperationException("List is empty");
            var random = new Random();
            return list[random.Next(list.Count)];
        }

        public static void AddIfNotExists<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        public static double Clamp(this double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public static string ReplaceUnderscores(this string input)
        {
            return input?.Replace("_", " ") ?? string.Empty;
        }

        public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime <= end;
        }

        public static bool IsBetween(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public static string ToFormattedString(this TimeSpan timeSpan)
        {
            return FormatUtils.FormatTime(timeSpan);
        }

        public static string ToMoneyString(this long amount)
        {
            return FormatUtils.FormatMoney(amount);
        }

        public static string ToPercentageString(this double value)
        {
            return FormatUtils.FormatPercentage(value);
        }

        public static bool IsValid(this Player player)
        {
            return ValidationUtils.IsValidPlayerName(player.Name) &&
                   ValidationUtils.IsValidLevel(player.Level) &&
                   ValidationUtils.IsValidMoney(player.TotalMoney) &&
                   ValidationUtils.IsValidStamina(player.Stamina, player.MaxStamina);
        }
    }

    #endregion
}