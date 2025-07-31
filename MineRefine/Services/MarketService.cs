using System;
using System.Collections.Generic;
using System.Linq;
using MineRefine.Models;

namespace MineRefine.Services
{
    public class MarketService
    {
        // Constants - Updated to current timestamp
        private const string CURRENT_DATETIME = "2025-07-31 13:29:22";
        private const string CURRENT_USER = "Valinor-70";

        private readonly Random _random;
        private readonly List<MarketData> _marketData;
        private readonly List<EconomicEvent> _economicEvents;
        private readonly List<SpecialEvent> _specialEvents;

        public MarketService()
        {
            _random = new Random();
            _marketData = InitializeMarketData();
            _economicEvents = new List<EconomicEvent>();
            _specialEvents = new List<SpecialEvent>();
        }

        #region Market Data Management

        // ADDED: Missing GetMarketData method
        public List<MarketData> GetMarketData()
        {
            return _marketData.ToList();
        }

        public List<MarketData> GetCurrentMarketData()
        {
            return _marketData.ToList();
        }

        public MarketData? GetMineralMarketData(string mineralId)
        {
            return _marketData.FirstOrDefault(m => m.MineralId == mineralId);
        }

        public void UpdateMarketPrices()
        {
            foreach (var data in _marketData)
            {
                // Generate price change based on volatility
                var changePercent = (_random.NextDouble() - 0.5) * 2 * data.Volatility * 100;

                // Apply economic events
                foreach (var economicEvent in _economicEvents.Where(e => e.IsCurrentlyActive()))
                {
                    if (economicEvent.AffectedMinerals.Contains(data.MineralId))
                    {
                        var eventMultiplier = economicEvent.GetMineralPriceMultiplier(data.MineralId);
                        changePercent *= eventMultiplier;
                    }
                }

                data.UpdatePrice(changePercent);
            }
        }

        private List<MarketData> InitializeMarketData()
        {
            return new List<MarketData>
            {
                new MarketData
                {
                    MineralId = "copper",
                    MineralName = "Copper Ore",
                    CurrentPrice = 10,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 1000,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.1,
                    Supply = 100.0,
                    Demand = 100.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                },
                new MarketData
                {
                    MineralId = "iron",
                    MineralName = "Iron Ore",
                    CurrentPrice = 20,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 800,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.12,
                    Supply = 95.0,
                    Demand = 105.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                },
                new MarketData
                {
                    MineralId = "silver",
                    MineralName = "Silver Ore",
                    CurrentPrice = 50,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 500,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.2,
                    Supply = 80.0,
                    Demand = 120.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                },
                new MarketData
                {
                    MineralId = "gold",
                    MineralName = "Gold Ore",
                    CurrentPrice = 100,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 200,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.25,
                    Supply = 70.0,
                    Demand = 130.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                },
                new MarketData
                {
                    MineralId = "diamond",
                    MineralName = "Diamond",
                    CurrentPrice = 500,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 50,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.4,
                    Supply = 60.0,
                    Demand = 140.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                },
                new MarketData
                {
                    MineralId = "quantum_crystal",
                    MineralName = "Quantum Crystal",
                    CurrentPrice = 10000,
                    PriceChange = 0.0,
                    Trend = "Stable",
                    Volume = 5,
                    LastUpdate = DateTime.Parse(CURRENT_DATETIME),
                    LastUpdated = DateTime.Parse(CURRENT_DATETIME),
                    Volatility = 0.6,
                    Supply = 30.0,
                    Demand = 170.0,
                    PriceMultiplier = 1.0,
                    TrendIcon = "📊",
                    MarketSentiment = "Neutral"
                }
            };
        }

        #endregion

        #region Economic Events

        public void GenerateRandomEconomicEvent()
        {
            if (_random.NextDouble() < 0.1) // 10% chance per update
            {
                var eventTemplates = GetEconomicEventTemplates();
                var template = eventTemplates[_random.Next(eventTemplates.Count)];

                var economicEvent = new EconomicEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = template.Name,
                    Description = template.Description,
                    StartTime = DateTime.Parse(CURRENT_DATETIME),
                    StartDate = DateTime.Parse(CURRENT_DATETIME),
                    Duration = TimeSpan.FromDays(template.DurationDays),
                    DurationDays = template.DurationDays,
                    EventType = template.EventType,
                    Severity = template.Severity,
                    IsActive = true,
                    IsGlobal = true,
                    AffectedMinerals = new List<string>(template.AffectedMinerals),
                    PriceMultipliers = new Dictionary<string, double>(template.PriceMultipliers),
                    MineralEffects = new Dictionary<string, double>(template.MineralEffects),
                    Icon = template.Icon,
                    IconEmoji = template.Icon
                };

                economicEvent.EndTime = economicEvent.StartTime.Add(economicEvent.Duration);
                _economicEvents.Add(economicEvent);
            }
        }

        private List<EconomicEvent> GetEconomicEventTemplates()
        {
            return new List<EconomicEvent>
            {
                new EconomicEvent
                {
                    Name = "Copper Shortage",
                    Description = "Industrial demand has caused a global copper shortage",
                    DurationDays = 3,
                    EventType = "Supply Shortage",
                    Severity = "Major",
                    AffectedMinerals = new List<string> { "copper" },
                    PriceMultipliers = new Dictionary<string, double> { { "copper", 1.5 } },
                    MineralEffects = new Dictionary<string, double> { { "copper", 1.5 } },
                    Icon = "⚠️"
                },
                new EconomicEvent
                {
                    Name = "Gold Rush Discovery",
                    Description = "New gold deposits discovered, flooding the market",
                    DurationDays = 5,
                    EventType = "Market Flood",
                    Severity = "Minor",
                    AffectedMinerals = new List<string> { "gold", "silver" },
                    PriceMultipliers = new Dictionary<string, double> { { "gold", 0.8 }, { "silver", 0.9 } },
                    MineralEffects = new Dictionary<string, double> { { "gold", 0.8 }, { "silver", 0.9 } },
                    Icon = "💰"
                },
                new EconomicEvent
                {
                    Name = "Quantum Tech Boom",
                    Description = "Breakthrough in quantum technology increases demand for quantum crystals",
                    DurationDays = 7,
                    EventType = "Technology Boom",
                    Severity = "Major",
                    AffectedMinerals = new List<string> { "quantum_crystal" },
                    PriceMultipliers = new Dictionary<string, double> { { "quantum_crystal", 2.0 } },
                    MineralEffects = new Dictionary<string, double> { { "quantum_crystal", 2.0 } },
                    Icon = "🚀"
                },
                new EconomicEvent
                {
                    Name = "Diamond Market Crash",
                    Description = "Synthetic diamond production causes natural diamond prices to plummet",
                    DurationDays = 4,
                    EventType = "Market Crash",
                    Severity = "Major",
                    AffectedMinerals = new List<string> { "diamond" },
                    PriceMultipliers = new Dictionary<string, double> { { "diamond", 0.6 } },
                    MineralEffects = new Dictionary<string, double> { { "diamond", 0.6 } },
                    Icon = "📉"
                },
                new EconomicEvent
                {
                    Name = "Infrastructure Boom",
                    Description = "Global infrastructure projects increase demand for metals",
                    DurationDays = 10,
                    EventType = "Economic Boom",
                    Severity = "Minor",
                    AffectedMinerals = new List<string> { "iron", "copper" },
                    PriceMultipliers = new Dictionary<string, double> { { "iron", 1.3 }, { "copper", 1.2 } },
                    MineralEffects = new Dictionary<string, double> { { "iron", 1.3 }, { "copper", 1.2 } },
                    Icon = "🏗️"
                },
                new EconomicEvent
                {
                    Name = "Luxury Market Surge",
                    Description = "Economic prosperity increases demand for luxury items",
                    DurationDays = 6,
                    EventType = "Luxury Boom",
                    Severity = "Minor",
                    AffectedMinerals = new List<string> { "gold", "silver", "diamond" },
                    PriceMultipliers = new Dictionary<string, double> { { "gold", 1.2 }, { "silver", 1.15 }, { "diamond", 1.25 } },
                    MineralEffects = new Dictionary<string, double> { { "gold", 1.2 }, { "silver", 1.15 }, { "diamond", 1.25 } },
                    Icon = "💎"
                }
            };
        }

        public List<EconomicEvent> GetActiveEconomicEvents()
        {
            return _economicEvents.Where(e => e.IsCurrentlyActive()).ToList();
        }

        public void RemoveExpiredEvents()
        {
            _economicEvents.RemoveAll(e => !e.IsCurrentlyActive());
            _specialEvents.RemoveAll(e => !e.IsCurrentlyActive());
        }

        #endregion

        #region Special Events

        public void GenerateRandomSpecialEvent()
        {
            if (_random.NextDouble() < 0.05) // 5% chance per update
            {
                var eventTemplates = GetSpecialEventTemplates();
                var template = eventTemplates[_random.Next(eventTemplates.Count)];

                var specialEvent = new SpecialEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = template.Name,
                    Description = template.Description,
                    StartTime = DateTime.Parse(CURRENT_DATETIME),
                    StartDate = DateTime.Parse(CURRENT_DATETIME),
                    Duration = TimeSpan.FromDays(7),
                    EventType = template.EventType,
                    Rarity = template.Rarity,
                    IsActive = true,
                    IsRepeatable = template.IsRepeatable,
                    Bonuses = new Dictionary<string, double>(template.Bonuses),
                    Effects = new Dictionary<string, double>(template.Effects),
                    UnlockedContent = new List<string>(template.UnlockedContent),
                    RequiredConditions = new List<string>(template.RequiredConditions),
                    CompletionCriteria = template.CompletionCriteria,
                    Icon = template.Icon,
                    IconEmoji = template.Icon,
                    BackgroundColor = template.BackgroundColor,
                    AffectedLocations = new List<string>(template.AffectedLocations),
                    Type = template.Type
                };

                specialEvent.EndTime = specialEvent.StartTime.Add(specialEvent.Duration);
                specialEvent.EndDate = specialEvent.EndTime;
                _specialEvents.Add(specialEvent);
            }
        }

        private List<SpecialEvent> GetSpecialEventTemplates()
        {
            return new List<SpecialEvent>
            {
                new SpecialEvent
                {
                    Name = "Lunar Eclipse Mining Bonus",
                    Description = "The lunar eclipse enhances mineral visibility, granting a temporary mining bonus",
                    EventType = "Celestial",
                    Rarity = "Rare",
                    IsRepeatable = true,
                    Bonuses = new Dictionary<string, double> { { "success_rate", 0.2 }, { "rare_find_chance", 0.15 } },
                    Effects = new Dictionary<string, double> { { "mining_speed", 1.3 } },
                    UnlockedContent = new List<string>(),
                    RequiredConditions = new List<string>(),
                    CompletionCriteria = "Mine 50 successful operations during the eclipse",
                    Icon = "🌙",
                    BackgroundColor = "#2F1B69",
                    AffectedLocations = new List<string> { "surface_mine", "deep_caves" },
                    Type = Models.EventType.Special
                },
                new SpecialEvent
                {
                    Name = "Quantum Resonance Week",
                    Description = "Reality fluctuations make quantum materials more abundant",
                    EventType = "Quantum",
                    Rarity = "Epic",
                    IsRepeatable = false,
                    Bonuses = new Dictionary<string, double> { { "quantum_find_chance", 0.5 } },
                    Effects = new Dictionary<string, double> { { "quantum_crystal_spawn", 2.0 } },
                    UnlockedContent = new List<string> { "quantum_scanner_blueprint" },
                    RequiredConditions = new List<string> { "has_quantum_access" },
                    CompletionCriteria = "Discover 5 quantum crystals",
                    Icon = "🌌",
                    BackgroundColor = "#4B0082",
                    AffectedLocations = new List<string> { "quantum_realm" },
                    Type = Models.EventType.Special
                },
                new SpecialEvent
                {
                    Name = "Miner's Festival",
                    Description = "The annual celebration of mining brings community bonuses",
                    EventType = "Seasonal",
                    Rarity = "Common",
                    IsRepeatable = true,
                    Bonuses = new Dictionary<string, double> { { "experience_gain", 0.5 }, { "skill_point_gain", 0.3 } },
                    Effects = new Dictionary<string, double> { { "stamina_cost_reduction", 0.25 } },
                    UnlockedContent = new List<string> { "festival_pickaxe" },
                    RequiredConditions = new List<string>(),
                    CompletionCriteria = "Participate in 100 mining operations",
                    Icon = "🎉",
                    BackgroundColor = "#FFD700",
                    AffectedLocations = new List<string> { "surface_mine", "deep_caves", "volcanic_depths" },
                    Type = Models.EventType.Seasonal
                }
            };
        }

        public List<SpecialEvent> GetActiveSpecialEvents()
        {
            return _specialEvents.Where(e => e.IsCurrentlyActive()).ToList();
        }

        #endregion

        #region Market Analysis

        public Dictionary<string, object> GetMarketAnalysis()
        {
            var totalVolume = _marketData.Sum(m => m.Volume);
            var averageChange = _marketData.Average(m => m.PriceChange);
            var highestGainer = _marketData.OrderByDescending(m => m.PriceChange).FirstOrDefault();
            var biggestLoser = _marketData.OrderBy(m => m.PriceChange).FirstOrDefault();

            return new Dictionary<string, object>
            {
                { "total_volume", totalVolume },
                { "average_change", averageChange },
                { "highest_gainer", highestGainer?.MineralName ?? "None" },
                { "highest_gain_percent", highestGainer?.PriceChange ?? 0.0 },
                { "biggest_loser", biggestLoser?.MineralName ?? "None" },
                { "biggest_loss_percent", biggestLoser?.PriceChange ?? 0.0 },
                { "active_events", _economicEvents.Count(e => e.IsCurrentlyActive()) },
                { "special_events", _specialEvents.Count(e => e.IsCurrentlyActive()) },
                { "market_sentiment", GetOverallMarketSentiment() },
                { "last_update", DateTime.Parse(CURRENT_DATETIME) }
            };
        }

        private string GetOverallMarketSentiment()
        {
            var averageChange = _marketData.Average(m => m.PriceChange);

            return averageChange switch
            {
                > 5 => "Very Bullish",
                > 1 => "Bullish",
                > -1 => "Neutral",
                > -5 => "Bearish",
                _ => "Very Bearish"
            };
        }

        public List<MarketData> GetTopPerformers(int count = 5)
        {
            return _marketData
                .OrderByDescending(m => m.PriceChange)
                .Take(count)
                .ToList();
        }

        public List<MarketData> GetWorstPerformers(int count = 5)
        {
            return _marketData
                .OrderBy(m => m.PriceChange)
                .Take(count)
                .ToList();
        }

        #endregion

        #region Price Calculations

        public long CalculateSellValue(string mineralId, long baseValue, int quantity = 1)
        {
            var marketData = GetMineralMarketData(mineralId);
            if (marketData == null) return baseValue * quantity;

            var marketMultiplier = marketData.PriceMultiplier;
            var demandMultiplier = Math.Max(0.5, marketData.Demand / 100.0);
            var supplyMultiplier = Math.Max(0.5, 100.0 / marketData.Supply);

            var finalMultiplier = marketMultiplier * demandMultiplier * supplyMultiplier;
            return (long)(baseValue * finalMultiplier * quantity);
        }

        public void ProcessSale(string mineralId, int quantity)
        {
            var marketData = GetMineralMarketData(mineralId);
            if (marketData == null) return;

            // Increase supply slightly
            marketData.Supply += quantity * 0.1;

            // Decrease demand slightly
            marketData.Demand -= quantity * 0.05;

            // Update volume
            marketData.Volume += quantity;

            // Clamp values
            marketData.Supply = Math.Max(10, Math.Min(200, marketData.Supply));
            marketData.Demand = Math.Max(10, Math.Min(200, marketData.Demand));
        }

        #endregion

        #region Market Data Refresh

        public void RefreshMarketData()
        {
            UpdateMarketPrices();
            GenerateRandomEconomicEvent();
            GenerateRandomSpecialEvent();
            RemoveExpiredEvents();
        }

        public DateTime GetLastUpdateTime()
        {
            return DateTime.Parse(CURRENT_DATETIME);
        }

        public bool IsMarketOpen()
        {
            // Market is always open in this simulation
            return true;
        }

        public double GetMarketVolatility()
        {
            return _marketData.Average(m => m.Volatility);
        }

        public int GetActiveEventCount()
        {
            return _economicEvents.Count(e => e.IsCurrentlyActive()) +
                   _specialEvents.Count(e => e.IsCurrentlyActive());
        }

        #endregion
    }
}