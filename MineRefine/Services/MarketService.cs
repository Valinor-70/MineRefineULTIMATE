using MineRefine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MineRefine.Services
{
    public class MarketService
    {
        private readonly Dictionary<string, MarketData> _mineralPrices = new();
        private readonly List<EconomicEvent> _activeEvents = new();
        private readonly Random _random = new();
        private DateTime _lastUpdate = DateTime.Parse("2025-06-06 11:00:34");

        public MarketService()
        {
            InitializeMarketData();
            InitializeEconomicEvents();
        }

        private void InitializeMarketData()
        {
            var minerals = new[]
            {
                "Gold", "Silver", "Copper", "Diamond", "Platinum", "Palladium", "Rhodium",
                "Iridium", "Ruby", "Emerald", "Sapphire", "Magnetite", "Iron", "Coal",
                "Void Crystal", "Antimatter Fragment", "Quantum Ore", "Crystal Shard",
                "Refined Element", "Unrefined Element", "Osmium", "Ruthenium", "Quartz",
                "Calcite", "Dolomite", "Feldspar", "Gypsum", "Barite", "Fluorite"
            };

            foreach (var mineral in minerals)
            {
                var trend = (_random.NextDouble() - 0.5) * 2.0; // -1.0 to 1.0
                var trendIcon = trend > 0.1 ? "📈" : trend < -0.1 ? "📉" : "➡️";
                var trendText = trend > 0.1 ? "Rising" : trend < -0.1 ? "Falling" : "Stable";
                var sentiment = trend > 0.3 ? "Bullish" : trend < -0.3 ? "Bearish" : "Neutral";

                _mineralPrices[mineral] = new MarketData
                {
                    MineralName = mineral,
                    PriceMultiplier = 1.0 + (trend * 0.3), // 0.7 to 1.3 multiplier
                    Trend = trendText,
                    TrendIcon = trendIcon,
                    LastUpdated = DateTime.Parse("2025-06-06 11:00:34"),
                    LastUpdate = DateTime.Parse("2025-06-06 11:00:34"),
                    MarketSentiment = sentiment,
                    PriceHistory = GenerateHistoricalPrices()
                };
            }
        }

        private void InitializeEconomicEvents()
        {
            var currentTime = DateTime.Parse("2025-06-06 11:00:34");

            // Add some active events
            _activeEvents.AddRange(new List<EconomicEvent>
            {
                new()
                {
                    Id = "global_tech_surge_2025",
                    Name = "Global Technology Surge",
                    Description = "Massive investment in quantum computing and AI drives demand for rare minerals",
                    Icon = "🚀",
                    StartDate = currentTime.AddHours(-8),
                    DurationDays = 5,
                    Duration = 5,
                    Severity = "High",
                    MineralEffects = new()
                    {
                        { "Quantum Ore", 1.45 },
                        { "Refined Element", 1.35 },
                        { "Platinum", 1.25 },
                        { "Palladium", 1.20 }
                    }
                },
                new()
                {
                    Id = "mining_labor_shortage_2025",
                    Name = "Global Mining Labor Shortage",
                    Description = "Worldwide shortage of skilled miners increases operational costs",
                    Icon = "⚠️",
                    StartDate = currentTime.AddHours(-18),
                    DurationDays = 7,
                    Duration = 7,
                    Severity = "Critical",
                    MineralEffects = new()
                    {
                        { "Gold", 1.15 },
                        { "Silver", 1.12 },
                        { "Copper", 1.18 },
                        { "Iron", 1.10 }
                    }
                },
                new()
                {
                    Id = "renewable_energy_boom_2025",
                    Name = "Renewable Energy Investment Boom",
                    Description = "Massive global shift to renewable energy increases demand for specific minerals",
                    Icon = "🌱",
                    StartDate = currentTime.AddHours(-4),
                    DurationDays = 10,
                    Duration = 10,
                    Severity = "Normal",
                    MineralEffects = new()
                    {
                        { "Magnetite", 1.30 },
                        { "Copper", 1.22 },
                        { "Silver", 1.18 }
                    }
                }
            });
        }

        private List<double> GenerateHistoricalPrices()
        {
            var history = new List<double>();
            var basePrice = 1.0;

            for (int i = 0; i < 24; i++) // 24 hours of history
            {
                var change = (_random.NextDouble() - 0.5) * 0.1; // ±5% change
                basePrice = Math.Max(0.5, Math.Min(2.0, basePrice + change));
                history.Add(basePrice);
            }

            return history;
        }

        public void UpdateMarket()
        {
            var currentTime = DateTime.Parse("2025-06-06 11:00:34");
            if ((currentTime - _lastUpdate).TotalMinutes < 30) return; // Update every 30 minutes

            foreach (var kvp in _mineralPrices)
            {
                var marketData = kvp.Value;

                // Base random market fluctuation
                var change = (_random.NextDouble() - 0.5) * 0.08; // ±4% base change

                // Apply economic events effects
                var eventMultiplier = 1.0;
                foreach (var economicEvent in _activeEvents.Where(e => e.IsActive))
                {
                    if (economicEvent.MineralEffects.ContainsKey(kvp.Key))
                    {
                        eventMultiplier *= economicEvent.MineralEffects[kvp.Key];
                    }
                }

                // Calculate new price with bounds
                var newPrice = marketData.PriceMultiplier * (1.0 + change) * eventMultiplier;
                marketData.PriceMultiplier = Math.Max(0.3, Math.Min(3.0, newPrice));

                // Update trend information
                var trendValue = newPrice - marketData.PriceMultiplier;
                marketData.Trend = trendValue > 0.05 ? "Rising" : trendValue < -0.05 ? "Falling" : "Stable";
                marketData.TrendIcon = trendValue > 0.05 ? "📈" : trendValue < -0.05 ? "📉" : "➡️";
                marketData.MarketSentiment = trendValue > 0.15 ? "Bullish" : trendValue < -0.15 ? "Bearish" : "Neutral";

                marketData.LastUpdated = currentTime;
                marketData.LastUpdate = currentTime;

                // Update price history (keep last 24 data points)
                marketData.PriceHistory.Add(marketData.PriceMultiplier);
                if (marketData.PriceHistory.Count > 24)
                    marketData.PriceHistory.RemoveAt(0);
            }

            // Check for new economic events
            TriggerRandomEconomicEvents();

            // Remove expired events
            _activeEvents.RemoveAll(e => !e.IsActive);

            _lastUpdate = currentTime;
        }

        private void TriggerRandomEconomicEvents()
        {
            if (_random.NextDouble() < 0.03) // 3% chance per update
            {
                var events = GetPossibleEconomicEvents();
                var selectedEvent = events[_random.Next(events.Count)];
                selectedEvent.StartDate = DateTime.Parse("2025-06-06 11:00:34");
                _activeEvents.Add(selectedEvent);
            }
        }

        private List<EconomicEvent> GetPossibleEconomicEvents()
        {
            var currentTime = DateTime.Parse("2025-06-06 11:00:34");

            return new List<EconomicEvent>
            {
                new()
                {
                    Id = "diamond_mine_discovery_2025",
                    Name = "Major Diamond Mine Discovery",
                    Description = "Huge diamond deposit found in unexplored regions, flooding the market!",
                    Icon = "💎",
                    StartDate = currentTime,
                    DurationDays = 4,
                    Duration = 4,
                    Severity = "High",
                    MineralEffects = new() { { "Diamond", 0.75 }, { "Ruby", 0.85 }, { "Emerald", 0.90 } }
                },
                new()
                {
                    Id = "space_mining_announcement_2025",
                    Name = "Space Mining Program Announced",
                    Description = "First commercial space mining mission targeting asteroid belt minerals!",
                    Icon = "🚀",
                    StartDate = currentTime,
                    DurationDays = 6,
                    Duration = 6,
                    Severity = "Critical",
                    MineralEffects = new()
                    {
                        { "Platinum", 1.50 },
                        { "Iridium", 1.40 },
                        { "Rhodium", 1.35 },
                        { "Void Crystal", 1.25 }
                    }
                },
                new()
                {
                    Id = "quantum_computer_breakthrough_2025",
                    Name = "Quantum Computer Breakthrough",
                    Description = "Revolutionary quantum computing advancement requires rare earth elements!",
                    Icon = "⚛️",
                    StartDate = currentTime,
                    DurationDays = 8,
                    Duration = 8,
                    Severity = "High",
                    MineralEffects = new()
                    {
                        { "Quantum Ore", 1.60 },
                        { "Refined Element", 1.45 },
                        { "Antimatter Fragment", 1.35 }
                    }
                },
                new()
                {
                    Id = "global_recession_warning_2025",
                    Name = "Global Economic Recession Warning",
                    Description = "Economic analysts warn of potential global recession affecting commodity markets",
                    Icon = "📉",
                    StartDate = currentTime,
                    DurationDays = 12,
                    Duration = 12,
                    Severity = "Critical",
                    MineralEffects = new()
                    {
                        { "Gold", 0.80 },
                        { "Silver", 0.75 },
                        { "Copper", 0.70 },
                        { "Iron", 0.72 },
                        { "Coal", 0.68 }
                    }
                },
                new()
                {
                    Id = "green_energy_mandate_2025",
                    Name = "Global Green Energy Mandate",
                    Description = "World governments mandate transition to green energy, boosting mineral demand",
                    Icon = "🌍",
                    StartDate = currentTime,
                    DurationDays = 15,
                    Duration = 15,
                    Severity = "Normal",
                    MineralEffects = new()
                    {
                        { "Magnetite", 1.35 },
                        { "Copper", 1.30 },
                        { "Silver", 1.25 },
                        { "Quartz", 1.20 }
                    }
                },
                new()
                {
                    Id = "volcanic_eruption_disruption_2025",
                    Name = "Volcanic Eruption Disrupts Mining",
                    Description = "Major volcanic activity disrupts mining operations in key regions",
                    Icon = "🌋",
                    StartDate = currentTime,
                    DurationDays = 5,
                    Duration = 5,
                    Severity = "High",
                    MineralEffects = new()
                    {
                        { "Platinum", 1.25 },
                        { "Palladium", 1.20 },
                        { "Rhodium", 1.18 },
                        { "Iridium", 1.15 }
                    }
                }
            };
        }

        public MarketData GetMarketData(string mineralName)
        {
            UpdateMarket();
            return _mineralPrices.GetValueOrDefault(mineralName, new MarketData
            {
                MineralName = mineralName,
                PriceMultiplier = 1.0,
                Trend = "Stable",
                TrendIcon = "➡️",
                LastUpdated = DateTime.Parse("2025-06-06 11:00:34"),
                LastUpdate = DateTime.Parse("2025-06-06 11:00:34"),
                MarketSentiment = "Neutral",
                PriceHistory = new List<double> { 1.0 }
            });
        }

        public Dictionary<string, MarketData> GetAllMarketData()
        {
            UpdateMarket();
            return new Dictionary<string, MarketData>(_mineralPrices);
        }

        public List<EconomicEvent> GetActiveEvents()
        {
            var currentTime = DateTime.Parse("2025-06-06 11:00:34");
            return _activeEvents.Where(e => e.IsActive).ToList();
        }

        public List<EconomicEvent> GetAllEvents()
        {
            return new List<EconomicEvent>(_activeEvents);
        }

        public long ApplyMarketPrice(string mineralName, long basePrice)
        {
            var marketData = GetMarketData(mineralName);
            var finalPrice = (long)(basePrice * marketData.PriceMultiplier);
            return Math.Max(1, finalPrice); // Ensure minimum price of 1
        }

        public double GetMarketMultiplier(string mineralName)
        {
            var marketData = GetMarketData(mineralName);
            return marketData.PriceMultiplier;
        }

        public string GetMarketTrend(string mineralName)
        {
            var marketData = GetMarketData(mineralName);
            return $"{marketData.TrendIcon} {marketData.Trend}";
        }

        public string GetMarketSummary()
        {
            var activeEvents = GetActiveEvents();
            var marketCondition = activeEvents.Count switch
            {
                0 => "Stable market conditions",
                1 => "Minor market fluctuations",
                2 => "Moderate market volatility",
                3 => "High market volatility",
                _ => "Extreme market conditions"
            };

            var avgMultiplier = _mineralPrices.Values.Average(m => m.PriceMultiplier);
            var marketSentiment = avgMultiplier > 1.1 ? "Bullish" : avgMultiplier < 0.9 ? "Bearish" : "Neutral";

            return $"{marketCondition} - Overall sentiment: {marketSentiment}";
        }

        public void ForceMarketUpdate()
        {
            _lastUpdate = DateTime.Parse("2025-06-06 10:30:34"); // Force next update
            UpdateMarket();
        }

        public MarketData GetMineralMarketInfo(string mineralName)
        {
            return GetMarketData(mineralName);
        }

        public bool IsMarketVolatile()
        {
            var avgVolatility = _mineralPrices.Values
                .Where(m => m.PriceHistory.Count > 1)
                .Average(m => CalculateVolatility(m.PriceHistory));

            return avgVolatility > 0.1; // 10% volatility threshold
        }

        private double CalculateVolatility(List<double> priceHistory)
        {
            if (priceHistory.Count < 2) return 0;

            var changes = new List<double>();
            for (int i = 1; i < priceHistory.Count; i++)
            {
                var change = Math.Abs(priceHistory[i] - priceHistory[i - 1]) / priceHistory[i - 1];
                changes.Add(change);
            }

            return changes.Average();
        }
    }
}