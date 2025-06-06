using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MineRefine.Models
{
    #region Core Player System

    public class Player : INotifyPropertyChanged
    {
        // Core Player Properties
        public string Name { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.NOVICE;
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
        public DateTime CreatedDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime LastPlayed { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime LastLogin { get; set; } = DateTime.Parse("2025-06-06 21:41:31");

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

        // Upgrades and Equipment
        public bool PowerDrill { get; set; } = false;
        public bool LuckyCharm { get; set; } = false;
        public bool Gpr { get; set; } = false;
        public bool RefineryUpgrade { get; set; } = false;
        public bool MagneticSurvey { get; set; } = false;

        // Enhanced Player Properties - Alpha Version
        public string Avatar { get; set; } = "classic_miner";
        public int SessionCount { get; set; } = 0;
        public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
        public int ConsecutiveSuccessfulMines { get; set; } = 0;
        public int BestMiningStreak { get; set; } = 0;
        public long SingleBestMineValue { get; set; } = 0;
        public string PreferredDifficulty { get; set; } = "EXPERT";
        public bool TutorialCompleted { get; set; } = false;

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
        }

        public void RecordMineValue(long value)
        {
            SingleBestMineValue = Math.Max(SingleBestMineValue, value);

            var today = DateTime.Parse("2025-06-06").ToString("yyyy-MM-dd");
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
                LocationFirstVisit[locationId] = DateTime.Parse("2025-06-06 21:41:31");
            }
        }

        public double GetSuccessRate()
        {
            if (TotalMinesCount == 0) return 0;
            var failures = Math.Max(0, TotalMinesCount - ConsecutiveSuccessfulMines);
            return ((double)(TotalMinesCount - failures) / TotalMinesCount) * 100;
        }

        public int GetMiningStreak()
        {
            return ConsecutiveSuccessfulMines;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Core Enums - Fixed with all required values
    public enum Difficulty
    {
        NOVICE,
        EXPERIENCED,
        EXPERT,
        LEGENDARY
    }

    public enum Rank
    {
        BEGINNER,           // Added missing BEGINNER
        NOVICE_MINER,
        INTERMEDIATE,       // Added missing INTERMEDIATE  
        EXPERIENCED_MINER,
        EXPERT,             // Added missing EXPERT
        EXPERT_MINER,
        MASTER_MINER,
        LEGENDARY_MINER,
        ASCENDED_MINER
    }

    #endregion

    #region Mining Session and Results System

    public class MiningSession : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = "Valinor-70";
        public string LocationId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double RiskMultiplier { get; set; } = 1.0;
        public int StaminaCost { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
        public List<MineResult> Results { get; set; } = new();
        public Dictionary<string, object> SessionData { get; set; } = new();

        // Added missing RiskLevel property
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
        public DateTime Timestamp { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        public WeatherCondition WeatherAtTime { get; set; } = WeatherCondition.Clear;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

    #endregion

    #region Special Events System

    public class SpecialEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🌟";
        public DateTime StartDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime EndDate { get; set; } = DateTime.Parse("2025-06-07 21:41:31");
        public bool IsActive { get; set; } = true;
        public EventType Type { get; set; } = EventType.Bonus;
        public Dictionary<string, double> Effects { get; set; } = new();
        public List<string> AffectedLocations { get; set; } = new();
        public string Rarity { get; set; } = "Common";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum EventType
    {
        Bonus,
        Penalty,
        Special,
        Market,
        Weather,
        Quantum,
        Temporal
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
        public DateTime DiscoveredDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string DiscoveredBy { get; set; } = "Valinor-70";

        // Market Properties
        public double CurrentMarketMultiplier { get; set; } = 1.0;
        public string MarketTrend { get; set; } = "Stable";
        public DateTime LastPriceUpdate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");

        // Added missing methods
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
                1 => 0.1,  // Low volatility for common
                2 => 0.15,
                3 => 0.25,
                4 => 0.35,
                5 => 0.45, // High volatility for legendary
                6 => 0.55,
                7 => 0.65, // Extreme volatility for quantum
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
        GANGUE
    }

    #endregion

    #region Mining Location System

    public class MiningLocation : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🏔️";
        public string RequiredRank { get; set; } = "BEGINNER";
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
        public DateTime LastVisited { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public int TimesVisited { get; set; } = 0;
        public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;
        public Dictionary<string, int> MineralDiscoveryCount { get; set; } = new();

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
        public DateTime AcquiredDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public int TimesUsed { get; set; } = 0;
        public List<string> RequiredSkills { get; set; } = new();
        public Dictionary<string, string> UpgradeTree { get; set; } = new();
        public string Manufacturer { get; set; } = "Mining Corp";
        public double EfficiencyRating { get; set; } = 1.0;

        // Added missing property
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum EquipmentType
    {
        Pickaxe,
        Helmet,
        Lantern,
        Armor,
        Scanner,
        Drill,
        Refinery,
        Transport
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
        public string Category { get; set; } = "General";
        public string Difficulty { get; set; } = "Normal";

        // Enhanced Properties - Fixed missing properties
        public DateTime UnlockedDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime CompletedDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31"); // Added missing property
        public bool IsSecret { get; set; } = false;
        public string UnlockCondition { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; } = 0.0;
        public bool IsCompleted { get; set; } = false;
        public string CompletedBy { get; set; } = string.Empty;
        public List<string> Prerequisites { get; set; } = new();
        public Dictionary<string, object> ExtraRewards { get; set; } = new();
        public string RewardUpgrade { get; set; } = string.Empty; // Added missing property

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum AchievementType
    {
        TotalMoney,
        TotalMines,
        SpecificMineral,
        LocationUnlock,
        LocationMastery,
        ConsecutiveSuccess,
        SkillPoints,
        RankAdvancement,
        RiskLevel,
        EquipmentCollection
    }

    #endregion

    #region Market and Economy

    public class MarketData : INotifyPropertyChanged
    {
        public string MineralName { get; set; } = string.Empty;
        public double PriceMultiplier { get; set; } = 1.0;
        public string TrendIcon { get; set; } = "➡️";
        public string Trend { get; set; } = "Stable";
        public DateTime LastUpdated { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime LastUpdate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public List<double> PriceHistory { get; set; } = new();
        public string MarketSentiment { get; set; } = "Neutral";

        // Enhanced Properties
        public long Volume { get; set; } = 0;
        public double DailyHigh { get; set; } = 1.0;
        public double DailyLow { get; set; } = 1.0;
        public double Volatility { get; set; } = 0.0;
        public List<string> MarketNews { get; set; } = new();
        public Dictionary<string, double> RegionalPrices { get; set; } = new();
        public double Supply { get; set; } = 100.0;
        public double Demand { get; set; } = 100.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EconomicEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "📈";
        public DateTime StartDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public int DurationDays { get; set; } = 1;
        public int Duration { get; set; } = 1;
        public string Severity { get; set; } = "Normal";
        public Dictionary<string, double> MineralEffects { get; set; } = new();

        // Enhanced Properties
        public bool IsActive { get; set; } = true;
        public List<string> AffectedRegions { get; set; } = new();
        public string Source { get; set; } = "Global Markets";
        public double ImpactScale { get; set; } = 1.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Challenge and Reward System

    public class DailyChallenge : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ChallengeType Type { get; set; } = ChallengeType.Daily;
        public long Target { get; set; } = 0;
        public DateTime StartDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime EndDate { get; set; } = DateTime.Parse("2025-06-07 21:41:31");
        public DateTime ExpiryDate { get; set; } = DateTime.Parse("2025-06-07 21:41:31");
        public string Icon { get; set; } = "🎯";
        public string Difficulty { get; set; } = "Normal";
        public Reward Reward { get; set; } = new();
        public long RewardMoney { get; set; } = 0;
        public int RewardSkillPoints { get; set; } = 0;

        // Enhanced Properties
        public long Progress { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public string? RequiredMineral { get; set; }
        public string? RequiredLocation { get; set; }
        public DateTime CompletedDate { get; set; }
        public string CompletedBy { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Reward
    {
        public long Money { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public Dictionary<string, int> Equipment { get; set; } = new();
        public List<string> UnlockedFeatures { get; set; } = new();
        public DateTime GrantedDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
    }

    public enum ChallengeType
    {
        Daily,
        Weekly,
        Monthly,
        Special,
        Achievement
    }

    #endregion

    #region Skill System

    public class SkillNode : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int RequiredPoints { get; set; } = 0;
        public string Icon { get; set; } = "⚡";
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public List<string> Prerequisites { get; set; } = new();
        public string Effect { get; set; } = string.Empty;
        public Dictionary<string, double> Bonuses { get; set; } = new();
        public List<string> UnlockedAbilities { get; set; } = new();
        public bool IsUnlocked { get; set; } = false;

        // Enhanced Properties
        public DateTime UnlockedDate { get; set; }
        public string UnlockedBy { get; set; } = string.Empty;
        public int Tier { get; set; } = 1;
        public List<string> Synergies { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Story and Narrative System

    public class StoryEvent : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "📖";
        public List<string> Choices { get; set; } = new();
        public Dictionary<string, Action<Player>> ChoiceEffects { get; set; } = new();
        public string NarrativeText { get; set; } = string.Empty;

        // Enhanced Properties
        public bool IsCompleted { get; set; } = false;
        public DateTime TriggeredDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string SelectedChoice { get; set; } = string.Empty;
        public string StoryArc { get; set; } = "Main";
        public List<string> Prerequisites { get; set; } = new();
        public int Priority { get; set; } = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion

    #region Game State and Session Management

    public class GameSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string PlayerName { get; set; } = "Valinor-70";
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime LastActivity { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public Dictionary<string, object> SessionData { get; set; } = new();
        public List<string> ActionsPerformed { get; set; } = new();
        public bool IsActive { get; set; } = true;

        // Statistics
        public int MiningOperations { get; set; } = 0;
        public long MoneyEarned { get; set; } = 0;
        public int LocationsVisited { get; set; } = 0;
        public int AchievementsUnlocked { get; set; } = 0;
    }

    public class GameSettings
    {
        public string Theme { get; set; } = "Dark";
        public double AnimationSpeed { get; set; } = 1.0;
        public bool AutoSave { get; set; } = true;
        public bool ShowTutorials { get; set; } = true;
        public string Language { get; set; } = "English";
        public double MasterVolume { get; set; } = 0.8;
        public bool SoundEffects { get; set; } = true;
        public bool BackgroundMusic { get; set; } = true;
        public bool ReducedAnimations { get; set; } = false;
        public bool HighContrast { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string CurrentUser { get; set; } = "Valinor-70";
        public string Version { get; set; } = "1.0.0-alpha";
        public string GameMode { get; set; } = "Ultimate Edition";
        public string SessionStartTime { get; set; } = "2025-06-06 21:41:31";
        public int TotalPlaySessions { get; set; } = 1;
        public string PreferredDifficulty { get; set; } = "EXPERT";
    }

    #endregion

    #region Mining Operations and Results

    public class MiningOperation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = "Valinor-70";
        public string LocationId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double RiskMultiplier { get; set; } = 1.0;
        public int StaminaCost { get; set; } = 10;
        public bool IsSuccessful { get; set; } = false;
        public List<MineralDiscovery> Discoveries { get; set; } = new();
        public long TotalValue { get; set; } = 0;
        public int ExperienceGained { get; set; } = 0;
        public string FailureReason { get; set; } = string.Empty;
        public Dictionary<string, object> Conditions { get; set; } = new();
    }

    public class MineralDiscovery
    {
        public string MineralId { get; set; } = string.Empty;
        public string MineralName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public long IndividualValue { get; set; } = 0;
        public long TotalValue { get; set; } = 0;
        public string Quality { get; set; } = "Standard";
        public bool IsBonusDiscovery { get; set; } = false;
        public DateTime DiscoveredAt { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string LocationId { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class MiningResult
    {
        public bool IsSuccess { get; set; }
        public Mineral? Mineral { get; set; }
        public long Value { get; set; }
        public MiningLocation? Location { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExperienceGained { get; set; }
        public string? BonusDiscovery { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    #endregion

    #region Quantum Mining System (Advanced Features)

    public class QuantumMiningSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = "Valinor-70";
        public DateTime StartTime { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string QuantumState { get; set; } = "Stable";
        public double RealityStability { get; set; } = 100.0;
        public List<string> QuantumEffects { get; set; } = new();
        public Dictionary<string, double> DimensionalModifiers { get; set; } = new();
        public bool QuantumTunnelActive { get; set; } = false;
        public double TemporalFlux { get; set; } = 0.0;
        public List<QuantumDiscovery> QuantumDiscoveries { get; set; } = new();
    }

    public class QuantumDiscovery
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "Void Crystal";
        public string Description { get; set; } = string.Empty;
        public long Value { get; set; } = 0;
        public bool DefiesPhysics { get; set; } = false;
        public Dictionary<string, object> QuantumProperties { get; set; } = new();
        public DateTime DiscoveredAt { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public string DimensionalOrigin { get; set; } = "Primary Reality";
    }

    #endregion

    #region Statistics and Analytics

    public class PlayerStatistics
    {
        public string PlayerId { get; set; } = "Valinor-70";
        public DateTime FirstLogin { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public DateTime LastLogin { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
        public int TotalSessions { get; set; } = 0;
        public long TotalMoneyEarned { get; set; } = 0;
        public long TotalMiningOperations { get; set; } = 0;
        public double AverageSuccessRate { get; set; } = 0.0;
        public int TotalAchievements { get; set; } = 0;
        public int LocationsUnlocked { get; set; } = 0;
        public int SkillsUnlocked { get; set; } = 0;
        public Dictionary<string, long> MineralCollection { get; set; } = new();
        public Dictionary<string, int> LocationVisits { get; set; } = new();
        public List<string> MilestonesReached { get; set; } = new();
        public DateTime LastStatUpdate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
    }

    public class GameAnalytics
    {
        public DateTime AnalysisDate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public int ActivePlayers { get; set; } = 1;
        public long TotalMiningOperations { get; set; } = 0;
        public Dictionary<string, int> PopularLocations { get; set; } = new();
        public Dictionary<string, long> MineralMarketValues { get; set; } = new();
        public Dictionary<string, double> DifficultyDistribution { get; set; } = new();
        public double AverageSessionLength { get; set; } = 0.0;
        public List<string> TrendingAchievements { get; set; } = new();
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    }

    #endregion

    #region Notification and UI Systems

    public class GameNotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = "ℹ️";
        public NotificationType Type { get; set; } = NotificationType.Info;
        public DateTime CreatedAt { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public TimeSpan DisplayDuration { get; set; } = TimeSpan.FromSeconds(5);
        public bool IsRead { get; set; } = false;
        public Dictionary<string, object> Data { get; set; } = new();
        public string TargetPlayer { get; set; } = "Valinor-70";
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Achievement,
        Challenge,
        Market,
        System
    }

    public class UIState
    {
        public string CurrentTab { get; set; } = "Mine";
        public string CurrentLocation { get; set; } = "quantum_realm";
        public double CurrentRiskMultiplier { get; set; } = 2.3;
        public bool IsLoading { get; set; } = false;
        public string LoadingMessage { get; set; } = string.Empty;
        public List<GameNotification> ActiveNotifications { get; set; } = new();
        public Dictionary<string, bool> TabVisibility { get; set; } = new();
        public DateTime LastUIUpdate { get; set; } = DateTime.Parse("2025-06-06 21:41:31");
        public Dictionary<string, object> UIPreferences { get; set; } = new();
    }

    #endregion
}